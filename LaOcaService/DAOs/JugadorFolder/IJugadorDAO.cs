using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService.DAOs.JugadorFolder
{
    public interface IJugadorDAO
    {
        void CrearJugador(Jugador jugador, string referenciaImagen);
        Jugador ObtenerJugadorPorId(int idJugador);
        void ModificarJugador(Jugador jugador);
    }
}
