using LaOcaDataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class LaOcaService : IServicioChat
    {
        
        public void UnirseAlChat(string nombreJugador, string codigoSala)
        {
            if (listaSalasActivas[codigoSala].Jugadores.ContainsKey(nombreJugador))
            {
                listaSalasActivas[codigoSala].Jugadores[nombreJugador].CanalCallbackChat = OperationContext.Current.GetCallbackChannel<IChatCallback>();
            }

            EnviarMensaje(nombreJugador, "se ha unido al chat", codigoSala);
        }

        public void EnviarMensaje(string nombreJugador, string mensaje, string codigoSala)
        {
            foreach (var cliente in listaSalasActivas[codigoSala].Jugadores)
            {
                if (!cliente.Value.NombreUsuario.Equals(nombreJugador))
                {
                    try
                    {
                        if (ValidarJugadorNoEstaBloqueado(cliente.Value.NombreUsuario, nombreJugador))
                        {
                            cliente.Value.CanalCallbackChat.MostrarMensaje(nombreJugador, mensaje);
                        }
                    }
                    catch (CommunicationException ex)
                    {
                        Console.WriteLine($"Error de comunicación con el cliente: {ex.Message}");
                    }
                    catch (TimeoutException ex)
                    {
                        Console.WriteLine($"El cliente no respondió a tiempo: {ex.Message}");
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Console.WriteLine($"El cliente se ha desconectado: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inesperado al llamar al cliente: {ex.Message}");
                    }
                } 
            }
        }

        private bool ValidarJugadorNoEstaBloqueado(string nombreJugadorReceptor, string nombreJugadorEmisor)
        {
            bool resultado = true;

            Jugador jugadorReceptor = listaJugadoresConectados[nombreJugadorReceptor];
            Jugador jugadorEmisor = listaJugadoresConectados[nombreJugadorEmisor];

            Amistad amistad = RecuperarAmistad(jugadorEmisor.IdJugador, jugadorReceptor.IdJugador);

            if (amistad.IdAmistad > 0)
            {
                if (amistad.Estado.Equals("Bloqueo"))
                {
                    resultado = false;
                }
            }

            return resultado;
        }
    }
}
