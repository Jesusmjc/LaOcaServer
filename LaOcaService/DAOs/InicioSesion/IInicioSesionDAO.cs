using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService.DAOs.InicioSesion
{
    internal interface IInicioSesionDAO
    {
        Jugador IniciarSesion(Cuenta cuenta);
    }
}
