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
    }

    [ServiceContract]
    public interface IServicioRecuperarSala
    {
        [OperationContract]
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
        [OperationContract]
        void AgregarCanalCallback(string nombreJugador, string codigoSala);

        [OperationContract]
        string PasarTurnoASiguienteJugador(int posicionJugadorTurnoActual, string codigoSala);
    }


    public interface IPartidaCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarNuevoJugadorEnTurno(string nombreNuevoJugadorEnTurno);

        [OperationContract(IsOneWay = true)]
        void ActualizarPosicionFicha(int nuevaPosicion, string nombreJugador);
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
        public Partida Partida;
    }

    [DataContract]
    public class Partida
    {
        [DataMember]
        public string NombreJugadorEnTurno;

        [DataMember]
        public List<string> NombresDeJugadoresEnOrdenDeTurnos;
    }
}
