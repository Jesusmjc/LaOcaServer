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
                    correoElectronico = cuenta.correoElectronico,
                    contrasena = cuenta.contrasena,
                    idJugador = cuenta.idJugador
                };
                contexto.Cuentas.Add(cuentaBD);
                contexto.SaveChanges();

                // Asignar el idCuenta generado a la cuenta original
                cuenta.idCuenta = cuentaBD.idCuenta;
            }
        }

        public void ModificarCuenta(Cuenta cuenta)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var cuentaBD = contexto.Cuentas.Find(cuenta.idCuenta);
                if (cuentaBD == null)
                {
                    return;
                }

                cuentaBD.correoElectronico = cuenta.correoElectronico;
                cuentaBD.contrasena = cuenta.contrasena;
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
                    idCuenta = cuentaBD.idCuenta,
                    correoElectronico = cuentaBD.correoElectronico,
                    contrasena = cuentaBD.contrasena,
                    idJugador = (int)cuentaBD.idJugador
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
                    idCuenta = cuentaBD.idCuenta,
                    correoElectronico = cuentaBD.correoElectronico,
                    contrasena = cuentaBD.contrasena,
                    idJugador = (int)cuentaBD.idJugador
                };
            }
        }

    }
}