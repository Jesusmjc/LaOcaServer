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
    public interface IServicioJugadoresEnLinea
    {
        [OperationContract]
        int AgregarJugadorConectado(Jugador nuevoJugadorConectado);

        [OperationContract]
        void EliminarJugadorDesconectado(Jugador jugadorDesconectado);

        [OperationContract]
        List<Jugador> RecuperarJugadoresConectados();
    }

    [ServiceContract(CallbackContract = typeof(IJugadoresEnLineaCallback))]
    public interface IServicioActualizacionJugadoresEnLinea
    {
        [OperationContract]
        void AgregarCanalCallbackJugadoresEnLinea(string nombreJugador);
    }

    public interface IJugadoresEnLineaCallback 
    {
        [OperationContract(IsOneWay = true)]
        void MostrarNuevoJugadorConectado(Jugador nuevoJugadorConectado);

        [OperationContract(IsOneWay = true)]
        void OcultarJugadorDesconectado(Jugador nombreJugadorDesconectado);

        [OperationContract(IsOneWay = true)]
        void OcultarJugadorQueTerminoAmistad(int idJugadorQueTerminoAmistad);
    }    
}
