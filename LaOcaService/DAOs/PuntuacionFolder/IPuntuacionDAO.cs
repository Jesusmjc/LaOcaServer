using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService.DAOs.PuntuacionFolder
{
    internal interface IPuntuacionDAO
    {
        void ActualizarEstadisticasJugador(int idJugador, int casillasRecorridas, bool ganoPartida);
    }
}
