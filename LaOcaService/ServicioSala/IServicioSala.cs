using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using LaOcaDataAccess;
using System.CodeDom;


namespace LaOcaService
{
    [ServiceContract(CallbackContract = typeof(ISalaCallback))]
    public interface IServicioSala
    {
        /// <summary>
        /// Agrega una nueva sala a la lista de salas activas
        /// </summary>
        /// <param name="nuevaSala"> La información de la sala que acaba de ser creada </param>
        /// <returns> 1 si se agregó la sala a la lista, 0 en caso contrario </returns>
        [OperationContract]
        int AgregarNuevaSala(Sala nuevaSala);

        /// <summary>
        /// Valida que no exista una sala con el código proporcionado
        /// </summary>
        /// <param name="codigoSala"> El código tentativo de una nueva sala </param>
        /// <returns> true si no existe una sala con el código proporcionado, false en caso contrario </returns>
        [OperationContract]
        bool VerificarCodigoSalaEsUnico(string codigoSala);

        /// <summary>
        /// Agrega el jugador proporcionado a la sala que corresponde al código,
        /// y notifica al resto de jugadores en la sala de este hecho
        /// </summary>
        /// <param name="nuevoJugador"> La información del jugador que será agregado a la sala </param>
        /// <param name="codigoSala"> El código de la sala a la que se unirá el jugador </param>
        /// <returns> 1 si se agregó el jugador a la sala, 0 en caso contrario </returns>
        [OperationContract]
        int AgregarJugadorASala(Jugador nuevoJugador, string codigoSala);

        /// <summary>
        /// Crea la partida y la asocia a la sala, decide el turno de los jugadores y
        /// notifica a los jugadores que no son el host que ha empezado la partida
        /// </summary>
        /// <param name="codigoSala"> El código de la sala cuya partida acaba de empezar </param>
        /// <returns> La información de la partida que acaba de empezar </returns>
        [OperationContract]
        Partida IniciarPartida(string codigoSala);
    }

    [ServiceContract]
    public interface IServicioRecuperarSala
    {
        /// <summary>
        /// Recupera una sala específica del diccionario de salas activas
        /// </summary>
        /// <param name="codigoSala"> El código que identifica a la sala deseada </param>
        /// <returns> La información de la sala correpondiente </returns>
        [OperationContract]
        [FaultContract(typeof(SalaException))]
        Sala RecuperarSala(string codigoSala);
    }

    public interface ISalaCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarNuevoJugadorEnSala(Jugador nuevoJugador);

        [OperationContract(IsOneWay = true)]
        void MostrarVentanaDePartida(Partida partida);
 
    }

    [ServiceContract(CallbackContract = typeof(IPartidaCallback))]
    public interface IServicioPartida
    {
        /// <summary>
        /// Agrega al jugador el canal de callback para poder mostrar
        /// en pantalla cambios durante la partida en curso
        /// </summary>
        /// <param name="nombreJugador"> El nombre del jugador en cuestión </param>
        /// <param name="codigoSala"> El código de la sala a la que está asociada la partida </param>
        [OperationContract]
        void AgregarCanalCallbackPartida(string nombreJugador, string codigoSala);

        /// <summary>
        /// Notifica al siguiente jugador (el jugador en la posición siguiente a la del jugador
        /// cuyo turno acaba de terminar) que ahora es su turno
        /// </summary>
        /// <param name="posicionJugadorTurnoActual"> La posición del jugador cuyo turno acaba de terminar </param>
        /// <param name="codigoSala"> El código de la sala a la que está asociada la partida  </param>
        /// <returns> Nombre del jugador que ahora tomará su turno </returns>
        [OperationContract]
        string PasarTurnoASiguienteJugador(int posicionJugadorTurnoActual, string codigoSala);

        /// <summary>
        /// Notifica al resto de jugadores del movimiento en la ficha del jugador correspondiente
        /// </summary>
        /// <param name="posicion"> La nueva posición en la que se encuentra la ficha del jugador </param>
        /// <param name="nombreJugador"> El nombre del jugador cuya ficha se ha movido </param>
        /// <param name="codigoSala"> El código de la sala a la que está asociada la partida </param>
        [OperationContract(IsOneWay = true)]
        void NotificarMovimientoFicha(int posicion, string nombreJugador, string codigoSala);

        /// <summary>
        /// Elimina a un jugador de la sala y notifica a los demás jugadores en partida de
        /// esta desconexión
        /// </summary>
        /// <param name="nombreJugador"> El nombre del jugados desconectado </param>
        /// <param name="codigoSala"> El código de la sala a la que está asociada la partida </param>
        [OperationContract(IsOneWay = true)]
        void AbandonarPartida(string nombreJugador, string codigoSala);

        [OperationContract]
        void ReintentarGuardarEstadisticas(string codigoSala, string nombreJugador);

        [OperationContract]
        void FinalizarSinGuardarEstadisticas(string codigoSala, string nombreJugador);

        [OperationContract]
        bool Ping();
    }

    public interface IPartidaCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarNuevoJugadorEnTurno(string nombreNuevoJugadorEnTurno);

        [OperationContract(IsOneWay = true)]
        void ActualizarPosicionFicha(int nuevaPosicion, string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void MovimientoFicha(int posicion, string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void MostrarPantallaVictoria(KeyValuePair<string, int>[] jugadoresOrdenados);

        [OperationContract(IsOneWay = true)]
        void NotificarAbandonoJugador(string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void MostrarMensajeError(string mensaje);

        [OperationContract(IsOneWay = true)]
        void MostrarOpcionesErrorBD(string nombreJugadorGanador);

        [OperationContract(IsOneWay = true)]
        void MostrarMensajeExito(string mensaje);
    }

    [ServiceContract(CallbackContract = typeof(IActualizacionJugadoresEnSalaCallback))]
    public interface IServicioActualizacionJugadoresEnSala
    {
        /// <summary>
        /// Agrega el canal de callback al jugador para que pueda
        /// mostrar en pantalla cambios relacionados con las conexiones/desconexiones
        /// de jugadores en la sala
        /// </summary>
        /// <param name="nombreJugador"> El nombre del jugador en cuestión </param>
        /// <param name="codigoSala"> El código de la sala en cuestión </param>
        [OperationContract]
        void AgregarCanalCallbackActualizacionJugadoresEnSala(string nombreJugador, string codigoSala);

        /// <summary>
        /// Notifica del abandono de la sala de un jugador al resto de jugadores en dicha sala
        /// </summary>
        /// <param name="nombreJugadorDesconectado"> El nombre del jugador que ha dejado la sala </param>
        /// <param name="codigoSala"> El códigio de la sala en cuestión </param>
        [OperationContract]
        void NotificarDesconexion(string nombreJugadorDesconectado, string codigoSala);

        /// <summary>
        /// Elimina la sala proporcionada del diccinoario de salas activas, y notifica
        /// a los jugadores que están en esta que ya no existe.
        /// </summary>
        /// <param name="codigoSala"> El código de la sala que será eliminada </param>
        [OperationContract]
        void EliminarSala(string codigoSala);

        /// <summary>
        /// Elimina a un jugador de un sala y notifica a dicho jugador y a los demás jugadores
        /// que no sean host de este hecho
        /// </summary>
        /// <param name="codigoSala"> El código de la sala donde se ha expulsado el jugador </param>
        /// <param name="nombreJugador"> El nombre del jugador expulsado </param>
        [OperationContract]
        void ExpulsarJugador(string codigoSala, string nombreJugador);

        /// <summary>
        /// Notifica al jugador objetivo de un cambio en el estado de su amistad con el jugador emisor
        /// mientras ambos se encuentran en una sala
        /// </summary>
        /// <param name="codigoSala"> El código de la sala donde se encuentran ambos jugadores</param>
        /// <param name="nombreJugadorEmisor"> El nombre del jugador que ha cambiado el estado de la amistad </param>
        /// <param name="nombreJugadorObjetivo"> El nombre del jugador que verá reflejado los cambios mediante callback </param>
        [OperationContract]
        void NotificarCambioEnAmistad(string codigoSala, string nombreJugadorEmisor, string nombreJugadorObjetivo);
    }

    public interface IActualizacionJugadoresEnSalaCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarDesconexionJugador(string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void ExpulsarAMenúPrincipal(string motivo);

        [OperationContract(IsOneWay = true)]
        void ActualizarEstadoAmistad(string nombreJugadorEmisor);
    }


    [DataContract]
    public class Sala
    {
        [DataMember]
        public int IdSala;

        [DataMember]
        public string Codigo;

        [DataMember] 
        public string Nombre;

        [DataMember]
        public string Visibilidad;

        [DataMember]
        public string NombreHost;

        [DataMember]
        public Dictionary<string, Jugador> Jugadores;

        [DataMember]
        public Partida Partida { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Sala sala = (Sala)obj;

            return IdSala == sala.IdSala &&
                   Codigo == sala.Codigo &&
                   Nombre == sala.Nombre &&
                   Visibilidad == sala.Visibilidad &&
                   NombreHost == sala.NombreHost;
        }

        public override int GetHashCode()
        {
            return (IdSala, Codigo, Nombre, Visibilidad, NombreHost).GetHashCode();
        }
    }

    [DataContract]
    public class Partida
    {
        [DataMember]
        public string NombreJugadorEnTurno { get; set; }

        [DataMember]
        public List<string> NombresDeJugadoresEnOrdenDeTurnos { get; set; }
    }

    [DataContract]
    public class SalaException
    {
        [DataMember]
        public string Mensaje { get; set; }

        public SalaException()
        {
            Mensaje = "Ha ocurrido un error al procesar la solicitud de la sala.";
        }

        public SalaException(string mensaje)
        {
            Mensaje = mensaje;
        }
    }
}
