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
            List<Jugador> listaJugadoresADesconectar = new List<Jugador>();
            foreach (var jugador in _ListaSalasActivas[codigoSala].Jugadores)
            {
                if (!jugador.Value.NombreUsuario.Equals(nombreJugador))
                {
                    try
                    {
                        if (ValidarJugadorNoEstaBloqueado(jugador.Value.NombreUsuario, nombreJugador))
                        {
                            jugador.Value.CanalCallbackChat.MostrarMensaje(nombreJugador, mensaje);
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        _LoggerChat.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                    }
                    catch (CommunicationException ex)
                    {
                        _LoggerChat.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                        listaJugadoresADesconectar.Add(jugador.Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inesperado al llamar al cliente: {ex.Message}");
                    }
                }
            }

            if (listaJugadoresADesconectar.Count > 0)
            {
                ManejarDesconexionInesperadaDeJugadoresEnSala(codigoSala, listaJugadoresADesconectar);
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
                   resultado = false;
                }
            }

            return resultado;
        }
    }
}
