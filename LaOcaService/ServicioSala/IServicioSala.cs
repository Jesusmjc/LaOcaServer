using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using LaOcaDataAccess;


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
        public string TipoDeAcceso;

        [DataMember]
        public string NombreHost;

        [DataMember]
        public Dictionary<string, Jugador> Jugadores;
    }
}
