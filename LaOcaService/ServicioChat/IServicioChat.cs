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
        [OperationContract(IsOneWay = true)]
        void UnirseAlChat(string nombreJugador, string codigoSala);


        [OperationContract(IsOneWay = true)]
        void EnviarMensaje(string nombreJugador, string mensaje, string codigoSala);
    }

    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarMensaje(string nombreJugador, string mensaje);
    }
}
