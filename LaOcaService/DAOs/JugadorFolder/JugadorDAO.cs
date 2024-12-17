using LaOcaDataAccess;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;

namespace LaOcaService.DAOs.JugadorFolder
{
    public class JugadorDAO : IJugadorDAO
    {
        private readonly LaOcaBDEntities contexto;
        private static readonly ILog _LoggerJugadorDAO = LogManager.GetLogger(typeof(JugadorDAO));

        public JugadorDAO(LaOcaBDEntities contexto)
        {
            this.contexto = contexto;
        }

        public void CrearJugador(Jugador jugador, string referenciaImagen)
        {
            try
            {
                var cuentaExistente = contexto.Cuentas.Find(jugador.IdCuenta);
                if (cuentaExistente == null)
                {
                    throw new KeyNotFoundException($"La cuenta con id {jugador.IdCuenta} no existe.");
                }

                var aspectoExistente = contexto.Aspectos.Find(jugador.IdFotoPerfil) ??
                                       CrearAspecto(jugador.IdFotoPerfil, referenciaImagen);

                var puntuacionExistente = contexto.Puntuaciones.Find(jugador.IdPuntuacion) ??
                                          CrearPuntuacion();

                var jugadorBD = new Jugadores
                {
                    nombreUsuario = jugador.NombreUsuario,
                    IdFotoPerfil = aspectoExistente.IdAspecto,
                    IdCuenta = cuentaExistente.IdCuenta,
                    IdPuntuacion = puntuacionExistente.IdPuntuacion
                };

                contexto.Jugadores.Add(jugadorBD);
                contexto.SaveChanges();

                cuentaExistente.IdJugador = jugadorBD.IdJugador;
                puntuacionExistente.IdJugador = jugadorBD.IdJugador;
                contexto.SaveChanges();

                _LoggerJugadorDAO.Info($"Jugador creado con ID: {jugadorBD.IdJugador}");
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerJugadorDAO.Error("Ocurrió una excepción al crear un jugador: ", ex);
                throw new FaultException<JugadorException>(
                    new JugadorException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }


        public void ModificarJugador(Jugador jugador)
        {
            try
            {
                var jugadorBD = contexto.Jugadores.Find(jugador.IdJugador);
                if (jugadorBD == null)
                {
                    _LoggerJugadorDAO.Warn($"Intento de modificar un jugador inexistente con ID: {jugador.IdJugador}");
                    return;
                }

                jugadorBD.nombreUsuario = jugador.NombreUsuario;
                jugadorBD.IdFotoPerfil = jugador.IdFotoPerfil;
                contexto.SaveChanges();

                _LoggerJugadorDAO.Info($"Jugador con ID: {jugador.IdJugador} modificado exitosamente.");
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                _LoggerJugadorDAO.Error("Ocurrió una excepción al modificar un jugador: ", ex);
                throw new FaultException<JugadorException>(
                    new JugadorException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public Jugador ObtenerJugadorPorId(int idJugador)
        {
            try
            {
                var jugadorBD = contexto.Jugadores.Find(idJugador);
                if (jugadorBD == null)
                {
                    _LoggerJugadorDAO.Warn($"Jugador no encontrado con ID: {idJugador}");
                    return null;
                }

                return new Jugador
                {
                    IdJugador = jugadorBD.IdJugador,
                    NombreUsuario = jugadorBD.nombreUsuario,
                    IdFotoPerfil = (int)jugadorBD.IdFotoPerfil,
                    IdPuntuacion = (int)jugadorBD.IdPuntuacion,
                    IdCuenta = (int)jugadorBD.IdCuenta
                };
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                _LoggerJugadorDAO.Error("Ocurrió una excepción al obtener un jugador por ID: ", ex);
                throw new FaultException<JugadorException>(
                    new JugadorException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public bool NombreUsuarioExisteCrear(string nombreUsuario)
        {
            try
            {
                return contexto.Jugadores.Any(j => j.nombreUsuario == nombreUsuario);
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                _LoggerJugadorDAO.Error("Ocurrió una excepción al verificar la existencia de un nombre de usuario: ", ex);
                throw new FaultException<JugadorException>(
                    new JugadorException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public bool NombreUsuarioExisteModificar(string nombreUsuario, int idJugadorActual)
        {
            try
            {
                return contexto.Jugadores.Any(j => j.nombreUsuario == nombreUsuario && j.IdJugador != idJugadorActual);
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                _LoggerJugadorDAO.Error("Ocurrió una excepción al verificar la existencia de un nombre de usuario para modificar: ", ex);
                throw new FaultException<JugadorException>(
                    new JugadorException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        private Aspectos CrearAspecto(int idFotoPerfil, string referenciaImagen)
        {
            var nuevoAspecto = new Aspectos
            {
                IdAspecto = idFotoPerfil,
                tipo = Utilidades.TIPO_FOTO_PERFIL,
                referencia = referenciaImagen
            };

            contexto.Aspectos.Add(nuevoAspecto);
            contexto.SaveChanges();
            return nuevoAspecto;
        }

        private Puntuaciones CrearPuntuacion()
        {
            var nuevaPuntuacion = new Puntuaciones
            {
                casillasRecorridasGlobal = 0,
                partidasGanadasGlobal = 0,
            };

            contexto.Puntuaciones.Add(nuevaPuntuacion);
            contexto.SaveChanges();
            return nuevaPuntuacion;
        }
    }
}
