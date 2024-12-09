using LaOcaDataAccess;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;

namespace LaOcaService.DAOs.PuntuacionFolder
{
    public class PuntuacionDAO : IPuntuacionDAO
    {
        private readonly LaOcaBDEntities contexto;
        private static readonly ILog logger = LogManager.GetLogger(typeof(PuntuacionDAO));

        public PuntuacionDAO(LaOcaBDEntities contexto)
        {
            this.contexto = contexto;
        }



        public void ActualizarEstadisticasJugador(int idJugador, int casillasRecorridas, bool ganoPartida)
        {
            const int MaxReintentos = 3;
            int intentos = 0;
            bool guardadoExitoso = false;

            while (!guardadoExitoso && intentos < MaxReintentos)
            {
                try
                {
                    var puntuacion = contexto.Puntuaciones.FirstOrDefault(p => p.IdJugador == idJugador);

                    if (puntuacion == null)
                    {
                        throw new KeyNotFoundException($"No se encontró una puntuación asociada al jugador con ID {idJugador}.");
                    }

                    puntuacion.casillasRecorridasGlobal += casillasRecorridas;

                    if (ganoPartida)
                    {
                        puntuacion.partidasGanadasGlobal += 1;
                    }

                    contexto.SaveChanges();
                    guardadoExitoso = true;
                }
                catch (Exception ex) when (ex is SqlException || ex is EntityException || ex is TimeoutException)
                {
                    intentos++;
                    if (intentos >= MaxReintentos)
                    {
                        // Registrar fallo y propagar la excepción para manejo posterior
                        logger.Error("Error al guardar estadísticas después de varios intentos: ", ex);
                        throw;
                    }

                    // Retrasar antes de reintentar
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }


        public (int CasillasRecorridasGlobal, int PartidasGanadasGlobal) ObtenerEstadisticasJugador(int idJugador)
        {
            try
            {
                var puntuacion = contexto.Puntuaciones.FirstOrDefault(p => p.IdJugador == idJugador);

                if (puntuacion == null)
                {
                    throw new KeyNotFoundException($"No se encontró una puntuación asociada al jugador con ID {idJugador}.");
                }

                int casillasRecorridas = puntuacion.casillasRecorridasGlobal ?? 0;
                int partidasGanadas = puntuacion.partidasGanadasGlobal ?? 0;

                return (casillasRecorridas, partidasGanadas);
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                logger.Error("Error al obtener estadísticas del jugador: ", ex);
                throw new FaultException<PuntuacionException>(
                    new PuntuacionException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public List<Jugador> ObtenerRankingGlobal()
        {
            try
            {
                var ranking = contexto.Puntuaciones
                    .Where(p => p.IdJugador != null)
                    .OrderByDescending(p => p.partidasGanadasGlobal)
                    .ThenByDescending(p => p.casillasRecorridasGlobal)
                    .Select(p => new Jugador
                    {
                        IdJugador = p.IdJugador ?? 0, 
                        NombreUsuario = p.Jugadores.FirstOrDefault().nombreUsuario, 
                        CasillasRecorridas = p.casillasRecorridasGlobal ?? 0,
                        PartidasGanadas = p.partidasGanadasGlobal ?? 0,
                        IdFotoPerfil = p.Jugadores.FirstOrDefault().IdFotoPerfil ?? 0 
                    })
                    .Take(10)
                    .ToList();

                return ranking;
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                logger.Error("Error al obtener el ranking global: ", ex);
                throw new FaultException<PuntuacionException>(
                    new PuntuacionException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }
    }
}