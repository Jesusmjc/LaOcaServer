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
                if (!listaSalasActivas[codigoSala].Jugadores.ContainsKey(nuevoJugador.NombreUsuario) && listaSalasActivas[codigoSala].Jugadores.Count <= 3)
                {
                    foreach (var jugador in listaSalasActivas[codigoSala].Jugadores)
                    {
                        jugador.Value.CanalCallbackSala.MostrarNuevoJugadorEnSala(nuevoJugador);
                    }

                    nuevoJugador.CanalCallbackSala = OperationContext.Current.GetCallbackChannel<ISalaCallback>();
                    listaSalasActivas[codigoSala].Jugadores.Add(nuevoJugador.NombreUsuario, nuevoJugador);

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

        public void NotificarDesconexion(string nombreJugadorDesconectado, string codigoSala)
        {
            Sala salaObjetivo = listaSalasActivas[codigoSala];
            salaObjetivo.Jugadores.Remove(nombreJugadorDesconectado);

            foreach (var parJugador in salaObjetivo.Jugadores)
            {
                parJugador.Value.CanalCallbackSala.MostrarDesconexionJugador(nombreJugadorDesconectado);
            }
        }

        public void EliminarSala(string codigoSala)
        {
            Sala salaObjetivo = listaSalasActivas[codigoSala];

            foreach (var parJugador in salaObjetivo.Jugadores)
            {
                if (!parJugador.Value.NombreUsuario.Equals(salaObjetivo.NombreHost))
                {
                    parJugador.Value.CanalCallbackSala.ExpulsarAMenúPrincipal("El anfitrión ha abandonado la sala. Regresarás al Menú Principal.");
                }
            }

            listaSalasActivas.Remove(codigoSala);
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
            Sala sala = listaSalasActivas[codigoSala];

            string nombreJugadorActual = (sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[posicionJugadorTurnoActual]);

            int posicionSiguienteJugador = (posicionJugadorTurnoActual + 1) % sala.Jugadores.Count;
            string nombreSiguienteJugador = (sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[posicionSiguienteJugador]);
            sala.Partida.NombreJugadorEnTurno = nombreSiguienteJugador;

            foreach (var parJugador in sala.Jugadores)
            {
                if (!parJugador.Key.Equals(nombreJugadorActual))
                {
                    parJugador.Value.CanalCallbackPartida.MostrarNuevoJugadorEnTurno(nombreSiguienteJugador);
                }
            }

            return nombreSiguienteJugador;
        }
    }
}
