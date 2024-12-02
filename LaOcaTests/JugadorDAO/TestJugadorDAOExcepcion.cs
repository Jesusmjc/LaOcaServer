using Xunit;
using System;
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
            _jugadorDAO = new LaOcaService.DAOs.JugadorFolder.JugadorDAO(_contexto);
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

            var excepcion = Assert.Throws<FaultException<JugadorException>>(() =>
            {
                _jugadorDAO.CrearJugador(jugador, "referencia_imagen.jpg");
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
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

            var excepcion = Assert.Throws<FaultException<JugadorException>>(() =>
            {
                _jugadorDAO.ModificarJugador(jugador);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaObtenerJugadorPorIdExcepcion()
        {
            int idJugadorInexistente = 999;

            var excepcion = Assert.Throws<FaultException<JugadorException>>(() =>
            {
                _jugadorDAO.ObtenerJugadorPorId(idJugadorInexistente);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaNombreUsuarioExisteExcepcion()
        {
            string nombreUsuarioInexistente = "NombreNoExistente";

            var excepcion = Assert.Throws<FaultException<JugadorException>>(() =>
            {
                _jugadorDAO.NombreUsuarioExisteCrear(nombreUsuarioInexistente);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaNombreUsuarioExisteModificarExcepcion()
        {
            string nombreUsuarioInexistente = "NombreNoExistente";
            int idJugadorInexistente = 999;

            var excepcion = Assert.Throws<FaultException<JugadorException>>(() =>
            {
                _jugadorDAO.NombreUsuarioExisteModificar(nombreUsuarioInexistente, idJugadorInexistente);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }
    }
}