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
        [OperationContract]
        int AgregarNuevaSala(Sala nuevaSala);

        [OperationContract]
        bool VerificarCodigoSalaEsUnico(string codigoSala);

        [OperationContract]
        int AgregarJugadorASala(Jugador nuevoJugador, string codigoSala);

        [OperationContract]
        Partida IniciarPartida(string codigoSala);

        [OperationContract]
        void NotificarDesconexion(string nombreJugadorDesconectado, string codigoSala);

        [OperationContract]
        void EliminarSala(string codigoSala);
    }

    [ServiceContract]
    public interface IServicioRecuperarSala
    {
        [OperationContract]
        [FaultContract(typeof(SalaException))]
        Sala RecuperarSala(string codigoSala);
    }

    [ServiceContract]
    public interface IServicioExpulsionSala
    {
        [OperationContract]
        void ExpulsarJugador(string codigoSala, string nombreJugador);
    }

    public interface ISalaCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarNuevoJugadorEnSala(Jugador nuevoJugador);

        [OperationContract(IsOneWay = true)]
        void MostrarVentanaDePartida(Partida partida);

        [OperationContract(IsOneWay = true)]
        void MostrarDesconexionJugador(string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void ExpulsarAMenúPrincipal(string motivo);
    }

    [ServiceContract(CallbackContract = typeof(IPartidaCallback))]
    public interface IServicioPartida
    {
        [OperationContract]
        void AgregarCanalCallbackPartida(string nombreJugador, string codigoSala);

        [OperationContract]
        string PasarTurnoASiguienteJugador(int posicionJugadorTurnoActual, string codigoSala);

        [OperationContract(IsOneWay = true)]
        void NotificarMovimientoFicha(int posicion, string nombreJugador, string codigoSala);

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

    }

    //[ServiceContract]
    //public interface IServicioActualizacionSalaYPartida
    //{
    //    [OperationContract]
    //    void AgregarCanalCallbackActualizacionSalaPartida(string nombreJugador, string codigoSala);

    //    [OperationContract]
    //    void ExpulsarJugador(string codigoSala, string nombreJugador);
    //}

    //public interface IActualizacionSalaYPartidaCallback
    //{
    //    [OperationContract(IsOneWay = true)]
    //    void MostrarDesconexionJugador(string nombreJugador);

    //    [OperationContract(IsOneWay = true)]
    //    void ExpulsarAMenúPrincipal(string motivo);
    //}


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
        public Partida Partida;


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
        public string NombreJugadorEnTurno;

        [DataMember]
        public List<string> NombresDeJugadoresEnOrdenDeTurnos;
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
