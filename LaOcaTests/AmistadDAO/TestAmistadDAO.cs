using Xunit;
using System;
using LaOcaService;
using LaOcaDataAccess;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;

namespace LaOcaTests.AmistadDAO
{
    public class TestAmistadDAO : IDisposable
    {
        private readonly LaOcaBDEntities _contexto;
        private DbContextTransaction _transaccion;

        private LaOcaService.DAOs.AmistadFolder.AmistadDAO _amistadDAO;
        private int _idAmistad;
        private DateTime _fecha;
        private int _idJugadorSolicitante;
        private int _idJugadorReceptor;

        private int _idJugadorConListaAmistades;
        private bool _listaAmistadesCreada = false;
        private List<Amistades> _listaAmistadesDePrueba = new List<Amistades>();

        public TestAmistadDAO()
        {
            _contexto = new LaOcaBDEntities();
            _transaccion = _contexto.Database.BeginTransaction();
            _amistadDAO = new LaOcaService.DAOs.AmistadFolder.AmistadDAO(_contexto);

            PrepararBaseDeDatos();
        }

        private void PrepararBaseDeDatos()
        {
            Jugadores jugadorSolicitante = new Jugadores();
            Jugadores jugadorReceptor = new Jugadores();

            _contexto.Jugadores.Add(jugadorSolicitante);
            _contexto.Jugadores.Add(jugadorReceptor);
            _contexto.SaveChanges();

            _idJugadorSolicitante = jugadorSolicitante.IdJugador;
            _idJugadorReceptor = jugadorReceptor.IdJugador;

            _fecha = DateTime.Now;

            Amistades amistadPrueba = new Amistades
            {
                estado = "Solicitud",
                fecha = _fecha,
                IdJugadorSolicitante = _idJugadorSolicitante,
                IdJugadorReceptor = _idJugadorReceptor
            };
            _contexto.Amistades.Add(amistadPrueba);
            _contexto.SaveChanges();

            _idAmistad = amistadPrueba.IdAmistad;
        }

        private void CrearListaAmistades()
        {
            if (_listaAmistadesCreada)
            {
                Jugadores jugadorSolicitante2 = new Jugadores();
                _contexto.Jugadores.Add(jugadorSolicitante2);
                _contexto.SaveChanges();
                _idJugadorConListaAmistades = jugadorSolicitante2.IdJugador;

                Amistades primeraAmistadDeLista = new Amistades
                {
                    estado = "Solicitud",
                    fecha = DateTime.Now,
                    IdJugadorReceptor = _idJugadorConListaAmistades,
                    IdJugadorSolicitante = _idJugadorReceptor
                };
                _listaAmistadesDePrueba.Add(primeraAmistadDeLista);

                Amistades segundaAmistadDeLista = new Amistades
                {
                    estado = "Solicitud",
                    fecha = DateTime.Now,
                    IdJugadorReceptor = _idJugadorConListaAmistades,
                    IdJugadorSolicitante = _idJugadorSolicitante
                };
                _listaAmistadesDePrueba.Add(segundaAmistadDeLista);

                foreach (Amistades amistad in _listaAmistadesDePrueba)
                {
                    _contexto.Amistades.Add(amistad);
                }
                _contexto.SaveChanges();
            }
        }

        [Fact]
        public void PruebaRecuperarAmistadExitoso()
        {
            var amistadEsperada = new Amistad
            {
                IdAmistad = _idAmistad,
                Estado = "Solicitud",
                Fecha = _fecha,
                IdJugadorSolicitante = _idJugadorSolicitante,
                IdJugadorReceptor = _idJugadorReceptor
            };

            Amistad amistad = _amistadDAO.RecuperarAmistad(_idJugadorSolicitante, _idJugadorReceptor);

            Assert.True(amistadEsperada.Equals(amistad));
        }

        [Fact]
        public void PruebaRecuperarAmistadFallido()
        {
            var amistadEsperada = new Amistad();

            Amistad amistad = _amistadDAO.RecuperarAmistad(_idJugadorSolicitante, _idJugadorReceptor + 1);

            Assert.True(amistadEsperada.Equals(amistad));
        }

        [Fact]
        public void PruebaRegistrarNuevaAmistadExitoso()
        {
            Jugadores jugadorSolicitante = new Jugadores();
            Jugadores jugadorReceptor = new Jugadores();

            _contexto.Jugadores.Add(jugadorSolicitante);
            _contexto.Jugadores.Add(jugadorReceptor);
            _contexto.SaveChanges();

            DateTime fechaNuevaAmistad = DateTime.Now;

            Amistad nuevaAmistad = new Amistad
            {
                Estado = "Solicitud",
                Fecha = fechaNuevaAmistad,
                IdJugadorSolicitante = jugadorSolicitante.IdJugador,
                IdJugadorReceptor = jugadorReceptor.IdJugador
            };

            int idNuevaAmistadRegistrada = _amistadDAO.RegistrarNuevaAmistad(nuevaAmistad);

            Assert.True(idNuevaAmistadRegistrada > 0);
        }

        [Fact]
        public void PruebaRegistrarNuevaAmistadFallido()
        {
            Amistad amistadExistente = new Amistad
            {
                IdJugadorSolicitante = _idJugadorSolicitante,
                IdJugadorReceptor = _idJugadorReceptor
            };

            int idNuevaAmistadRegistrada = _amistadDAO.RegistrarNuevaAmistad(amistadExistente);

            Assert.True(idNuevaAmistadRegistrada == 0);
        }

        [Fact]
        public void PruebaActualizarEstadoAmistadExitoso()
        {
            Amistad amistadActualizada = new Amistad
            {
                IdAmistad = _idAmistad,
                Estado = "Solicitud",
                Fecha = DateTime.Now,
                IdJugadorReceptor = _idJugadorSolicitante,
                IdJugadorSolicitante = _idJugadorReceptor
            };

            int resultado = _amistadDAO.ActualizarEstadoAmistad(amistadActualizada);

            Assert.True(resultado == 1);
        }

        [Fact]
        public void PruebaActualizarEstadoAmistadFallido()
        {
            Amistad amistadActualizadaConIdQueNoExiste = new Amistad
            {
                IdAmistad = _idAmistad + 1000,
                Estado = "Amigos",
                Fecha = DateTime.Now
            };

            int resultado = _amistadDAO.ActualizarEstadoAmistad(amistadActualizadaConIdQueNoExiste);

            Assert.True(resultado == 0);
        }

        [Fact]
        public void PruebaRecuperarAmistadesDeJugadorExitoso()
        {
            CrearListaAmistades();

            List<Amistad> listaAmistadesEsperadas = new List<Amistad>();

            foreach (Amistades amistad in _listaAmistadesDePrueba)
            {
                listaAmistadesEsperadas.Add(new Amistad
                {
                    IdAmistad = amistad.IdAmistad,
                    Estado = amistad.estado,
                    Fecha = (DateTime)amistad.fecha,
                    IdJugadorSolicitante = (int)amistad.IdJugadorSolicitante,
                    IdJugadorReceptor = (int)amistad.IdJugadorReceptor
                });
            }

            List<Amistad> listaAmistades = _amistadDAO.RecuperarAmistadesDeJugador(_idJugadorConListaAmistades, "Solicitud");

            Assert.True(listaAmistadesEsperadas.OrderBy(a => a.IdAmistad).SequenceEqual(listaAmistades.OrderBy(a => a.IdAmistad)));
        }

        [Fact]
        public void PruebaRecuperarAmistadesDeJugadorFallido()
        {
            CrearListaAmistades();

            List<Amistad> listaAmistadesEsperadas = new List<Amistad>();

            foreach (Amistades amistad in _listaAmistadesDePrueba)
            {
                if (amistad.estado.Equals("Amigos"))
                {
                    listaAmistadesEsperadas.Add(new Amistad
                    {
                        IdAmistad = amistad.IdAmistad,
                        Estado = amistad.estado,
                        Fecha = (DateTime)amistad.fecha,
                        IdJugadorSolicitante = (int)amistad.IdJugadorSolicitante,
                        IdJugadorReceptor = (int)amistad.IdJugadorReceptor
                    });
                }
            }

            List<Amistad> listaAmistades = _amistadDAO.RecuperarAmistadesDeJugador(_idJugadorConListaAmistades, "Amigos");

            Assert.True(listaAmistadesEsperadas.OrderBy(a => a.IdAmistad).SequenceEqual(listaAmistades.OrderBy(a => a.IdAmistad)));
        }

        public void Dispose()
        {
            _transaccion.Rollback();
            _transaccion.Dispose();
            _contexto.Dispose();
        }
    }
}
