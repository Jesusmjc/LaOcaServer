using LaOcaDataAccess;
using LaOcaService.DAOs.InicioSesion;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService.DAOs
{
    internal class InicioSesionDAO : IInicioSesionDAO
    {
        public InicioSesionDAO() {}

        public Jugador IniciarSesion(Cuenta cuentaInicioSesion)
        {
            Jugador jugadorInicioSesion = new Jugador();

            try
            {
                using (var contexto = new LaOcaBDEntities())
                {
                    var cuentaBD = contexto.Cuentas.Where(cuenta => cuenta.correoElectronico == cuentaInicioSesion.CorreoElectronico
                                                            && cuenta.contrasena == cuentaInicioSesion.Contrasena).FirstOrDefault();

                    if (cuentaBD != null)
                    {
                        var jugadorBD = contexto.Jugadores.Where(jugador => jugador.IdCuenta == cuentaBD.IdCuenta).FirstOrDefault();


                        jugadorInicioSesion = new Jugador
                        {
                            IdJugador = jugadorBD.IdJugador,
                            NombreUsuario = jugadorBD.nombreUsuario,
                            //Nombre = jugadorBD.nombre,
                            //ApellidoPaterno = jugadorBD.apellidoPaterno,
                            //ApellidoMaterno = jugadorBD.apellidoMaterno,
                            //IdPuntuacion = (int)jugadorBD.IdPuntuacion,
                            //IdFotoPerfil = (int)jugadorBD.IdFotoPerfil,
                        };
                    }
                }
            }
            catch (Exception ex) when (ex is SqlException | ex is EntityCommandExecutionException | ex is InvalidOperationException
                                        | ex is InvalidOperationException | ex is EntityException | ex is TimeoutException)
            {
                Console.WriteLine("Error al iniciar sesión. " + ex.Message + "\n" + ex.InnerException.Message);
                throw new Exception(ex.Message + "\n" + ex.InnerException.Message + "\n");
            }
            //catch (SqlException ex)
            //{

            //}
            //catch (EntityCommandExecutionException ex)
            //{

            //}
            //catch (Exception ex)
            //{

            //}

            return jugadorInicioSesion;
        }
    }
}
