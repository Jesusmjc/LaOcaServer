using LaOcaService.DAOs;
using LaOcaService.DAOs.AmistadFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

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

    public partial class LaOcaService : IServicioAmistad
    {
        public int RegistrarNuevaAmistad(Amistad nuevaAmistad, string nombreJugadorReceptor)
        {
            AmistadDAO amistadDAO = new AmistadDAO(new LaOcaDataAccess.LaOcaBDEntities());
            Amistad amistadExistente = amistadDAO.RecuperarAmistad(nuevaAmistad.IdJugadorSolicitante, nuevaAmistad.IdJugadorReceptor);

            nuevaAmistad.Fecha = DateTime.Now;

            int resultado = 0;

            if (amistadExistente.IdAmistad == 0)
            {
                resultado = amistadDAO.RegistrarNuevaAmistad(nuevaAmistad);
                
                if (resultado > 0)
                {
                    nuevaAmistad.IdAmistad = resultado;
                    listaJugadoresConectados[nombreJugadorReceptor].Amistades.Add(nuevaAmistad);
                }
                else
                {
                    throw new FaultException<AmistadException>(
                        new AmistadException("Ocurrió un error al procesar la solicitud de amistad. "),
                        new FaultReason("Error interno del servidor. ")
                    );
                }
            }
            else
            {
                if (!amistadExistente.Estado.Equals("Amigos") || !amistadExistente.Estado.Equals("Bloqueo"))
                {
                    nuevaAmistad.IdAmistad = amistadExistente.IdAmistad;
                    amistadDAO.ActualizarEstadoAmistad(nuevaAmistad);

                    listaJugadoresConectados[nombreJugadorReceptor].Amistades.Add(nuevaAmistad);
                }
                else if (amistadExistente.Estado.Equals("Amigos"))
                {
                    throw new FaultException<AmistadException>(
                        new AmistadException("No se envió la solicitud de amistad. "),
                        new FaultReason("¡Ustedes ya son amigos!.")
                    );
                }
                else
                {
                    throw new FaultException<AmistadException>(
                        new AmistadException("No se envió la solicitud de amistad. "),
                        new FaultReason("El jugador te ha bloqueado.")
                    );
                }
            }

            return resultado;
        }

        public void ActualizarSolicitudAmistad(Amistad solicitudAmistad, string nuevoEstado)
        {
            AmistadDAO amistadDAO = new AmistadDAO(new LaOcaDataAccess.LaOcaBDEntities());

            string estadoPrevio = solicitudAmistad.Estado;
            solicitudAmistad.Estado = nuevoEstado;
            solicitudAmistad.Fecha = DateTime.Now;

            int resultado = amistadDAO.ActualizarEstadoAmistad(solicitudAmistad);

            if (estadoPrevio.Equals("Amigos") && (nuevoEstado.Equals("Rechazada") || nuevoEstado.Equals("Bloqueo")))
            {
                Jugador amigoEliminado = ObtenerJugadorPorId(solicitudAmistad.IdJugadorReceptor);
                if (listaJugadoresConectados.ContainsKey(amigoEliminado.NombreUsuario))
                {
                    amigoEliminado = listaJugadoresConectados[amigoEliminado.NombreUsuario];
                    amigoEliminado.CanalCallbackJugadoresEnLinea?.OcultarJugadorQueTerminoAmistad(solicitudAmistad.IdJugadorSolicitante);
                }
            }

            if (resultado == 0)
            {
                throw new FaultException<AmistadException>(
                       new AmistadException("Ocurrió un error al actualizar la solicitud de amistad. "),
                       new FaultReason("Error interno del servidor. ")
                   );
            }
        }

        public List<Amistad> RecuperarAmistades(int idJugador, string estado)
        {
            AmistadDAO amistadDAO = new AmistadDAO(new LaOcaDataAccess.LaOcaBDEntities());
            List<Amistad> listaAmistades = amistadDAO.RecuperarAmistadesDeJugador(idJugador, estado);

            return listaAmistades;
        }

        public Amistad RecuperarAmistad(int idJugadorSolicitante, int idJugadorReceptor)
        {
            AmistadDAO amistadDAO = new AmistadDAO(new LaOcaDataAccess.LaOcaBDEntities());
            Amistad amistadExistente = amistadDAO.RecuperarAmistad(idJugadorSolicitante, idJugadorReceptor);

            return amistadExistente;
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
