using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService.DAOs.CarpetaCuenta
{
    internal interface ICuentaDAO
    {
        void CrearCuenta(Cuenta cuenta);
        Cuenta ObtenerCuentaPorId(int idCuenta);
    }
}
