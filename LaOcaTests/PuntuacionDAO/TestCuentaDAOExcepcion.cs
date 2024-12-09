using Xunit;
using System;
using LaOcaService;
using LaOcaDataAccess;
using System.ServiceModel;

namespace LaOcaTests.PuntuacionDAO
{
    public class TestPuntuacionDAOExcepcion
    {
        private readonly LaOcaBDEntities _contexto;
        private LaOcaService.DAOs.PuntuacionFolder.PuntuacionDAO _puntuacionDAO;

        public TestPuntuacionDAOExcepcion()
        {
            _contexto = new LaOcaBDEntities();
            _puntuacionDAO = new LaOcaService.DAOs.PuntuacionFolder.PuntuacionDAO(_contexto);
        }

        [Fact]
        public void PruebaActualizarEstadisticasJugadorExcepcion()
        {
            int idJugadorInexistente = 999;
            int casillasRecorridas = 10;
            bool ganoPartida = true;

            var excepcion = Assert.Throws<FaultException<PuntuacionException>>(() =>
            {
                _puntuacionDAO.ActualizarEstadisticasJugador(idJugadorInexistente, casillasRecorridas, ganoPartida);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaObtenerEstadisticasJugadorExcepcion()
        {
            int idJugadorInexistente = 999;

            var excepcion = Assert.Throws<FaultException<PuntuacionException>>(() =>
            {
                _puntuacionDAO.ObtenerEstadisticasJugador(idJugadorInexistente);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaObtenerRankingGlobalExcepcion()
        {
            var excepcion = Assert.Throws<FaultException<PuntuacionException>>(() =>
            {
                _puntuacionDAO.ObtenerRankingGlobal();
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }
    }
}
