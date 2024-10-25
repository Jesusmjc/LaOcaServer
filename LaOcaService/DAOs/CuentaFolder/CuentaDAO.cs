using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaOcaDataAccess;
using LaOcaService.DAOs.CuentaFolder;

namespace LaOcaService.DAOs
{
    internal class CuentaDAO : ICuentaDAO
    {
        public CuentaDAO() { }
        public void CrearCuenta(Cuenta cuenta)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var cuentaBD = new Cuentas
                {
                    correoElectronico = cuenta.CorreoElectronico,
                    contrasena = cuenta.Contrasena,
                    IdJugador = cuenta.IdJugador
                };
                contexto.Cuentas.Add(cuentaBD);
                contexto.SaveChanges();

                // Asignar el idCuenta generado a la cuenta original
                cuenta.IdCuenta = cuentaBD.IdCuenta;
            }
        }

        public void ModificarCuenta(Cuenta cuenta)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var cuentaBD = contexto.Cuentas.Find(cuenta.IdCuenta);
                if (cuentaBD == null)
                {
                    return;
                }

                cuentaBD.correoElectronico = cuenta.CorreoElectronico;
                cuentaBD.contrasena = cuenta.Contrasena;
                contexto.SaveChanges();
            }
        }


        public Cuenta ObtenerCuentaPorId(int idCuenta)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var cuentaBD = contexto.Cuentas.Find(idCuenta);
                if (cuentaBD == null)
                {
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
        }

        public Cuenta ObtenerCuentaPorCorreo(string correoElectronico)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                // Realizar la consulta a la base de datos buscando por correo electrónico
                var cuentaBD = contexto.Cuentas.FirstOrDefault(c => c.correoElectronico == correoElectronico);
                if (cuentaBD == null)
                {
                    return null;
                }

                // Retornar un objeto Cuenta con los datos encontrados
                return new Cuenta
                {
                    IdCuenta = cuentaBD.IdCuenta,
                    CorreoElectronico = cuentaBD.correoElectronico,
                    Contrasena = cuentaBD.contrasena,
                    IdJugador = (int)cuentaBD.IdJugador
                };
            }
        }

    }
}