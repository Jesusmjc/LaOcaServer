using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaOcaDataAccess;
using LaOcaService.DAOs.CuentaFolder;

namespace LaOcaService.DAOs
{
    public class CuentaDAO : ICuentaDAO
    {
        //private readonly LaOcaBDEntities contexto;
        public CuentaDAO() {}
        /*public CuentaDAO(LaOcaBDEntities contexto)
        {
            this.contexto = contexto;
        }*/

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
                    throw new KeyNotFoundException($"Cuenta con ID {cuenta.IdCuenta} no encontrada.");
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
                var cuentaBD = contexto.Cuentas.FirstOrDefault(c => c.correoElectronico == correoElectronico);
                if (cuentaBD == null)
                {
                    throw new KeyNotFoundException($"Cuenta con correo {correoElectronico} no encontrada.");
                }

                if (cuentaBD.IdJugador == null)
                {
                    throw new InvalidOperationException("La cuenta no tiene un jugador asociado.");
                }

                return new Cuenta
                {
                    IdCuenta = cuentaBD.IdCuenta,
                    CorreoElectronico = cuentaBD.correoElectronico,
                    Contrasena = cuentaBD.contrasena,
                    IdJugador = cuentaBD.IdJugador.Value // Asegurar que no sea nulo
                };
            }
        }

        public bool CorreoExiste(string correoElectronico)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                return contexto.Cuentas.Any(c => c.correoElectronico == correoElectronico);
            }
        }

    }
}