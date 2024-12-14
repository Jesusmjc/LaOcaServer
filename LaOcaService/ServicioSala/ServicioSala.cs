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
        private static Dictionary<string, Sala> _ListaSalasActivas = new Dictionary<string, Sala>();
        private static readonly Dictionary<int, string> _FichasPorPosicion = new Dictionary<int, string>
        {
            { 0, "FichaOcaAmarilla" },
            { 1, "FichaOcaAzul" },
            { 2, "FichaOcaRosa" },
            { 3, "FichaOcaVerde" }
        };

        private static readonly ILog _LoggerSala = LogManager.GetLogger(typeof(IServicioJugadoresEnLinea));

        public int AgregarNuevaSala(Sala nuevaSala)
        {
            int resultado = 0;
            if (!_ListaSalasActivas.ContainsKey(nuevaSala.Codigo))
            {
                nuevaSala.Jugadores[nuevaSala.NombreHost].CanalCallbackSala = OperationContext.Current.GetCallbackChannel<ISalaCallback>();

                _ListaSalasActivas.Add(nuevaSala.Codigo, nuevaSala);
                resultado = 1;
            }

            return resultado;
        }

        public bool VerificarCodigoSalaEsUnico(string codigoSala)
        {
            return !_ListaSalasActivas.ContainsKey(codigoSala);
        }

        public int AgregarJugadorASala(Jugador nuevoJugador, string codigoSala)
        {
            int resultado = 0;
            if (_ListaSalasActivas.ContainsKey(codigoSala))
            {
                Sala sala = _ListaSalasActivas[codigoSala];

                if (!sala.Jugadores.ContainsKey(nuevoJugador.NombreUsuario) && sala.Jugadores.Count < 4)
                {
                    int posicionJugador = sala.Jugadores.Count;

                    nuevoJugador.FichaAsignada = _FichasPorPosicion.ContainsKey(posicionJugador) ? _FichasPorPosicion[posicionJugador] : "FichaOcaAmarilla";

                    foreach (var jugador in sala.Jugadores)
                    {
                        try
                        {
                            jugador.Value.CanalCallbackSala.MostrarNuevoJugadorEnSala(nuevoJugador);
                        }
                        catch (CommunicationException ex)
                        {
                            _LoggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _LoggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
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
            List<string> ordenDeTurnos = DecidirOrdenDeTurnos(_ListaSalasActivas[codigoSala]);
            Partida nuevaPartida = new Partida()
            {
                NombresDeJugadoresEnOrdenDeTurnos = ordenDeTurnos,
                NombreJugadorEnTurno = ordenDeTurnos[0]
            };

            if (_ListaSalasActivas.ContainsKey(codigoSala))
            {
                _ListaSalasActivas[codigoSala].Partida = nuevaPartida;

                foreach (var parJugador in _ListaSalasActivas[codigoSala].Jugadores)
                {
                    if (!parJugador.Key.Equals(_ListaSalasActivas[codigoSala].NombreHost))
                    {
                        try
                        {
                            parJugador.Value.CanalCallbackSala.MostrarVentanaDePartida(nuevaPartida);
                        }
                        catch (CommunicationException ex)
                        {
                            _LoggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _LoggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
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

            if (_ListaSalasActivas.ContainsKey(codigoSala))
            {
                sala = _ListaSalasActivas[codigoSala];
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
            if (_ListaSalasActivas.ContainsKey(codigoSala))
            {
                Sala sala = _ListaSalasActivas[codigoSala];

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
                        ManejarFinDePartida(jugador, sala);
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

        private void ManejarFinDePartida(Jugador jugador, Sala sala)
        {
            jugador.HaLlegadoAMeta = true;

            var puntuacionDAO = new PuntuacionDAO(new LaOcaBDEntities());
            foreach (var jugadorSala in sala.Jugadores.Values)
            {
                bool esGanador = jugadorSala.NombreUsuario == jugador.NombreUsuario;
                if (!jugadorSala.EsInvitado) {
                    puntuacionDAO.ActualizarEstadisticasJugador(
                        jugadorSala.IdJugador,
                        jugadorSala.CasillasRecorridas,
                        esGanador
                    );
                }
            }

            var jugadoresOrdenados = sala.Jugadores.Values
            .OrderByDescending(j => j.HaLlegadoAMeta)
            .ThenByDescending(j => j.HaLlegadoAMeta ? 0 : j.UltimaPosicion)
            .ThenByDescending(j => j.CasillasRecorridas)
            .Select(j => new KeyValuePair<string, int>(j.NombreUsuario, j.CasillasRecorridas))
            .ToArray();

            _ListaSalasActivas.Remove(sala.Codigo);

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

        public void AgregarCanalCallbackPartida(string nombreJugador, string codigoSala)
        {
            if (_ListaSalasActivas.ContainsKey(codigoSala))
            {
                if (_ListaSalasActivas[codigoSala].Jugadores.ContainsKey(nombreJugador))
                {
                    _ListaSalasActivas[codigoSala].Jugadores[nombreJugador].CanalCallbackPartida = OperationContext.Current.GetCallbackChannel<IPartidaCallback>();
                }
            }
        }

        public string PasarTurnoASiguienteJugador(int posicionJugadorTurnoActual, string codigoSala)
        {
            lock (_ListaSalasActivas)
            {
                if (!_ListaSalasActivas.ContainsKey(codigoSala))
                {
                    throw new ArgumentException("Código de sala no válido.");
                }

                Sala sala = _ListaSalasActivas[codigoSala];
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
                        _LoggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        _LoggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
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
            lock (_ListaSalasActivas)
            {
                if (!_ListaSalasActivas.ContainsKey(codigoSala)) return;

                Sala sala = _ListaSalasActivas[codigoSala];

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

                        _ListaSalasActivas.Remove(codigoSala);
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
            if (_ListaSalasActivas.ContainsKey(codigoSala))
            {
                if (_ListaSalasActivas[codigoSala].Jugadores.ContainsKey(nombreJugador))
                {
                    _ListaSalasActivas[codigoSala].Jugadores[nombreJugador].CanalCallbackJugadoresEnSala = OperationContext.Current.GetCallbackChannel<IActualizacionJugadoresEnSalaCallback>();
                }
            }
        }

        public void NotificarDesconexion(string nombreJugadorDesconectado, string codigoSala)
        {
            Sala salaObjetivo = _ListaSalasActivas[codigoSala];
            salaObjetivo.Jugadores.Remove(nombreJugadorDesconectado);

            foreach (var parJugador in salaObjetivo.Jugadores)
            {
                try
                {
                    parJugador.Value.CanalCallbackJugadoresEnSala.MostrarDesconexionJugador(nombreJugadorDesconectado);
                }
                catch (CommunicationException ex)
                {
                    _LoggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                }
                catch (TimeoutException ex)
                {
                    _LoggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                }
            }
        }

        public void EliminarSala(string codigoSala)
        {
            Sala salaObjetivo = _ListaSalasActivas[codigoSala];

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
                        _LoggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        _LoggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                    }
                }
            }

            _ListaSalasActivas.Remove(codigoSala);
        }

        public void ExpulsarJugador(string codigoSala, string nombreJugador)
        {
            Sala salaObjetivo = _ListaSalasActivas[codigoSala];
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
                        _LoggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        _LoggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
                    }
                }
            }
        }

        public void NotificarCambioEnAmistad(string codigoSala, string nombreJugadorEmisor, string nombreJugadorObjetivo)
        {
            try
            {
                _ListaSalasActivas[codigoSala].Jugadores[nombreJugadorObjetivo].CanalCallbackJugadoresEnSala.ActualizarEstadoAmistad(nombreJugadorEmisor);
            }
            catch (CommunicationException ex)
            {
                _LoggerSala.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
            }
            catch (TimeoutException ex)
            {
                _LoggerSala.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
            }
        }
    }
}
