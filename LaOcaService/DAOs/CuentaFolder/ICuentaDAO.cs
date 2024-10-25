using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService.DAOs.CuentaFolder
{
    public interface ICuentaDAO
    {
        void CrearCuenta(Cuenta cuenta);
        Cuenta ObtenerCuentaPorId(int idCuenta);
        void ModificarCuenta(Cuenta cuenta);
        Cuenta ObtenerCuentaPorCorreo(string correoElectronico);
    }
}
