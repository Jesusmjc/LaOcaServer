using LaOcaDataAccess;
using LaOcaService.DAOs;
using log4net;
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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public partial class LaOcaService : IServicioChat
    {
        private static readonly ILog _LoggerChat = LogManager.GetLogger(typeof(IServicioChat));

        public void UnirseAlChat(string nombreJugador, string codigoSala)
        {
            if (_ListaSalasActivas[codigoSala].Jugadores.ContainsKey(nombreJugador))
            {
                try
                {
                    _ListaSalasActivas[codigoSala].Jugadores[nombreJugador].CanalCallbackChat = OperationContext.Current.GetCallbackChannel<IChatCallback>();
                }
                catch (CommunicationException ex)
                {
                    _LoggerChat.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                }
                catch (TimeoutException ex)
                {
                    _LoggerChat.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                }
                
            }

            EnviarMensaje(nombreJugador, "se ha unido al chat", codigoSala);
        }

        public void EnviarMensaje(string nombreJugador, string mensaje, string codigoSala)
        {
            foreach (var cliente in _ListaSalasActivas[codigoSala].Jugadores)
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
                        _LoggerChat.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        _LoggerChat.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
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

            Jugador jugadorReceptor = _ListaJugadoresConectados[nombreJugadorReceptor];
            Jugador jugadorEmisor = _ListaJugadoresConectados[nombreJugadorEmisor];

            Amistad amistad = RecuperarAmistad(jugadorEmisor.IdJugador, jugadorReceptor.IdJugador);

            if (amistad.IdAmistad > 0)
            {
                if (amistad.Estado.Equals("Bloqueo"))
                {
                    try
                    {
                        resultado = false;
                    }
                    catch (CommunicationException ex)
                    {
                        _LoggerChat.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        _LoggerChat.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                    }
                }
            }

            return resultado;
        }
    }
}
