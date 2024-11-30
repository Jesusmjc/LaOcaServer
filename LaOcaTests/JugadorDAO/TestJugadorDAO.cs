using System;
using Xunit;
using LaOcaDataAccess;
using LaOcaService.DAOs.JugadorFolder;
using LaOcaService;
using System.Linq;

namespace LaOcaTests.JugadorDAO
{
    public class TestJugadorDAO : IDisposable
    {
        private readonly LaOcaBDEntities contexto;
        private LaOcaService.DAOs.JugadorFolder.JugadorDAO jugadorDAO;
        private int idJugadorPrueba;

        public TestJugadorDAO()
        {
            contexto = new LaOcaBDEntities();
            jugadorDAO = new LaOcaService.DAOs.JugadorFolder.JugadorDAO();
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
            contexto.Puntuaciones.Add(puntuacion);
            contexto.SaveChanges();

            var cuenta = new Cuentas
            {
                correoElectronico = "prueba@correo.com",
                contrasena = "contrasenaPrueba"
            };
            contexto.Cuentas.Add(cuenta);
            contexto.SaveChanges();

            var jugador = new Jugadores
            {
                nombreUsuario = "usuarioPrueba",
                IdCuenta = cuenta.IdCuenta,
                IdFotoPerfil = 1,
                IdPuntuacion = puntuacion.IdPuntuacion
            };
            contexto.Jugadores.Add(jugador);
            contexto.SaveChanges();
            idJugadorPrueba = jugador.IdJugador;
        }

        [Fact]
        public void PruebaCrearJugador()
        {
            var nuevoJugador = new Jugador
            {
                NombreUsuario = "nuevoUsuario",
                IdCuenta = contexto.Cuentas.First().IdCuenta,
                IdFotoPerfil = 2,
                IdPuntuacion = 2
            };

            jugadorDAO.CrearJugador(nuevoJugador, "imagen.jpg");

            var jugadorBD = contexto.Jugadores.FirstOrDefault(j => j.nombreUsuario == "nuevoUsuario");
            Assert.True(jugadorBD != null && jugadorBD.nombreUsuario == "nuevoUsuario");
        }

        [Fact]
        public void PruebaModificarJugador()
        {
            var jugador = new Jugador
            {
                IdJugador = idJugadorPrueba,
                NombreUsuario = "usuarioModificado",
                IdFotoPerfil = 1
            };

            jugadorDAO.ModificarJugador(jugador);

            using (var nuevoContexto = new LaOcaBDEntities())
            {
                var jugadorBD = nuevoContexto.Jugadores.Find(idJugadorPrueba);
                Assert.True(jugadorBD != null && jugadorBD.nombreUsuario == "usuarioModificado");
            }
        }

        [Fact]
        public void PruebaObtenerJugadorPorId()
        {
            var jugador = jugadorDAO.ObtenerJugadorPorId(idJugadorPrueba);
            Assert.True(jugador != null && jugador.NombreUsuario == "usuarioPrueba");
        }

        [Fact]
        public void PruebaNombreUsuarioExisteCrear()
        {
            var existe = jugadorDAO.NombreUsuarioExisteCrear("usuarioPrueba");
            Assert.True(existe);
        }

        [Fact]
        public void PruebaNombreUsuarioExisteModificar()
        {
            var existe = jugadorDAO.NombreUsuarioExisteModificar("usuarioPrueba", idJugadorPrueba);
            Assert.False(existe);
        }

        public void Dispose()
        {
            contexto.Dispose();
        }
    }
}