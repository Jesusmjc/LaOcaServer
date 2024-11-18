using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace LaOcaService
{
    public partial class LaOcaService : IServicioJugadoresEnLinea
    {
        public static Dictionary<string, Jugador> listaJugadoresConectados = new Dictionary<string, Jugador>();
        
        public int AgregarJugadorConectado(Jugador nuevoJugadorConectado)
        {
            int resultado = 0;

            if (!listaJugadoresConectados.ContainsKey(nuevoJugadorConectado.NombreUsuario))
            {
                listaJugadoresConectados.Add(nuevoJugadorConectado.NombreUsuario, nuevoJugadorConectado);
                resultado = 1;

                foreach (var parJugador in listaJugadoresConectados)
                {
                    parJugador.Value.CanalCallbackJugadoresEnLinea?.MostrarNuevoJugadorConectado(nuevoJugadorConectado);
                }
            }

            return resultado;
        }

        public void EliminarJugadorDesconectado(Jugador jugadorDesconectado)
        {
            if (listaJugadoresConectados.ContainsKey(jugadorDesconectado.NombreUsuario))
            {
                listaJugadoresConectados.Remove(jugadorDesconectado.NombreUsuario);

                foreach (var parJugador in listaJugadoresConectados)
                {
                    if (!parJugador.Key.Equals(jugadorDesconectado.NombreUsuario))
                    {
                        parJugador.Value.CanalCallbackJugadoresEnLinea?.OcultarJugadorDesconectado(jugadorDesconectado);
                    }
                }
            }
        }

        public List<Jugador> RecuperarJugadoresConectados()
        {
            List<Jugador> jugadoresConectados = new List<Jugador>();

            foreach (var parJugador in listaJugadoresConectados)
            {
                jugadoresConectados.Add(parJugador.Value);
            }

            return jugadoresConectados;
        }
    }

    public partial class LaOcaService : IServicioActualizacionJugadoresEnLinea
    {
        public void AgregarCanalCallbackJugadoresEnLinea(string nombreJugador)
        {
            listaJugadoresConectados[nombreJugador].CanalCallbackJugadoresEnLinea = OperationContext.Current.GetCallbackChannel<IJugadoresEnLineaCallback>();
        }
    }
}
