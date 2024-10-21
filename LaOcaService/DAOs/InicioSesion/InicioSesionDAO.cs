using LaOcaDataAccess;
using LaOcaService.DAOs.InicioSesion;
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
    public class InicioSesionDAO : IInicioSesionDAO
    {
        private readonly LaOcaBDEntities contexto;

        public InicioSesionDAO() {}

        public InicioSesionDAO(LaOcaBDEntities contexto)
        {
            this.contexto = contexto;
        }

        public Jugador IniciarSesion(Cuenta cuentaInicioSesion)
        {
            Jugador jugadorInicioSesion = new Jugador();

            try
            {
                //using (var contexto = new LaOcaBDEntities())
                //{
                    var cuentaBD = contexto.Cuentas.Where(cuenta => cuenta.correoElectronico == cuentaInicioSesion.CorreoElectronico
                                                            && cuenta.contrasena == cuentaInicioSesion.Contrasena).FirstOrDefault();

                    if (cuentaBD != null)
                    {
                        var jugadorBD = contexto.Jugadores.Where(jugador => jugador.IdCuenta == cuentaBD.IdCuenta).FirstOrDefault();

                        jugadorInicioSesion = new Jugador
                        {
                            IdJugador = jugadorBD.IdJugador,
                            IdCuenta = (int)jugadorBD.IdCuenta,
                            NombreUsuario = jugadorBD.nombreUsuario,
                            //IdPuntuacion = (int)jugadorBD.IdPuntuacion,
                            //IdFotoPerfil = (int)jugadorBD.IdFotoPerfil,
                        };
                    }
                //}
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                        | ex is InvalidOperationException | ex is EntityException | ex is TimeoutException
                                        | ex is DbEntityValidationException)
            {
                Console.WriteLine("Error al iniciar sesión. " + ex.Message + "\n" + ex.InnerException.Message); //aquí iría un log

                throw new FaultException<InicioSesionException>(
                    new InicioSesionException("Ocurrió un error al conectar con la Base de Datos. "),
                    new FaultReason("Error intenrno del servidor. " + ex.Message + ex.InnerException.Message)
                );
            }

            return jugadorInicioSesion;
        }
    }
}
