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

namespace LaOcaService
{
    public partial class LaOcaService : IServicioCuenta
    {
        private readonly ICuentaDAO _cuentaDAO;
        private readonly IJugadorDAO _jugadorDAO;
        private readonly IAspectoDAO _aspectoDAO;

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

        private static string GenerarCodigoVerificacion()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public void SolicitarRecuperacionContrasena(string correoElectronico)
        {
            var cuenta = _cuentaDAO.ObtenerCuentaPorCorreo(correoElectronico);

            if (cuenta == null)
            {
                throw new KeyNotFoundException("No se encontró una cuenta con ese correo electrónico.");
            }
            EnviarCodigoVerificacion(correoElectronico);
        }

        public void ModificarContraseña(int idCuenta, string nuevaContrasena)
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

        public Cuenta ObtenerCuentaPorCodigoVerificacion(string codigoVerificacion)
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el ranking global: {ex.Message}");
                throw new FaultException("Error al obtener el ranking global. Por favor, intente de nuevo más tarde.");
            }
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
            var puntuacionDAO = new PuntuacionDAO(new LaOcaBDEntities());
            var estadisticas = puntuacionDAO.ObtenerEstadisticasJugador(idJugador);

            return $": {estadisticas.CasillasRecorridasGlobal}, : {estadisticas.PartidasGanadasGlobal}";
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
                            tipo = Utilidades.TIPOFOTOPERFIL,
                            referencia = urlImagen
                        };
                        contexto.Aspectos.Add(nuevoAspecto);
                    }
                }

                contexto.SaveChanges();
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
    }
}