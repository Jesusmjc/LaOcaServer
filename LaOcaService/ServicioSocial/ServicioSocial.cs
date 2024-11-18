using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    public partial class LaOcaService : IServicioSocial
    {
        public bool EnviarInvitacionAPartida(string nombreJugador, Jugador jugadorEmisor, string codigoSala)
        {
            bool resultado = false;

            InvitacionPartida invitacion = new InvitacionPartida
            {
                JugadorEmisor = jugadorEmisor,
                CodigoSalaObjetivo = codigoSala
            };

            if (!listaJugadoresConectados[nombreJugador].Invitaciones.Contains(invitacion))
            {
                listaJugadoresConectados[nombreJugador].CanalCallbackBuzon?.MostrarNuevaInvitacionAPartida(invitacion);
                listaJugadoresConectados[nombreJugador].Invitaciones.Add(invitacion);

                resultado = true;
            }

            return resultado;
        }

        public void EliminarInvitacionAPartida(string nombreJugador, InvitacionPartida invitacion)
        {
            listaJugadoresConectados[nombreJugador].Invitaciones.Remove(invitacion);
        }

        public List<InvitacionPartida> RecuperarInvitaciones(string nombreJugador)
        {
            List<InvitacionPartida> listaInvitaciones = listaJugadoresConectados[nombreJugador].Invitaciones;

            return listaInvitaciones;
        }
    }

    public partial class LaOcaService : IServicioBuzon
    {
        public void AgregarCanalCallbackBuzon(string nombreJugador)
        {
            listaJugadoresConectados[nombreJugador].CanalCallbackBuzon = OperationContext.Current.GetCallbackChannel<IBuzonCallback>();
        }
    }
}
