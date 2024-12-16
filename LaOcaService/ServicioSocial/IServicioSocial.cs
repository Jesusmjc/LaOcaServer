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
        /// <summary>
        /// Agrega a la lista de invitaciones del jugador receptor una nueva invitación
        /// enviada por el jugador emisor
        /// </summary>
        /// <param name="nombreJugadorReceptor"> El nombre del jugador que recibe la invitación </param>
        /// <param name="jugadorEmisor"> La información del jugador que envía la invitación </param>
        /// <param name="codigoSala"> El código de la sala a la que se está invitando al receptor </param>
        /// <returns> true si se agregó la invitación correctamente, false en caso contrario </returns>
        [OperationContract]
        bool EnviarInvitacionAPartida(string nombreJugadorReceptor, Jugador jugadorEmisor, string codigoSala);

        /// <summary>
        /// Se elimina una invitación de la lista de invitaciones de un jugador
        /// </summary>
        /// <param name="nombreJugador"> Nombre del jugador que rechazó o aceptó la invitación </param>
        /// <param name="invitacion"> La información de la invitación que será eliminada </param>
        [OperationContract]
        void EliminarInvitacionAPartida(string nombreJugador, InvitacionPartida invitacion);

        /// <summary>
        /// Recupera la información de todas las invitaciones que tiene un jugador
        /// </summary>
        /// <param name="nombreJugador"> Nombre del jugador cuyas invitaciones pendientes serán recuperadas </param>
        /// <returns> Un arreglo con las invitaciones pendientes del jugador </returns>
        [OperationContract]
        List<InvitacionPartida> RecuperarInvitaciones(string nombreJugador);
    }

    [ServiceContract]
    public interface IServicioAmistad
    {
        /// <summary>
        /// Registra en la base de datos una nueva amistad entre dos jugadores,
        /// y notifica al jugador receptor de esta nueva amistad
        /// </summary>
        /// <param name="nuevaAmistad"> La información de la nueva amistad </param>
        /// <param name="nombreJugadorReceptor"> Nombre del jugador que será notificado de la nueva amistad </param>
        /// <returns> 
        /// El identificador de la amistad dentro de la base de datos si se guardó la nueva amistad correctamente,
        /// 0 en caso contrario
        /// </returns>
        [OperationContract]
        [FaultContract(typeof(AmistadException))]
        int RegistrarNuevaAmistad(Amistad nuevaAmistad, string nombreJugadorReceptor);

        /// <summary>
        /// Recupera todas las amistades del jugador proporcionado
        /// </summary>
        /// <param name="idJugador">  Identificador de la base de datos del jugador cuyas amistades serán recuperadas </param>
        /// <param name="estado"> El estado de las amistades que serán recuperadas </param>
        /// <returns> Arreglo de amistades del jugador cuyo estado coincide con el estado proporcionado </returns>
        [OperationContract]
        [FaultContract(typeof(AmistadException))]
        List<Amistad> RecuperarAmistades(int idJugador, string estado);

        /// <summary>
        /// Cambia el estado actual de la amistad proporcionada en la base de datos al estado proporcionado
        /// </summary>
        /// <param name="solicitudAmistad"> Información de la amistad cuyo estado será actualizado </param>
        /// <param name="nuevoEstado"> El nuevo estado que tendrá la amistad proporcionada </param>
        [OperationContract]
        [FaultContract(typeof(AmistadException))]
        void ActualizarSolicitudAmistad(Amistad solicitudAmistad, string nuevoEstado);

        /// <summary>
        /// Recupera de la base de datos la amistad entre dos jugadores
        /// </summary>
        /// <param name="idJugadorSolicitante"> El identificador del jugador que inició la amistad </param>
        /// <param name="idJugadorReceptor"> El identificador del jugador que recibió la amistad </param>
        /// <returns> La información de la amistad que existe entre ambos jugadores, o una amistad vacía en caso contrario </returns>
        [OperationContract]
        [FaultContract(typeof(AmistadException))]
        Amistad RecuperarAmistad(int idJugadorSolicitante, int idJugadorReceptor);
    }

    [ServiceContract(CallbackContract = typeof(IBuzonCallback))]
    public interface IServicioBuzon
    {
        /// <summary>
        /// Agrega un canal de callback al jugador para que pueda mostrar en pantalla
        /// nuevas invitaciones a partida que reciba en tiempo real
        /// </summary>
        /// <param name="nombreJugador"> Nombre del jugador en cuestión </param>
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
        public Jugador JugadorEmisor { get; set; }

        [DataMember]
        public string CodigoSalaObjetivo { get; set; }

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
        public int IdAmistad { get; set; }

        [DataMember]
        public string Estado { get; set; } // 'Solicitud', 'Amigos', 'Bloqueo', 'Rechazada'

        [DataMember]
        public int IdJugadorSolicitante { get; set; }

        [DataMember]
        public int IdJugadorReceptor { get; set; }

        [DataMember]
        public DateTime Fecha { get; set; }

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
