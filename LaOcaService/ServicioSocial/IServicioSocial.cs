using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    [ServiceContract]
    public interface IServicioSocial
    {
        [OperationContract]
        bool EnviarInvitacionAPartida(string nombreJugadorReceptor, Jugador jugadorEmisor, string codigoSala);

        [OperationContract]
        void EliminarInvitacionAPartida(string nombreJugador, InvitacionPartida invitacion);

        [OperationContract]
        List<InvitacionPartida> RecuperarInvitaciones(string nombreJugador);
    }

    [ServiceContract]
    public interface IServicioAmistad
    {
        [OperationContract]
        [FaultContract(typeof(AmistadException))]
        void EnviarSolicitudAmistad(Jugador jugadorSolicitante, Jugador jugadorReceptor);

        [OperationContract]
        [FaultContract(typeof(AmistadException))]
        List<Amistad> RecuperarAmistades(int idJugador, string estado);

        [OperationContract]
        [FaultContract(typeof(AmistadException))]
        void ActualizarSolicitudAmistad(Amistad solicitudAmistad, string nuevoEstado);

        [OperationContract]
        [FaultContract(typeof(AmistadException))]
        Amistad RecuperarAmistad(int idJugadorSolicitante, int idJugadorReceptor);
    }

    //public interface IAmistadCallback
    //{

    //}

    [ServiceContract(CallbackContract = typeof(IBuzonCallback))]
    public interface IServicioBuzon
    {
        [OperationContract]
        void AgregarCanalCallbackBuzon(string nombreJugador);
    }

    public interface IBuzonCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarNuevaInvitacionAPartida(InvitacionPartida invitacion);

        [OperationContract(IsOneWay = true)]
        void MostrarNuevaSolicitudAmistad(Amistad solicitudAmistad);
    }

    [DataContract]
    public class InvitacionPartida
    {
        [DataMember]
        public Jugador JugadorEmisor;

        [DataMember]
        public String CodigoSalaObjetivo;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            InvitacionPartida invitacion = (InvitacionPartida)obj;

            return JugadorEmisor.Equals(invitacion.JugadorEmisor) &&
                   CodigoSalaObjetivo == invitacion.CodigoSalaObjetivo;
        }

        public override int GetHashCode()
        {
            return (JugadorEmisor, CodigoSalaObjetivo).GetHashCode();
        }
    }

    [DataContract]
    public class Amistad
    {
        [DataMember]
        public int IdAmistad;

        [DataMember]
        public string Estado; // 'Solicitud', 'Amigos', 'Bloqueo', 'Rechazada'

        [DataMember]
        public int IdJugadorSolicitante;

        [DataMember]
        public int IdJugadorReceptor;

        [DataMember]
        public DateTime Fecha;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Amistad amistad = (Amistad)obj;

            return IdAmistad == amistad.IdAmistad &&
                   Estado == amistad.Estado &&
                   IdJugadorSolicitante == amistad.IdJugadorSolicitante &&
                   IdJugadorReceptor == amistad.IdJugadorReceptor;
        }

        public override int GetHashCode()
        {
            return (IdAmistad, Estado, IdJugadorSolicitante, IdJugadorReceptor).GetHashCode();
        }
    }

    [DataContract]
    public class AmistadException
    {
        [DataMember]
        public string Mensaje { get; set; }

        public AmistadException()
        {
            Mensaje = "Ha ocurrido un error con la solicitud de amistad.";
        }

        public AmistadException(string mensaje)
        {
            Mensaje = mensaje;
        }
    }

}
