using Xunit;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.ServiceModel;
using LaOcaService;
using LaOcaDataAccess;

namespace LaOcaTests.JugadorDAO
{
    public class TestJugadorDAOExcepcion
    {
        private readonly LaOcaBDEntities _contexto;
        private LaOcaService.DAOs.JugadorFolder.JugadorDAO _jugadorDAO;

        public TestJugadorDAOExcepcion()
        {
            _contexto = new LaOcaBDEntities();
            _jugadorDAO = new LaOcaService.DAOs.JugadorFolder.JugadorDAO();
        }

        [Fact]
        public void PruebaCrearJugadorExcepcion()
        {
            Jugador jugador = new Jugador
            {
                IdCuenta = 999,
                IdFotoPerfil = 1,
                IdPuntuacion = 1,
                NombreUsuario = "UsuarioPrueba"
            };

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _jugadorDAO.CrearJugador(jugador, "referencia_imagen.jpg");
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }

        [Fact]
        public void PruebaModificarJugadorExcepcion()
        {
            Jugador jugador = new Jugador
            {
                IdJugador = 999,
                NombreUsuario = "UsuarioActualizado",
                IdFotoPerfil = 2
            };

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _jugadorDAO.ModificarJugador(jugador);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }

        [Fact]
        public void PruebaObtenerJugadorPorIdExcepcion()
        {
            int idJugadorInexistente = 999;

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _jugadorDAO.ObtenerJugadorPorId(idJugadorInexistente);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }

        [Fact]
        public void PruebaNombreUsuarioExisteExcepcion()
        {
            string nombreUsuarioInexistente = "NombreNoExistente";

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _jugadorDAO.NombreUsuarioExisteCrear(nombreUsuarioInexistente);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }

        [Fact]
        public void PruebaNombreUsuarioExisteModificarExcepcion()
        {
            string nombreUsuarioInexistente = "NombreNoExistente";
            int idJugadorInexistente = 999;

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _jugadorDAO.NombreUsuarioExisteModificar(nombreUsuarioInexistente, idJugadorInexistente);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }
    }
}