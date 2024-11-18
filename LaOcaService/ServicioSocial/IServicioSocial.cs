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
}
