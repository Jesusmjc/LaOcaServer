using System;
using System.Linq;
using Xunit;
using LaOcaDataAccess;
using LaOcaService.DAOs.PuntuacionFolder;
using System.Data.Entity;
using System.Collections.Generic;

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
        public void PruebaActualizarEstadisticasJugadorExitoso()
        {
            _puntuacionDAO.ActualizarEstadisticasJugador(_idJugadorPrueba, 5, true);

            var puntuacionActualizada = _contexto.Puntuaciones.FirstOrDefault(p => p.IdJugador == _idJugadorPrueba);

            Assert.True(puntuacionActualizada != null &&
                        puntuacionActualizada.casillasRecorridasGlobal == 15 &&
                        puntuacionActualizada.partidasGanadasGlobal == 2);
        }

        [Fact]
        public void PruebaActualizarEstadisticasJugadorFallido()
        {
            var ex = Assert.Throws<KeyNotFoundException>(() => _puntuacionDAO.ActualizarEstadisticasJugador(-1, 5, true)); // ID inexistente
            Assert.Contains("No se encontró una puntuación asociada al jugador con ID -1.", ex.Message);
        }

        [Fact]
        public void PruebaObtenerEstadisticasJugadorExitoso()
        {
            var estadisticas = _puntuacionDAO.ObtenerEstadisticasJugador(_idJugadorPrueba);

            Assert.True(estadisticas.CasillasRecorridasGlobal == 10 &&
                        estadisticas.PartidasGanadasGlobal == 1);
        }

        [Fact]
        public void PruebaObtenerEstadisticasJugadorFallido()
        {
            var ex = Assert.Throws<KeyNotFoundException>(() => _puntuacionDAO.ObtenerEstadisticasJugador(-1)); // ID inexistente
            Assert.Contains("No se encontró una puntuación asociada al jugador con ID -1.", ex.Message);
        }

        [Fact]
        public void PruebaObtenerRankingGlobalExitoso()
        {
            var ranking = _puntuacionDAO.ObtenerRankingGlobal();

            Assert.Contains(ranking, r => r.IdJugador == _idJugadorPrueba);
        }

        [Fact]
        public void PruebaObtenerRankingGlobalFallido()
        {
            _contexto.Amistades.RemoveRange(_contexto.Amistades); 
            _contexto.SaveChanges();

            _contexto.Puntuaciones.RemoveRange(_contexto.Puntuaciones);
            _contexto.SaveChanges();

            var ranking = _puntuacionDAO.ObtenerRankingGlobal();
            Assert.Empty(ranking);
        }

        public void Dispose()
        {
            _transaccion.Rollback();
            _transaccion.Dispose();
            _contexto.Dispose();
        }
    }
}
