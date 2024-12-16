using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IServicioChat
    {
        /// <summary>
        /// Agrega al jugador correspondiente al chat de la sala correspondiente,
        /// de forma que podrá enviar/recibir mensajes de otros jugadores en la
        /// misma sala
        /// </summary>
        /// <param name="nombreJugador"> El jugador que se unió a la sala </param>
        /// <param name="codigoSala"> El código de la sala a la que se unió el jugador </param>
        [OperationContract(IsOneWay = true)]
        void UnirseAlChat(string nombreJugador, string codigoSala);

        /// <summary>
        /// Envia un mensaje del chat al resto de jugadores en la sala
        /// </summary>
        /// <param name="nombreJugador"> El nombre del jugador que envió el mensaje </param>
        /// <param name="mensaje"> La cadena que contiene el mensaje enviado </param>
        /// <param name="codigoSala"> El código de la sala donde se envió el mensaje </param>
        [OperationContract(IsOneWay = true)]
        void EnviarMensaje(string nombreJugador, string mensaje, string codigoSala);
    }

    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarMensaje(string nombreJugador, string mensaje);
    }
}
