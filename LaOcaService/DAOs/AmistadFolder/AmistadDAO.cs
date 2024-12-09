using LaOcaDataAccess;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService.DAOs
{
    public class AmistadDAO : AmistadFolder.IAmistadDAO
    {
        private readonly LaOcaBDEntities contexto;
        private static readonly ILog logger = LogManager.GetLogger(typeof(AmistadDAO));

        public AmistadDAO(LaOcaBDEntities contexto)
        {
            this.contexto = contexto;
        }

        public Amistad RecuperarAmistad(int idJugadorSolicitante, int idJugadorReceptor)
        {
            Amistad amistad = new Amistad();

            try
            {
                var amistadBD = contexto.Amistades.Where(a =>
                                                        (a.IdJugadorSolicitante == idJugadorSolicitante && a.IdJugadorReceptor == idJugadorReceptor) ||
                                                        (a.IdJugadorSolicitante == idJugadorReceptor && a.IdJugadorReceptor == idJugadorSolicitante)
                                                        ).FirstOrDefault();

                if (amistadBD != null)
                {
                    amistad = new Amistad
                    {
                        IdAmistad = amistadBD.IdAmistad,
                        Estado = amistadBD.estado,
                        Fecha = (DateTime)amistadBD.fecha,
                        IdJugadorReceptor = (int)amistadBD.IdJugadorReceptor,
                        IdJugadorSolicitante = (int)amistadBD.IdJugadorSolicitante,
                    };
                }   
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is InvalidOperationException | ex is EntityException | ex is TimeoutException
                                         | ex is DbEntityValidationException)
            {
                logger.Error("Ocurrió una excepción al consultar amistades: ", ex);

                throw new FaultException<AmistadException>(
                    new AmistadException("Ocurrió un error al conectar con la Base de Datos. Por favor intente más tarde."),
                    new FaultReason("Error interno del servidor. ")
                );
            }

            return amistad;
        }

        public int RegistrarNuevaAmistad(Amistad nuevaAmistad)
        {
            int resultado = 0;
            try
            {
                var amistadBD = contexto.Amistades.Where(a =>
                                                        (a.IdJugadorSolicitante == nuevaAmistad.IdJugadorSolicitante && a.IdJugadorReceptor == nuevaAmistad.IdJugadorReceptor) ||
                                                        (a.IdJugadorSolicitante == nuevaAmistad.IdJugadorReceptor && a.IdJugadorReceptor == nuevaAmistad.IdJugadorSolicitante)
                                                        ).FirstOrDefault();

                if (amistadBD == null)
                {
                    Amistades nuevaAmistadBD = new Amistades
                    {
                        estado = nuevaAmistad.Estado,
                        IdJugadorReceptor = nuevaAmistad.IdJugadorReceptor,
                        IdJugadorSolicitante = nuevaAmistad.IdJugadorSolicitante,
                        fecha = nuevaAmistad.Fecha
                    };

                    contexto.Amistades.Add(nuevaAmistadBD);
                    contexto.SaveChanges();

                    resultado = nuevaAmistadBD.IdAmistad;
                }
                else
                {
                    logger.Error("Se intentó guardar una nueva amistad que ya existe.");
                }
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is InvalidOperationException | ex is EntityException | ex is TimeoutException
                                         | ex is DbEntityValidationException)
            {
                logger.Error("Ocurrió una excepción al registar una nueva amistad: ", ex);

                throw new FaultException<AmistadException>(
                    new AmistadException("Ocurrió un error al conectar con la Base de Datos. "),
                    new FaultReason("Error interno del servidor. ")
                );
            }

            return resultado;
        }

        public int ActualizarEstadoAmistad(Amistad amistad)
        {
            int resultado = 0;
            try
            {
                Amistades amistadBD = contexto.Amistades.Where(a => a.IdAmistad == amistad.IdAmistad).FirstOrDefault();
                
                if (amistadBD != null)
                {
                    amistadBD.estado = amistad.Estado;
                    amistadBD.fecha = amistad.Fecha;
                    amistadBD.IdJugadorSolicitante = amistad.IdJugadorSolicitante;
                    amistadBD.IdJugadorReceptor = amistad.IdJugadorReceptor;

                    resultado = contexto.SaveChanges();
                }
                else
                {
                    logger.Error("\nSe intentó actualizar una amistad que no existe. \n");
                } 
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is InvalidOperationException | ex is EntityException | ex is TimeoutException
                                         | ex is DbEntityValidationException)
            {
                logger.Error("Ocurrió una excepción al actualizar una amistad: ", ex);

                throw new FaultException<AmistadException>(
                    new AmistadException("Ocurrió un error al conectar con la Base de Datos. "),
                    new FaultReason("Error interno del servidor. ")
                );
            }

            return resultado;
        }

        public List<Amistad> RecuperarAmistadesDeJugador(int idJugador, string estado)
        {
            List<Amistad> amistades = new List<Amistad>();

            try
            {
                List<Amistades> amistadesBD = new List<Amistades>();

                switch (estado)
                {
                    case "Solicitud":
                        amistadesBD = contexto.Amistades.Where(a => (a.IdJugadorReceptor == idJugador && a.estado == estado)).ToList();
                        break;

                    case "Bloqueo":
                        amistadesBD = contexto.Amistades.Where(a => (a.IdJugadorSolicitante == idJugador && a.estado == estado)).ToList();
                        break;

                    case "Amigos":
                        amistadesBD = contexto.Amistades.Where(a => ((a.IdJugadorSolicitante == idJugador || a.IdJugadorReceptor == idJugador) &&
                                                                      a.estado == estado)).ToList();
                        break;
                }

                if (amistadesBD?.Count > 0)
                {
                    foreach (var amistad in amistadesBD)
                    {
                        amistades.Add(new Amistad
                        {
                            IdAmistad = amistad.IdAmistad,
                            Estado = amistad.estado,
                            IdJugadorReceptor = (int)amistad.IdJugadorReceptor,
                            IdJugadorSolicitante = (int)amistad.IdJugadorSolicitante,
                            Fecha = (DateTime)amistad.fecha
                        });
                    }
                }
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is InvalidOperationException | ex is EntityException | ex is TimeoutException
                                         | ex is DbEntityValidationException)
            {
                logger.Error("Ocurrió una excepción al recuperar las amistades: ", ex);

                throw new FaultException<AmistadException>(
                    new AmistadException("Ocurrió un error al conectar con la Base de Datos. "),
                    new FaultReason("Error interno del servidor. ")
                );
            }

            return amistades;
        }
    }
}
