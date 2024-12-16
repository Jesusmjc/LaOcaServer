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
        /// <summary>
        /// Incluye un jugador en el diccionari de jugadores conectados,
        /// y notifica a los clientes conectados de esta nueva conexión
        /// </summary>
        /// <param name="nuevoJugadorConectado"> El jugador que inició sesión recientemente </param>
        /// <returns> 0 si no se agregó el jugador a la lista, 1 si el jugador se agregó </returns>
        [OperationContract]
        int AgregarJugadorConectado(Jugador nuevoJugadorConectado);

        /// <summary>
        /// Elimina a un jugador del diccionario de jugadores conectados,
        /// y notifica a los clientes conectados de la desconexión
        /// </summary>
        /// <param name="jugadorDesconectado"> El jugador que cerró sesión recientemente </param>
        [OperationContract]
        void EliminarJugadorDesconectado(Jugador jugadorDesconectado);

        /// <summary>
        /// Recupera todos los jugadores en el diccionario de jugadores conectados
        /// </summary>
        /// <returns> Un arreglo con todos los jugadores conectados actualmente </returns>
        [OperationContract]
        List<Jugador> RecuperarJugadoresConectados();
    }

    [ServiceContract(CallbackContract = typeof(IJugadoresEnLineaCallback))]
    public interface IServicioActualizacionJugadoresEnLinea
    {
        /// <summary>
        /// Agrega al jugador con el nombre correspondiente el canal para poder
        /// recibir métodos de callback relacionados con la conexión/desconexión de
        /// otros jugadores
        /// </summary>
        /// <param name="nombreJugador"> 
        /// El nombre de jugador que ahora será notificado de 
        /// conexiones/desconexiones
        /// </param>
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
