using System;
using Xunit;
using LaOcaDataAccess;
using LaOcaService.DAOs.JugadorFolder;
using LaOcaService;
using System.Linq;
using System.Data.Entity;

namespace LaOcaTests.JugadorDAO
{
    public class TestJugadorDAO : IDisposable
    {
        private readonly LaOcaBDEntities _contexto;
        private readonly DbContextTransaction _transaccion;
        private readonly LaOcaService.DAOs.JugadorFolder.JugadorDAO _jugadorDAO;
        private int _idJugadorPrueba;

        public TestJugadorDAO()
        {
            _contexto = new LaOcaBDEntities();
            _transaccion = _contexto.Database.BeginTransaction();
            _jugadorDAO = new LaOcaService.DAOs.JugadorFolder.JugadorDAO(_contexto);
            PrepararBaseDeDatos();
        }

        private void PrepararBaseDeDatos()
        {
            var puntuacion = new Puntuaciones
            {
                casillasRecorridasGlobal = 0,
                monedasObtenidasGlobal = 0,
                partidasGanadasGlobal = 0,
                monedasActuales = 0
            };
            _contexto.Puntuaciones.Add(puntuacion);
            _contexto.SaveChanges();

            var cuenta = new Cuentas
            {
                correoElectronico = "prueba@correo.com",
                contrasena = "contrasenaPrueba"
            };
            _contexto.Cuentas.Add(cuenta);
            _contexto.SaveChanges();

            var jugador = new Jugadores
            {
                nombreUsuario = "usuarioPrueba",
                IdCuenta = cuenta.IdCuenta,
                IdFotoPerfil = 1,
                IdPuntuacion = puntuacion.IdPuntuacion
            };
            _contexto.Jugadores.Add(jugador);
            _contexto.SaveChanges();
            _idJugadorPrueba = jugador.IdJugador;
        }

        [Fact]
        public void PruebaCrearJugador()
        {
            var nuevoJugador = new Jugador
            {
                NombreUsuario = "nuevoUsuario",
                IdCuenta = _contexto.Cuentas.First().IdCuenta,
                IdFotoPerfil = 2,
                IdPuntuacion = 2
            };

            _jugadorDAO.CrearJugador(nuevoJugador, "imagen.jpg");

            var jugadorBD = _contexto.Jugadores.FirstOrDefault(j => j.nombreUsuario == "nuevoUsuario");
            Assert.True(jugadorBD != null && jugadorBD.nombreUsuario == "nuevoUsuario");
        }

        [Fact]
        public void PruebaModificarJugador()
        {
            var jugador = new Jugador
            {
                IdJugador = _idJugadorPrueba,
                NombreUsuario = "usuarioModificado",
                IdFotoPerfil = 1
            };

            _jugadorDAO.ModificarJugador(jugador);

            var jugadorBD = _contexto.Jugadores.Find(_idJugadorPrueba);
            Assert.True(jugadorBD != null && jugadorBD.nombreUsuario == "usuarioModificado");
        }

        [Fact]
        public void PruebaObtenerJugadorPorId()
        {
            var jugador = _jugadorDAO.ObtenerJugadorPorId(_idJugadorPrueba);
            Assert.True(jugador != null && jugador.NombreUsuario == "usuarioPrueba");
        }

        [Fact]
        public void PruebaNombreUsuarioExisteCrear()
        {
            var existe = _jugadorDAO.NombreUsuarioExisteCrear("usuarioPrueba");
            Assert.True(existe);
        }

        [Fact]
        public void PruebaNombreUsuarioExisteModificar()
        {
            var existe = _jugadorDAO.NombreUsuarioExisteModificar("usuarioPrueba", _idJugadorPrueba);
            Assert.False(existe);
        }

        public void Dispose()
        {
            _transaccion.Rollback();
            _transaccion.Dispose();
            _contexto.Dispose();
        }
    }
}
