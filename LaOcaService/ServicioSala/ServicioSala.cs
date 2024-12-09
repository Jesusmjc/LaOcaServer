using log4net;
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
        private static readonly Dictionary<int, string> fichaPorPosicion = new Dictionary<int, string>
        {
            { 0, "FichaOcaAmarilla" },
            { 1, "FichaOcaAzul" },
            { 2, "FichaOcaRosa" },
            { 3, "FichaOcaVerde" }
        };

        private static readonly ILog _loggerSala = LogManager.GetLogger(typeof(IServicioJugadoresEnLinea));

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
            return !listaSalasActivas.ContainsKey(codigoSala);
        }

        public int AgregarJugadorASala(Jugador nuevoJugador, string codigoSala)
        {
            int resultado = 0;
            if (listaSalasActivas.ContainsKey(codigoSala))
            {
                Sala sala = listaSalasActivas[codigoSala];

                if (!sala.Jugadores.ContainsKey(nuevoJugador.NombreUsuario) && sala.Jugadores.Count < 4)
                {
                    int posicionJugador = sala.Jugadores.Count;

                    nuevoJugador.FichaAsignada = fichaPorPosicion.ContainsKey(posicionJugador) ? fichaPorPosicion[posicionJugador] : "FichaOcaAmarilla";

                    foreach (var jugador in sala.Jugadores)
                    {
                        try
                        {
                            jugador.Value.CanalCallbackSala.MostrarNuevoJugadorEnSala(nuevoJugador);
                        }
                        catch (CommunicationException ex)
                        {
                            _loggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _loggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                        }
                    }

                    nuevoJugador.CanalCallbackSala = OperationContext.Current.GetCallbackChannel<ISalaCallback>();

                    sala.Jugadores.Add(nuevoJugador.NombreUsuario, nuevoJugador);

                    resultado = 1;
                }
            }
            return resultado;
        }


        public Partida IniciarPartida(string codigoSala)
        {
            List<string> ordenDeTurnos = DecidirOrdenDeTurnos(listaSalasActivas[codigoSala]);
            Partida nuevaPartida = new Partida()
            {
                NombresDeJugadoresEnOrdenDeTurnos = ordenDeTurnos,
                NombreJugadorEnTurno = ordenDeTurnos[0]
            };

            if (listaSalasActivas.ContainsKey(codigoSala))
            {
                listaSalasActivas[codigoSala].Partida = nuevaPartida;

                foreach (var parJugador in listaSalasActivas[codigoSala].Jugadores)
                {
                    if (!parJugador.Key.Equals(listaSalasActivas[codigoSala].NombreHost))
                    {
                        try
                        {
                            parJugador.Value.CanalCallbackSala.MostrarVentanaDePartida(nuevaPartida);
                        }
                        catch (CommunicationException ex)
                        {
                            _loggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _loggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                        }
                    }
                }
            }

            return nuevaPartida;
        }

        private List<string> DecidirOrdenDeTurnos(Sala sala)
        {
            Random random = new Random();

            List<string> nombresDeJugadoresEnOrdenDeTurnos = sala.Jugadores.Keys.OrderBy(key => random.Next()).ToList();

            return nombresDeJugadoresEnOrdenDeTurnos;
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

            if (sala.Codigo != codigoSala)
            {
                throw new FaultException<SalaException>(
                    new SalaException("No existe una sala con ese código."),
                    new FaultReason("No se encontró la sala.")
                );
            }

            return sala;
        }
    }

    public partial class LaOcaService : IServicioPartida
    {
        public void AgregarCanalCallbackPartida(string nombreJugador, string codigoSala)
        {
            if (listaSalasActivas.ContainsKey(codigoSala))
            {
                if (listaSalasActivas[codigoSala].Jugadores.ContainsKey(nombreJugador))
                {
                    listaSalasActivas[codigoSala].Jugadores[nombreJugador].CanalCallbackPartida = OperationContext.Current.GetCallbackChannel<IPartidaCallback>();
                }
            }
        }

        public string PasarTurnoASiguienteJugador(int posicionJugadorTurnoActual, string codigoSala)
        {
            lock (listaSalasActivas)
            {
                if (!listaSalasActivas.ContainsKey(codigoSala))
                {
                    throw new ArgumentException("Código de sala no válido.");
                }

                Sala sala = listaSalasActivas[codigoSala];
                string nombreJugadorActual = sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[posicionJugadorTurnoActual];

                int posicionSiguienteJugador = (posicionJugadorTurnoActual + 1) % sala.Jugadores.Count;
                string nombreSiguienteJugador = sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[posicionSiguienteJugador];
                sala.Partida.NombreJugadorEnTurno = nombreSiguienteJugador;

                foreach (var parJugador in sala.Jugadores)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            parJugador.Value.CanalCallbackPartida.MostrarNuevoJugadorEnTurno(nombreSiguienteJugador);
                        }
                        catch (CommunicationException ex)
                        {
                            _loggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _loggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al notificar al jugador {parJugador.Key}: {ex.Message}");
                        }
                    });
                }

                return nombreSiguienteJugador;
            }
        }
    }

    public partial class LaOcaService : IServicioActualizacionJugadoresEnSala
    {
        public void AgregarCanalCallbackActualizacionJugadoresEnSala(string nombreJugador, string codigoSala)
        {
            if (listaSalasActivas.ContainsKey(codigoSala))
            {
                if (listaSalasActivas[codigoSala].Jugadores.ContainsKey(nombreJugador))
                {
                    listaSalasActivas[codigoSala].Jugadores[nombreJugador].CanalCallbackJugadoresEnSala = OperationContext.Current.GetCallbackChannel<IActualizacionJugadoresEnSalaCallback>();
                }
            }
        }

        public void NotificarDesconexion(string nombreJugadorDesconectado, string codigoSala)
        {
            Sala salaObjetivo = listaSalasActivas[codigoSala];
            salaObjetivo.Jugadores.Remove(nombreJugadorDesconectado);

            foreach (var parJugador in salaObjetivo.Jugadores)
            {
                try
                {
                    parJugador.Value.CanalCallbackJugadoresEnSala.MostrarDesconexionJugador(nombreJugadorDesconectado);
                }
                catch (CommunicationException ex)
                {
                    _loggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                }
                catch (TimeoutException ex)
                {
                    _loggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                }
            }
        }

        public void EliminarSala(string codigoSala)
        {
            Sala salaObjetivo = listaSalasActivas[codigoSala];

            foreach (var parJugador in salaObjetivo.Jugadores)
            {
                if (!parJugador.Value.NombreUsuario.Equals(salaObjetivo.NombreHost))
                {
                    try
                    {
                        parJugador.Value.CanalCallbackJugadoresEnSala.ExpulsarAMenúPrincipal("El anfitrión ha abandonado la sala. Regresarás al Menú Principal.");
                    }
                    catch (CommunicationException ex)
                    {
                        _loggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        _loggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                    }
                }
            }

            listaSalasActivas.Remove(codigoSala);
        }

        public void ExpulsarJugador(string codigoSala, string nombreJugador)
        {
            Sala salaObjetivo = listaSalasActivas[codigoSala];
            salaObjetivo.Jugadores[nombreJugador].CanalCallbackJugadoresEnSala.ExpulsarAMenúPrincipal("El anfitrión te ha expulsado de la sala. Regresarás al Menú Principal");
            salaObjetivo.Jugadores.Remove(nombreJugador);

            string nombreHost = salaObjetivo.NombreHost;
            foreach (var parJugador in salaObjetivo.Jugadores)
            {
                if (!parJugador.Key.Equals(nombreHost))
                {
                    try
                    {
                        parJugador.Value.CanalCallbackJugadoresEnSala?.MostrarDesconexionJugador(nombreJugador);
                    }
                    catch (CommunicationException ex)
                    {
                        _loggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        _loggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                    }
                }
            }
        }

        public void NotificarCambioEnAmistad(string codigoSala, string nombreJugadorEmisor, string nombreJugadorObjetivo)
        {
            try
            {
                listaSalasActivas[codigoSala].Jugadores[nombreJugadorObjetivo].CanalCallbackJugadoresEnSala.ActualizarEstadoAmistad(nombreJugadorEmisor);
            }
            catch (CommunicationException ex)
            {
                _loggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
            }
            catch (TimeoutException ex)
            {
                _loggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
            }
        }
    }
}
