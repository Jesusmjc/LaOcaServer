using log4net;
using LaOcaDataAccess;
using LaOcaService.DAOs.PuntuacionFolder;
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
        public void NotificarMovimientoFicha(int posicion, string nombreJugador, string codigoSala)
        {
            if (listaSalasActivas.ContainsKey(codigoSala))
            {
                Sala sala = listaSalasActivas[codigoSala];

                if (sala.Jugadores.ContainsKey(nombreJugador))
                {
                    var jugador = sala.Jugadores[nombreJugador];

                    if (posicion != jugador.UltimaPosicion)
                    {
                        jugador.CasillasRecorridas++;
                        jugador.UltimaPosicion = posicion;
                    }

                    if (posicion == 63 && !jugador.HaLlegadoAMeta)
                    {
                        jugador.HaLlegadoAMeta = true;

                        var puntuacionDAO = new PuntuacionDAO(new LaOcaBDEntities());
                        foreach (var jugadorSala in sala.Jugadores.Values)
                        {
                            bool esGanador = jugadorSala.NombreUsuario == nombreJugador;
                            puntuacionDAO.ActualizarEstadisticasJugador(
                                jugadorSala.IdJugador,
                                jugadorSala.CasillasRecorridas,
                                esGanador
                            );
                        }

                        var jugadoresOrdenados = sala.Jugadores.Values
                        .OrderByDescending(j => j.HaLlegadoAMeta)
                        .ThenByDescending(j => j.HaLlegadoAMeta ? 0 : j.UltimaPosicion)
                        .ThenByDescending(j => j.CasillasRecorridas)
                        .Select(j => new KeyValuePair<string, int>(j.NombreUsuario, j.CasillasRecorridas))
                        .ToArray();

                        foreach (var jugadorSala in sala.Jugadores.Values)
                        {
                            try
                            {
                                jugadorSala.CanalCallbackPartida?.MostrarPantallaVictoria(jugadoresOrdenados);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al notificar victoria a {jugadorSala.NombreUsuario}: {ex.Message}");
                            }
                        }
                    }
                }
                foreach (var jugador in sala.Jugadores.Values)
                {
                    try
                    {
                        jugador.CanalCallbackPartida?.MovimientoFicha(posicion, nombreJugador);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al notificar movimiento para {jugador.NombreUsuario}: {ex.Message}");
                    }
                }
            }
        }

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
                List<string> jugadoresRestantes = sala.Partida.NombresDeJugadoresEnOrdenDeTurnos;

                if (jugadoresRestantes.Count == 0)
                {
                    throw new InvalidOperationException("No hay jugadores restantes en la sala.");
                }

                int posicionSiguienteJugador = (posicionJugadorTurnoActual + 1) % jugadoresRestantes.Count;
                string nombreSiguienteJugador = jugadoresRestantes[posicionSiguienteJugador];
                sala.Partida.NombreJugadorEnTurno = nombreSiguienteJugador;

                foreach (var jugador in sala.Jugadores.Values)
                {
                    try
                    {
                        jugador.CanalCallbackPartida?.MostrarNuevoJugadorEnTurno(nombreSiguienteJugador);
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
                        Console.WriteLine($"Error al notificar nuevo turno para {jugador.NombreUsuario}: {ex.Message}");
                    }
                }
                return nombreSiguienteJugador;
            }
        }

        public void AbandonarPartida(string nombreJugador, string codigoSala)
        {
            lock (listaSalasActivas)
            {
                if (!listaSalasActivas.ContainsKey(codigoSala)) return;

                Sala sala = listaSalasActivas[codigoSala];

                if (sala.Jugadores.ContainsKey(nombreJugador))
                {
                    sala.Jugadores.Remove(nombreJugador);
                    sala.Partida.NombresDeJugadoresEnOrdenDeTurnos.Remove(nombreJugador);

                    foreach (var jugador in sala.Jugadores.Values)
                    {
                        try
                        {
                            jugador.CanalCallbackPartida?.NotificarAbandonoJugador(nombreJugador);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al notificar abandono: {ex.Message}");
                        }
                    }

                    if (sala.Jugadores.Count == 1)
                    {
                        var jugadorRestante = sala.Jugadores.Values.First();
                        var jugadoresOrdenados = new[] { new KeyValuePair<string, int>(jugadorRestante.NombreUsuario, jugadorRestante.CasillasRecorridas) };
                        jugadorRestante.CanalCallbackPartida?.MostrarPantallaVictoria(jugadoresOrdenados);

                        listaSalasActivas.Remove(codigoSala);
                    }
                    else if (sala.Partida.NombreJugadorEnTurno == nombreJugador)
                    {
                        if (sala.Partida.NombresDeJugadoresEnOrdenDeTurnos.Count > 0)
                        {
                            sala.Partida.NombreJugadorEnTurno = sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[0];

                            foreach (var jugador in sala.Jugadores.Values)
                            {
                                try
                                {
                                    jugador.CanalCallbackPartida?.MostrarNuevoJugadorEnTurno(sala.Partida.NombreJugadorEnTurno);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error al notificar nuevo turno: {ex.Message}");
                                }
                            }
                        }
                    }
                }
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
