using LaOcaDataAccess;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;

namespace LaOcaService.DAOs.AspectoFolder
{
    public class AspectoDAO : IAspectoDAO
    {
        private readonly LaOcaBDEntities contexto;
        private static readonly ILog _LoggerAspectoDAO = LogManager.GetLogger(typeof(AspectoDAO));

        public AspectoDAO(LaOcaBDEntities contexto)
        {
            this.contexto = contexto;
        }

        public void CrearAspecto(Aspecto aspecto)
        {
            try
            {
                var errores = new List<string>();

                if (string.IsNullOrWhiteSpace(aspecto.Tipo))
                {
                    errores.Add("El campo 'Tipo' es requerido.");
                }

                if (string.IsNullOrWhiteSpace(aspecto.Referencia))
                {
                    errores.Add("El campo 'Referencia' es requerido.");
                }

                if (errores.Any())
                {
                    throw new FaultException<AspectoException>(
                        new AspectoException(string.Join(" ", errores)),
                        new FaultReason("Datos inválidos.")
                    );
                }

                var aspectoBD = new Aspectos
                {
                    IdAspecto = aspecto.IdAspecto,
                    tipo = aspecto.Tipo,
                    referencia = aspecto.Referencia
                };

                contexto.Aspectos.Add(aspectoBD);
                contexto.SaveChanges();

                _LoggerAspectoDAO.Info($"Aspecto creado con ID: {aspecto.IdAspecto}");
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                _LoggerAspectoDAO.Error("Ocurrió una excepción al crear un aspecto: ", ex);
                throw new FaultException<AspectoException>(
                    new AspectoException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public Aspecto ObtenerAspectoPorId(int idAspecto)
        {
            try
            {
                var aspectoBD = contexto.Aspectos.FirstOrDefault(a => a.IdAspecto == idAspecto);
                if (aspectoBD == null)
                {
                    _LoggerAspectoDAO.Warn($"Aspecto no encontrado con ID: {idAspecto}");
                    throw new KeyNotFoundException($"Aspecto con ID {idAspecto} no encontrado.");
                }

                _LoggerAspectoDAO.Info($"Aspecto encontrado con ID: {idAspecto}");
                return new Aspecto
                {
                    IdAspecto = aspectoBD.IdAspecto,
                    Referencia = aspectoBD.referencia,
                    Tipo = aspectoBD.tipo
                };
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                _LoggerAspectoDAO.Error("Ocurrió una excepción al obtener un aspecto por ID: ", ex);
                throw new FaultException<AspectoException>(
                    new AspectoException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public void ModificarAspecto(Aspecto aspecto)
        {
            try
            {
                var aspectoBD = contexto.Aspectos.Find(aspecto.IdAspecto);
                if (aspectoBD == null)
                {
                    _LoggerAspectoDAO.Warn($"Intento de modificar un aspecto inexistente con ID: {aspecto.IdAspecto}");
                    throw new KeyNotFoundException($"Aspecto con ID {aspecto.IdAspecto} no encontrado.");
                }

                aspectoBD.referencia = aspecto.Referencia;
                aspectoBD.tipo = aspecto.Tipo;
                contexto.SaveChanges();

                _LoggerAspectoDAO.Info($"Aspecto con ID: {aspecto.IdAspecto} modificado exitosamente.");
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        _LoggerAspectoDAO.Error($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                    }
                }

                throw new FaultException<AspectoException>(
                    new AspectoException("Ocurrió un error de validación en los datos."),
                    new FaultReason("Error de validación.")
                );
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException)
            {
                _LoggerAspectoDAO.Error("Ocurrió una excepción al modificar un aspecto: ", ex);
                throw new FaultException<AspectoException>(
                    new AspectoException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }
    }
}
