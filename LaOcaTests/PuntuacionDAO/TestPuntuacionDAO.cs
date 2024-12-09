using System;
using System.Linq;
using Xunit;
using LaOcaDataAccess;
using LaOcaService.DAOs.PuntuacionFolder;
using System.Data.Entity;

namespace LaOcaTests.PuntuacionDAO
{
    public class TestPuntuacionDAO : IDisposable
    {
        private readonly LaOcaBDEntities _contexto;
        private readonly DbContextTransaction _transaccion;
        private readonly LaOcaService.DAOs.PuntuacionFolder.PuntuacionDAO _puntuacionDAO;
        private int _idJugadorPrueba;

        public TestPuntuacionDAO()
        {
            _contexto = new LaOcaBDEntities();
            _transaccion = _contexto.Database.BeginTransaction();
            _puntuacionDAO = new LaOcaService.DAOs.PuntuacionFolder.PuntuacionDAO(_contexto);
            PrepararBaseDeDatos();
        }

        private void PrepararBaseDeDatos()
        {
            var jugador = new Jugadores
            {
                nombreUsuario = "JugadorPrueba",
                IdCuenta = 1,
                IdFotoPerfil = 1
            };
            _contexto.Jugadores.Add(jugador);
            _contexto.SaveChanges();

            _idJugadorPrueba = jugador.IdJugador;

            var puntuacion = new Puntuaciones
            {
                IdJugador = _idJugadorPrueba,
                casillasRecorridasGlobal = 10,
                partidasGanadasGlobal = 1
            };
            _contexto.Puntuaciones.Add(puntuacion);
            _contexto.SaveChanges();
        }

        [Fact]
        public void PruebaActualizarEstadisticasJugador()
        {
            _puntuacionDAO.ActualizarEstadisticasJugador(_idJugadorPrueba, 5, true);

            var puntuacionActualizada = _contexto.Puntuaciones.FirstOrDefault(p => p.IdJugador == _idJugadorPrueba);

            Assert.True(puntuacionActualizada != null &&
                        puntuacionActualizada.casillasRecorridasGlobal == 15 &&
                        puntuacionActualizada.partidasGanadasGlobal == 2);
        }

        [Fact]
        public void PruebaObtenerEstadisticasJugador()
        {
            var estadisticas = _puntuacionDAO.ObtenerEstadisticasJugador(_idJugadorPrueba);

            Assert.True(estadisticas.CasillasRecorridasGlobal == 10 &&
                        estadisticas.PartidasGanadasGlobal == 1);
        }

        [Fact]
        public void PruebaObtenerRankingGlobal()
        {
            var ranking = _puntuacionDAO.ObtenerRankingGlobal();

            Assert.Contains(ranking, r => r.IdJugador == _idJugadorPrueba);
        }

        public void Dispose()
        {
            _transaccion.Rollback();
            _transaccion.Dispose();
            _contexto.Dispose();
        }
    }
}
