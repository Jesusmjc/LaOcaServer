using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    public partial class LaOcaService : IServicioSala
    {
        public static Dictionary<string, Sala> listaSalasActivas = new Dictionary<string, Sala>();

        public int AgregarNuevaSala(Sala nuevaSala)
        {
            int resultado = 0;
            if (!listaSalasActivas.ContainsKey(nuevaSala.Codigo))
            {
                nuevaSala.Jugadores[nuevaSala.NombreHost].CanalCallbackSala = OperationContext.Current.GetCallbackChannel<ISalaCallback>();

                listaSalasActivas.Add(nuevaSala.Codigo, nuevaSala);
                resultado = 1;
            }

            return resultado;
        }

        public bool VerificarCodigoSalaEsUnico(string codigoSala)
        {
            bool esCodigoUnico = false;

            if (!listaSalasActivas.ContainsKey(codigoSala))
            {
                esCodigoUnico = true;
            }

            return esCodigoUnico;
        }

        public int AgregarJugadorASala(Jugador nuevoJugador, string codigoSala)
        {
            int resultado = 0;
            if (listaSalasActivas.ContainsKey(codigoSala))
            {
                if (!listaSalasActivas[codigoSala].Jugadores.ContainsKey(nuevoJugador.NombreUsuario) && listaSalasActivas[codigoSala].Jugadores.Count <= 3)
                {
                    foreach (var jugador in listaSalasActivas[codigoSala].Jugadores)
                    {
                        jugador.Value.CanalCallbackSala.MostrarNuevoJugadorEnSala(nuevoJugador);
                    }

                    nuevoJugador.CanalCallbackSala = OperationContext.Current.GetCallbackChannel<ISalaCallback>();
                    listaSalasActivas[codigoSala].Jugadores.Add(nuevoJugador.NombreUsuario, nuevoJugador);

                    resultado = 1;
                }
            }
            return resultado;
        }
    }

    public partial class LaOcaService : IServicioRecuperarSala
    {
        public Sala RecuperarSala(string codigoSala)
        {
            Sala sala = new Sala();

            if (listaSalasActivas.ContainsKey(codigoSala))
            {
                sala = listaSalasActivas[codigoSala];
            }

            return sala;
        }
    }
}
