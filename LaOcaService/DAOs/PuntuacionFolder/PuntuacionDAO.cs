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
                logger.Info($"Estadísticas actualizadas para el jugador con ID: {idJugador}. " +
                            $"Casillas recorridas: {puntuacion.casillasRecorridasGlobal}, Partidas ganadas: {puntuacion.partidasGanadasGlobal}");
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                logger.Error("Error al actualizar estadísticas del jugador: ", ex);
                throw new FaultException<PuntuacionException>(
                    new PuntuacionException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }
    }
}
