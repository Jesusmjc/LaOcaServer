using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using log4net;

namespace LaOcaService
{
    public partial class LaOcaService : IServicioJugadoresEnLinea
    {
        private static Dictionary<string, Jugador> _ListaJugadoresConectados = new Dictionary<string, Jugador>();

        private static readonly ILog _LoggerJugadoresEnLinea = LogManager.GetLogger(typeof(IServicioJugadoresEnLinea));

        public int AgregarJugadorConectado(Jugador nuevoJugadorConectado)
        {
            int resultado = 0;

            if (!_ListaJugadoresConectados.ContainsKey(nuevoJugadorConectado.NombreUsuario))
            {
                _ListaJugadoresConectados.Add(nuevoJugadorConectado.NombreUsuario, nuevoJugadorConectado);
                resultado = 1;

                if (!nuevoJugadorConectado.EsInvitado)
                {
                    List<Jugador> listaJugadoresADesconectar = new List<Jugador>();

                    foreach (var parJugador in _ListaJugadoresConectados)
                    {
                        try
                        {
                            parJugador.Value.CanalCallbackJugadoresEnLinea?.MostrarNuevoJugadorConectado(nuevoJugadorConectado);
                        }
                        catch (CommunicationException ex)
                        {
                            _LoggerJugadoresEnLinea.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _LoggerJugadoresEnLinea.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                            listaJugadoresADesconectar.Add(parJugador.Value);
                        }
                    }

                    if (listaJugadoresADesconectar.Count > 0)
                    {
                        ManejarDesconexionInesperadaDeJugadoresEnLinea(listaJugadoresADesconectar);
                    }
                }
            }

            return resultado;
        }

        public void EliminarJugadorDesconectado(Jugador jugadorDesconectado)
        {
            if (_ListaJugadoresConectados.ContainsKey(jugadorDesconectado.NombreUsuario))
            {
                _ListaJugadoresConectados.Remove(jugadorDesconectado.NombreUsuario);

                if (!jugadorDesconectado.EsInvitado)
                {
                    List<Jugador> listaJugadoresADesconectar = new List<Jugador>();

                    foreach (var parJugador in _ListaJugadoresConectados)
                    {
                        if (!parJugador.Key.Equals(jugadorDesconectado.NombreUsuario))
                        {
                            try
                            {
                                parJugador.Value.CanalCallbackJugadoresEnLinea?.OcultarJugadorDesconectado(jugadorDesconectado);
                            }
                            catch (CommunicationException ex)
                            {
                                _LoggerJugadoresEnLinea.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                            }
                            catch (TimeoutException ex)
                            {
                                _LoggerJugadoresEnLinea.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                                listaJugadoresADesconectar.Add(parJugador.Value);
                            }
                        }
                    }

                    if (listaJugadoresADesconectar.Count > 0)
                    {
                        ManejarDesconexionInesperadaDeJugadoresEnLinea(listaJugadoresADesconectar);
                    }
                }
            }
        }

        public List<Jugador> RecuperarJugadoresConectados()
        {
            List<Jugador> jugadoresConectados = new List<Jugador>();

            foreach (var parJugador in _ListaJugadoresConectados)
            {
                jugadoresConectados.Add(parJugador.Value);
            }

            return jugadoresConectados;
        }

        private void ManejarDesconexionInesperadaDeJugadoresEnLinea(List<Jugador> listaJugadoresADesconectar)
        {
            foreach (var jugador in listaJugadoresADesconectar)
            {
                _ListaJugadoresConectados.Remove(jugador.NombreUsuario);
            }

            foreach (var parJugador in _ListaJugadoresConectados)
            {
                try
                {
                    parJugador.Value.CanalCallbackJugadoresEnLinea?.OcultarJugadorDesconectado(parJugador.Value);
                }
                catch (TimeoutException ex)
                {
                    _LoggerSala.Error("Error al comunicarse con un cliente cuando se avisaba de la desconexión inesperada de otro cliente. La conexión tardó demasiado.", ex);
                }
                catch (CommunicationException ex)
                {
                    _LoggerSala.Error("Error al comunicarse con un cliente cuando se avisaba de la desconexión inesperada de otro cliente. El cliente se desconectó de forma inesperada.", ex);
                }
            }
        }
    }

    public partial class LaOcaService : IServicioActualizacionJugadoresEnLinea
    {
        public void AgregarCanalCallbackJugadoresEnLinea(string nombreJugador)
        {
            _ListaJugadoresConectados[nombreJugador].CanalCallbackJugadoresEnLinea = OperationContext.Current.GetCallbackChannel<IJugadoresEnLineaCallback>();
        }
    }
}
