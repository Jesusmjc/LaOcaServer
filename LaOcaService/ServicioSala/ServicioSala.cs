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
        // Diccionario para asignar fichas a cada jugador en la sala
        private static readonly Dictionary<int, string> fichaPorPosicion = new Dictionary<int, string>
        {
            { 0, "FichaOcaAmarilla" },
            { 1, "FichaOcaAzul" },
            { 2, "FichaOcaRosa" },
            { 3, "FichaOcaVerde" }
        };


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
            bool esCodigoUnico = false;

            if (!listaSalasActivas.ContainsKey(codigoSala))
            {
                esCodigoUnico = true;
            }

            return esCodigoUnico;
        }

        public int AgregarJugadorASala(Jugador nuevoJugador, string codigoSala)
        {
            int resultado = 0;
            if (listaSalasActivas.ContainsKey(codigoSala))
            {
                Sala sala = listaSalasActivas[codigoSala];

                // Verifica que no haya más de 4 jugadores en la sala
                if (!sala.Jugadores.ContainsKey(nuevoJugador.NombreUsuario) && sala.Jugadores.Count < 4)
                {
                    int posicionJugador = sala.Jugadores.Count;

                    // Asigna una ficha según la posición en la sala
                    nuevoJugador.FichaAsignada = fichaPorPosicion.ContainsKey(posicionJugador) ? fichaPorPosicion[posicionJugador] : "FichaOcaAmarilla";

                    foreach (var jugador in sala.Jugadores)
                    {
                        jugador.Value.CanalCallbackSala.MostrarNuevoJugadorEnSala(nuevoJugador);
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
                        parJugador.Value.CanalCallbackSala.MostrarVentanaDePartida(nuevaPartida);
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

            return sala;
        }
    }

    public partial class LaOcaService : IServicioPartida
    {
        public void AgregarCanalCallback(string nombreJugador, string codigoSala)
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
            // Bloquear el acceso al método para asegurar que un solo hilo cambia el turno
            lock (listaSalasActivas)
            {
                if (!listaSalasActivas.ContainsKey(codigoSala))
                {
                    throw new ArgumentException("Código de sala no válido.");
                }

                Sala sala = listaSalasActivas[codigoSala];
                string nombreJugadorActual = sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[posicionJugadorTurnoActual];

                // Calcular la posición del siguiente jugador
                int posicionSiguienteJugador = (posicionJugadorTurnoActual + 1) % sala.Jugadores.Count;
                string nombreSiguienteJugador = sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[posicionSiguienteJugador];
                sala.Partida.NombreJugadorEnTurno = nombreSiguienteJugador;

                // Notificar de forma asincrónica a todos los jugadores sobre el nuevo turno
                foreach (var parJugador in sala.Jugadores)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            parJugador.Value.CanalCallbackPartida.MostrarNuevoJugadorEnTurno(nombreSiguienteJugador);
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
}
