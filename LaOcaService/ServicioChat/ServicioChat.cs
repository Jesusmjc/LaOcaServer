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
        private readonly Dictionary<string, IChatCallback> _clientes = new Dictionary<string, IChatCallback>();
        
        public void UnirseAlChat(string nombreJugador)
        {
            IChatCallback callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();

            if (!_clientes.ContainsKey(nombreJugador))
            {
                _clientes.Add(nombreJugador, callback);
                Console.WriteLine(nombreJugador + " se ha unido al chat.");
            }

            EnviarMensaje(nombreJugador, " se ha unido al chat");
        }

        public void EnviarMensaje(string nombreJugador, string mensaje)
        {
            foreach (var cliente in _clientes.Values)
            {
                try
                {
                    cliente.MostrarMensaje(nombreJugador, mensaje);
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
