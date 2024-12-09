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
        public static Dictionary<string, Jugador> listaJugadoresConectados = new Dictionary<string, Jugador>();

        private static readonly ILog _loggerJugadoresEnLinea = LogManager.GetLogger(typeof(IServicioJugadoresEnLinea));

        public int AgregarJugadorConectado(Jugador nuevoJugadorConectado)
        {
            int resultado = 0;

            if (!listaJugadoresConectados.ContainsKey(nuevoJugadorConectado.NombreUsuario))
            {
                listaJugadoresConectados.Add(nuevoJugadorConectado.NombreUsuario, nuevoJugadorConectado);
                resultado = 1;

                if (!nuevoJugadorConectado.EsInvitado)
                {
                    foreach (var parJugador in listaJugadoresConectados)
                    {
                        try
                        {
                            parJugador.Value.CanalCallbackJugadoresEnLinea?.MostrarNuevoJugadorConectado(nuevoJugadorConectado);
                        }
                        catch (CommunicationException ex)
                        {
                            _loggerJugadoresEnLinea.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _loggerJugadoresEnLinea.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                        }
                    }
                }
            }

            return resultado;
        }

        public void EliminarJugadorDesconectado(Jugador jugadorDesconectado)
        {
            if (listaJugadoresConectados.ContainsKey(jugadorDesconectado.NombreUsuario))
            {
                listaJugadoresConectados.Remove(jugadorDesconectado.NombreUsuario);

                if (!jugadorDesconectado.EsInvitado)
                {
                    foreach (var parJugador in listaJugadoresConectados)
                    {
                        if (!parJugador.Key.Equals(jugadorDesconectado.NombreUsuario))
                        {
                            try
                            {
                                parJugador.Value.CanalCallbackJugadoresEnLinea?.OcultarJugadorDesconectado(jugadorDesconectado);
                            }
                            catch (CommunicationException ex)
                            {
                                _loggerJugadoresEnLinea.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                            }
                            catch (TimeoutException ex)
                            {
                                _loggerJugadoresEnLinea.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                            }
                        }
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
