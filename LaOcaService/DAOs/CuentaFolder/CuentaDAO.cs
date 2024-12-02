using LaOcaDataAccess;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;

namespace LaOcaService.DAOs.CuentaFolder
{
    public class CuentaDAO : ICuentaDAO
    {
        private readonly LaOcaBDEntities contexto;
        private static readonly ILog logger = LogManager.GetLogger(typeof(CuentaDAO));

        public CuentaDAO(LaOcaBDEntities contexto)
        {
            this.contexto = contexto;
        }

        public void CrearCuenta(Cuenta cuenta)
        {
            try
            {
                var cuentaBD = new Cuentas
                {
                    correoElectronico = cuenta.CorreoElectronico,
                    contrasena = cuenta.Contrasena,
                    IdJugador = cuenta.IdJugador
                };

                contexto.Cuentas.Add(cuentaBD);
                contexto.SaveChanges();

                cuenta.IdCuenta = cuentaBD.IdCuenta;

                logger.Info($"Cuenta creada con ID: {cuenta.IdCuenta}");
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                logger.Error("Ocurrió una excepción al crear una cuenta: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }


        public void ModificarCuenta(Cuenta cuenta)
        {
            try
            {
                var cuentaBD = contexto.Cuentas.Find(cuenta.IdCuenta);
                if (cuentaBD == null)
                {
                    logger.Warn($"Intento de modificar una cuenta inexistente con ID: {cuenta.IdCuenta}");
                    throw new KeyNotFoundException($"Cuenta con ID {cuenta.IdCuenta} no encontrada.");
                }

                cuentaBD.correoElectronico = cuenta.CorreoElectronico;
                cuentaBD.contrasena = cuenta.Contrasena;
                contexto.SaveChanges();

                logger.Info($"Cuenta con ID: {cuenta.IdCuenta} modificada exitosamente.");
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                logger.Error("Ocurrió una excepción al modificar una cuenta: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public Cuenta ObtenerCuentaPorId(int idCuenta)
        {
            try
            {
                var cuentaBD = contexto.Cuentas.Find(idCuenta);
                if (cuentaBD == null)
                {
                    logger.Warn($"Cuenta no encontrada con ID: {idCuenta}");
                    return null;
                }

                return new Cuenta
                {
                    IdCuenta = cuentaBD.IdCuenta,
                    CorreoElectronico = cuentaBD.correoElectronico,
                    Contrasena = cuentaBD.contrasena,
                    IdJugador = (int)cuentaBD.IdJugador
                };
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                logger.Error("Ocurrió una excepción al obtener una cuenta por ID: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public Cuenta ObtenerCuentaPorCorreo(string correoElectronico)
        {
            try
            {
                var cuentaBD = contexto.Cuentas.FirstOrDefault(c => c.correoElectronico == correoElectronico);
                if (cuentaBD == null)
                {
                    logger.Warn($"Cuenta no encontrada con correo: {correoElectronico}");
                    throw new KeyNotFoundException($"Cuenta con correo {correoElectronico} no encontrada.");
                }

                if (cuentaBD.IdJugador == null)
                {
                    logger.Error("La cuenta no tiene un jugador asociado.");
                    throw new InvalidOperationException("La cuenta no tiene un jugador asociado.");
                }

                return new Cuenta
                {
                    IdCuenta = cuentaBD.IdCuenta,
                    CorreoElectronico = cuentaBD.correoElectronico,
                    Contrasena = cuentaBD.contrasena,
                    IdJugador = cuentaBD.IdJugador.Value
                };
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                logger.Error("Ocurrió una excepción al obtener una cuenta por correo: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public bool CorreoExiste(string correoElectronico)
        {
            try
            {
                return contexto.Cuentas.Any(c => c.correoElectronico == correoElectronico);
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                         | ex is EntityException | ex is TimeoutException | ex is DbEntityValidationException)
            {
                logger.Error("Ocurrió una excepción al verificar la existencia de un correo electrónico: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }
    }
}
