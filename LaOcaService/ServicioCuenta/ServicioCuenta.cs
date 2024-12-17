using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LaOcaDataAccess;
using LaOcaService.DAOs;
using LaOcaService.DAOs.CuentaFolder;
using LaOcaService.DAOs.JugadorFolder;
using LaOcaService.DAOs.AspectoFolder;
using LaOcaService.DAOs.PuntuacionFolder;
using System.ServiceModel;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using log4net;

namespace LaOcaService
{
    public partial class LaOcaService : IServicioCuenta
    {
        private readonly ICuentaDAO _cuentaDAO;
        private readonly IJugadorDAO _jugadorDAO;
        private readonly IAspectoDAO _aspectoDAO;
        private static readonly ILog _LoggerCuentaDAO = LogManager.GetLogger(typeof(IServicioJugadoresEnLinea));
        private readonly Dictionary<string, string> _codigosVerificacion = new Dictionary<string, string>();

        public LaOcaService()
        {
            _cuentaDAO = new CuentaDAO(new LaOcaBDEntities());
            _jugadorDAO = new JugadorDAO(new LaOcaBDEntities());
            _aspectoDAO = new AspectoDAO(new LaOcaBDEntities());
            InicializarJuego();
        }

        public LaOcaService(ICuentaDAO cuentaDAO, IJugadorDAO jugadorDAO, IAspectoDAO aspectoDAO)
        {
            _cuentaDAO = cuentaDAO ?? throw new ArgumentNullException(nameof(cuentaDAO));
            _jugadorDAO = jugadorDAO ?? throw new ArgumentNullException(nameof(jugadorDAO));
            _aspectoDAO = aspectoDAO ?? throw new ArgumentNullException(nameof(aspectoDAO));
        }

        public void CrearCuenta(Cuenta cuenta, Jugador jugador, string referenciaImagen)
        {
            _cuentaDAO.CrearCuenta(cuenta);
            jugador.IdCuenta = cuenta.IdCuenta;
            _jugadorDAO.CrearJugador(jugador, referenciaImagen);
        }

        public void ModificarCuenta(Cuenta cuenta)
        {
            _cuentaDAO.ModificarCuenta(cuenta);
        }

        public Cuenta ObtenerCuentaPorId(int idCuenta)
        {
            return _cuentaDAO.ObtenerCuentaPorId(idCuenta);
        }

        public bool VerificarContraseñaActual(int idCuenta, string contraseñaActual)
        {
            try
            {
                using (var contexto = new LaOcaBDEntities())
                {
                    var cuenta = contexto.Cuentas.Find(idCuenta);
                    if (cuenta != null && cuenta.contrasena == contraseñaActual)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerCuentaDAO.Error("Ocurrió un error inesperado: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        private static string GenerarCodigoVerificacion()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public void SolicitarRecuperacionContrasena(string correoElectronico)
        {
            try
            {
                var cuenta = _cuentaDAO.ObtenerCuentaPorCorreo(correoElectronico);

                if (cuenta == null)
                {
                    throw new KeyNotFoundException("No se encontró una cuenta con ese correo electrónico.");
                }
                EnviarCodigoVerificacion(correoElectronico);
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerCuentaDAO.Error("Ocurrió un error inesperado: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public void ModificarContraseña(int idCuenta, string nuevaContrasena)
        {
            try
            {
                using (var contexto = new LaOcaBDEntities())
                {
                    var cuenta = contexto.Cuentas.Find(idCuenta);
                    if (cuenta == null)
                    {
                        throw new KeyNotFoundException("No se encontró la cuenta especificada.");
                    }

                    cuenta.contrasena = nuevaContrasena;
                    contexto.SaveChanges();
                }
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerCuentaDAO.Error("Ocurrió un error inesperado: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public Cuenta ObtenerCuentaPorCodigoVerificacion(string codigoVerificacion)
        {
            try
            {
                var correo = _codigosVerificacion.FirstOrDefault(x => x.Value == codigoVerificacion).Key;

                if (correo == null)
                {
                    return null;
                }

                var cuenta = _cuentaDAO.ObtenerCuentaPorCorreo(correo);

                if (cuenta != null)
                {
                    _codigosVerificacion.Remove(correo);
                }
                return cuenta;
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerCuentaDAO.Error("Ocurrió un error inesperado: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public bool CorreoExiste(string correoElectronico)
        {
            return _cuentaDAO.CorreoExiste(correoElectronico);
        }
        public bool NombreUsuarioExisteCrear(string nombreUsuario)
        {
            return _jugadorDAO.NombreUsuarioExisteCrear(nombreUsuario);
        }

        public bool NombreUsuarioExisteModificar(string nombreUsuario, int idJugadorActual)
        {
            return _jugadorDAO.NombreUsuarioExisteModificar(nombreUsuario, idJugadorActual);
        }

        public List<Jugador> ObtenerRankingGlobal()
        {
            try
            {
                var puntuacionDAO = new PuntuacionDAO(new LaOcaBDEntities());
                return puntuacionDAO.ObtenerRankingGlobal();
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerCuentaDAO.Error("Ocurrió un error inesperado: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public bool ProbarConexionConBD()
        {
            try
            {
                using (var contexto = new LaOcaBDEntities())
                {
                    contexto.Database.Connection.Open();
                    contexto.Database.Connection.Close();
                }
                return true;
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerCuentaDAO.Error("Ocurrió un error inesperado: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }

        public bool ProbarConexionConServidor()
        {
            return true;
        }
    }

    public partial class LaOcaService : IServicioJugador
    {
        public void ModificarJugador(Jugador jugador)
        {
            _jugadorDAO.ModificarJugador(jugador);
        }

        public Jugador ObtenerJugadorPorId(int idJugador)
        {
            return _jugadorDAO.ObtenerJugadorPorId(idJugador);
        }

        public string ConsultarEstadisticasJugador(int idJugador)
        {
            try
            {
                var puntuacionDAO = new PuntuacionDAO(new LaOcaBDEntities());
                var estadisticas = puntuacionDAO.ObtenerEstadisticasJugador(idJugador);

                return $": {estadisticas.CasillasRecorridasGlobal}, : {estadisticas.PartidasGanadasGlobal}";
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerCuentaDAO.Error("Ocurrió un error inesperado: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }
    }

    public partial class LaOcaService : IServicioAspecto
    {
        public void ModificarAspecto(Aspecto aspecto)
        {
            _aspectoDAO.ModificarAspecto(aspecto);
        }

        public void CrearAspecto(Aspecto aspecto)
        {
            _aspectoDAO.CrearAspecto(aspecto);
        }

        public Aspecto ObtenerAspectoPorId(int idAspecto)
        {
            return _aspectoDAO.ObtenerAspectoPorId(idAspecto);
        }

        public void SincronizarAspectos(Dictionary<string, int> referenciaToIdMap)
        {
            try
            {
                using (var contexto = new LaOcaBDEntities())
                {
                    foreach (var referencia in referenciaToIdMap)
                    {
                        var idAspecto = referencia.Value;
                        var urlImagen = referencia.Key;

                        var aspectoExistente = contexto.Aspectos.Find(idAspecto);
                        if (aspectoExistente == null)
                        {
                            var nuevoAspecto = new Aspectos
                            {
                                IdAspecto = idAspecto,
                                tipo = Utilidades.TIPO_FOTO_PERFIL,
                                referencia = urlImagen
                            };
                            contexto.Aspectos.Add(nuevoAspecto);
                        }
                    }

                    contexto.SaveChanges();
                }
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerCuentaDAO.Error("Ocurrió un error inesperado: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }
    }

    public partial class LaOcaService : IServicioCodigo
    {
        public void EnviarCodigoVerificacion(string correoElectronico)
        {
            string codigoVerificacion = GenerarCodigoVerificacion();
            _codigosVerificacion[correoElectronico] = codigoVerificacion;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("OcaGameService@gmail.com", "kaerrnwxqnjmbvgg"),
                EnableSsl = true
            };

            MailMessage mensaje = new MailMessage
            {
                From = new MailAddress("tuCorreo@gmail.com"),
                Subject = "Código de verificación",
                Body = $"Tu código de verificación es: {codigoVerificacion}"
            };

            mensaje.To.Add(correoElectronico);

            try
            {
                smtpClient.Send(mensaje);
                Console.WriteLine("Correo enviado exitosamente.");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error de operación inválida: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
            }
        }

        public bool VerificarCodigoCrearCuenta(string correo, string codigo)
        {
            return _codigosVerificacion.TryGetValue(correo, out string codigoAlmacenado) && codigoAlmacenado == codigo;
        }

        public int VerificarCodigoRecuperarContraseña(string correo, string codigo)
        {
            try
            {
                if (_codigosVerificacion.TryGetValue(correo, out string codigoAlmacenado) && codigoAlmacenado == codigo)
                {
                    var cuenta = _cuentaDAO.ObtenerCuentaPorCorreo(correo);

                    if (cuenta != null)
                    {
                        _codigosVerificacion.Remove(correo);
                        return cuenta.IdCuenta;
                    }
                }
                return -1;
            }
            catch (Exception ex) when (ex is SqlException || ex is EntityCommandExecutionException ||
                                       ex is InvalidOperationException || ex is EntityException ||
                                       ex is TimeoutException || ex is DbEntityValidationException)
            {
                _LoggerCuentaDAO.Error("Ocurrió un error inesperado: ", ex);
                throw new FaultException<CuentaException>(
                    new CuentaException("Ocurrió un error al conectar con la Base de Datos."),
                    new FaultReason("Error interno del servidor.")
                );
            }
        }
    }
}