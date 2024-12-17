using log4net;
using LaOcaDataAccess;
using LaOcaService.DAOs.PuntuacionFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Data.SqlClient;

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
        public bool Ping()
        {
            return true; // Si el servidor responde, devuelve verdadero.
        }

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
                    catch (SqlException)
                    {
                        Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
                    }
                    catch (EntityException)
                    {
                        Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
                    }
                }
            }
        }

        private void ManejarFinDePartida(Jugador jugadorGanador, Sala sala)
        {
            jugadorGanador.HaLlegadoAMeta = true;

            try
            {
                var puntuacionDAO = new PuntuacionDAO(new LaOcaBDEntities());
                foreach (var jugadorSala in sala.Jugadores.Values)
                {
                    bool esGanador = jugadorSala.NombreUsuario == jugadorGanador.NombreUsuario;

                    if (!jugadorSala.EsInvitado)
                    {
                        puntuacionDAO.ActualizarEstadisticasJugador(
                            jugadorSala.IdJugador,
                            jugadorSala.CasillasRecorridas,
                            esGanador
                        );
                    }
                }
            }
            catch (FaultException)
            {
                NotificarErrorConOpciones(sala, jugadorGanador.NombreUsuario);
                return;
            }
            catch (EntityException)
            {
                NotificarErrorConOpciones(sala, jugadorGanador.NombreUsuario);
                return;
            }

            FinalizarPartidaYNotificar(sala, jugadorGanador);
        }

        public void ReintentarGuardarEstadisticas(string codigoSala, string nombreJugador)
        {
            if (_ListaSalasActivas.ContainsKey(codigoSala))
            {
                Sala sala = _ListaSalasActivas[codigoSala];
                Jugador jugador = sala.Jugadores[nombreJugador];

                try
                {
                    var puntuacionDAO = new PuntuacionDAO(new LaOcaBDEntities());
                    puntuacionDAO.ActualizarEstadisticasJugador(
                        jugador.IdJugador,
                        jugador.CasillasRecorridas,
                        jugador.HaLlegadoAMeta
                    );

                    try
                    {
                        jugador.CanalCallbackPartida?.MostrarMensajeExito("Todas las estadísticas se guardaron correctamente.");
                        FinalizarPartidaYNotificar(sala, jugador);
                    }
                    catch (CommunicationException ex)
                    {
                        _LoggerSala.Error("Error al enviar el callback MostrarMensajeExito", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        _LoggerSala.Error("Timeout al enviar el callback MostrarMensajeExito", ex);
                    }
                }
                catch (FaultException)
                {
                    NotificarErrorConOpciones(sala, nombreJugador);
                }
                catch (EntityException)
                {
                    NotificarErrorConOpciones(sala, nombreJugador);
                }
                catch (Exception)
                {
                    NotificarErrorConOpciones(sala, nombreJugador);
                }
            }
        }

        private void FinalizarPartidaYNotificar(Sala sala, Jugador jugadorGanador)
        {
            var jugadoresOrdenados = sala.Jugadores.Values
                .OrderByDescending(j => j.HaLlegadoAMeta)
                .ThenByDescending(j => j.HaLlegadoAMeta ? 0 : j.UltimaPosicion)
                .ThenByDescending(j => j.CasillasRecorridas)
                .Select(j => new KeyValuePair<string, int>(j.NombreUsuario, j.CasillasRecorridas))
                .ToArray();

            _ListaSalasActivas.Remove(sala.Codigo);

            foreach (var jugador in sala.Jugadores.Values)
            {
                try
                {
                    Task.Run(() =>
                    {
                        jugador.CanalCallbackPartida?.MostrarPantallaVictoria(jugadoresOrdenados);
                    });
                }
                catch (FaultException ex)
                {
                    _LoggerSala.Error("Error al notificar fin de partida", ex);
                }
                catch (EntityException ex)
                {
                    _LoggerSala.Error("Error al notificar fin de partida", ex);
                }
                catch (Exception ex)
                {
                    _LoggerSala.Error("Error al notificar fin de partida", ex);
                }
            }
        }

        public void FinalizarSinGuardarEstadisticas(string codigoSala, string nombreJugador)
        {
            if (_ListaSalasActivas.ContainsKey(codigoSala))
            {
                Sala sala = _ListaSalasActivas[codigoSala];
                Jugador jugador = sala.Jugadores[nombreJugador];

                try
                {
                    jugador.CanalCallbackPartida?.MostrarMensajeExito("La partida ha finalizado sin guardar las estadísticas.");
                    FinalizarPartidaYNotificar(sala, jugador);
                }
                catch (CommunicationException ex)
                {
                    _LoggerSala.Error("Error al enviar el callback MostrarMensajeExito", ex);
                }
                catch (TimeoutException ex)
                {
                    _LoggerSala.Error("Timeout al enviar el callback MostrarMensajeExito", ex);
                }
            }
        }

        private void NotificarErrorConOpciones(Sala sala, string nombreJugadorGanador)
        {
            foreach (var jugador in sala.Jugadores.Values)
            {
                try
                {
                    jugador.CanalCallbackPartida?.MostrarOpcionesErrorBD(nombreJugadorGanador);
                }
                catch (CommunicationException ex)
                {
                    Console.WriteLine($"Error al notificar error BD: {ex.Message}");
                }
                catch (TimeoutException ex)
                {
                    Console.WriteLine($"Error al notificar error BD: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al notificar error BD: {ex.Message}");
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
                if (!_ListaSalasActivas.ContainsKey(codigoSala))
                {
                    return;
                }

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
                        catch (SqlException)
                        {
                            Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
                        }
                        catch (EntityException)
                        {
                            Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
                        }
                    }

                    if (sala.Jugadores.Count == 1)
                    {
                        var jugadorRestante = sala.Jugadores.Values.First();
                        jugadorRestante.HaLlegadoAMeta = true;

                        try
                        {
                            var puntuacionDAO = new PuntuacionDAO(new LaOcaBDEntities());
                            puntuacionDAO.ActualizarEstadisticasJugador(
                                jugadorRestante.IdJugador,
                                jugadorRestante.CasillasRecorridas,
                                jugadorRestante.HaLlegadoAMeta
                            );

                            jugadorRestante.CanalCallbackPartida?.MostrarMensajeExito("Has ganado la partida por default.");
                            FinalizarPartidaYNotificar(sala, jugadorRestante);
                        }
                        catch (FaultException)
                        {
                            NotificarErrorConOpciones(sala, jugadorRestante.NombreUsuario);
                        }
                        catch (SqlException)
                        {
                            NotificarErrorConOpciones(sala, jugadorRestante.NombreUsuario);
                        }
                        catch (EntityException)
                        {
                            NotificarErrorConOpciones(sala, jugadorRestante.NombreUsuario);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error inesperado al finalizar partida: {ex.Message}");
                            NotificarErrorConOpciones(sala, jugadorRestante.NombreUsuario);
                        }
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
                                catch (SqlException)
                                {
                                    Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
                                }
                                catch (EntityException)
                                {
                                    Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
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