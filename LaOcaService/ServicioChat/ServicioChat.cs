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
        //private readonly Dictionary<string, IChatCallback> clientes = new Dictionary<string, IChatCallback>();
        
        public void UnirseAlChat(string nombreJugador, string codigoSala)
        {
            //IChatCallback callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();

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
                try
                {
                    cliente.Value.CanalCallbackChat.MostrarMensaje(nombreJugador, mensaje);
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
}
