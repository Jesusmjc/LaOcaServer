using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace LaOcaService
{
    [ServiceContract]
    public interface IServicioCuenta
    {
        [OperationContract]
        void CrearCuenta(Cuenta cuenta, Jugador jugador, string referenciaImagen);

        [OperationContract]
        void ModificarCuenta(Cuenta cuenta);

        [OperationContract]
        void ModificarJugador(Jugador jugador);

        [OperationContract]
        void ModificarAspecto(Aspecto aspecto);

        [OperationContract]
        Cuenta ObtenerCuentaPorId(int idCuenta);

        [OperationContract]
        Jugador ObtenerJugadorPorId(int idJugador);

        [OperationContract]
        void CrearAspecto(Aspecto aspecto);

        [OperationContract]
        Aspecto ObtenerAspectoPorId(int idAspecto);

        [OperationContract]
        void EnviarCodigoVerificacion(string correoElectronico);

        [OperationContract]
        bool VerificarCodigoCrearCuenta(string correo, string codigo);

        [OperationContract]
        int VerificarCodigoRecuperarContraseña(string correo, string codigo);

        [OperationContract]
        bool VerificarContraseñaActual(int idCuenta, string contraseñaActual);

        [OperationContract]
        void SolicitarRecuperacionContrasena(string correoElectronico);

        [OperationContract]
        void ModificarContraseña(int idCuenta, string nuevaContrasena);

        [OperationContract]
        Cuenta ObtenerCuentaPorCodigoVerificacion(string codigoVerificacion);

        [OperationContract]
        bool CorreoExiste(string correoElectronico);

        [OperationContract]
        bool NombreUsuarioExisteCrear(string nombreUsuario);

        [OperationContract]
        bool NombreUsuarioExisteModificar(string nombreUsuario, int idJugadorActual);

        [OperationContract]
        void SincronizarAspectos(Dictionary<string, int> referenciaToIdMap);
    }

    [DataContract]
    public class Cuenta
    {
        [DataMember]
        public int IdCuenta { get; set; }
        [DataMember]
        public string CorreoElectronico { get; set; }
        [DataMember]
        public string Contrasena { get; set; }
        [DataMember]
        public int IdJugador { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null | GetType() != obj.GetType())
            {
                return false;
            }

            Cuenta cuenta = (Cuenta)obj;

            return IdCuenta == cuenta.IdCuenta && CorreoElectronico == cuenta.CorreoElectronico;
        }

        public override int GetHashCode()
        {
            return (IdCuenta, CorreoElectronico).GetHashCode();
        }
    }

    [DataContract]
    public class Jugador
    {
        [DataMember]
        public int IdJugador { get; set; }
        [DataMember]
        public string NombreUsuario { get; set; }
        [DataMember]
        public int CasillasRecorridas { get; set; }
        [DataMember]
        public int UltimaPosicion { get; set; }
        [DataMember]
        public bool HaLlegadoAMeta { get; set; }
        [DataMember]
        public int IdFotoPerfil { get; set; }
        [DataMember]
        public int IdPuntuacion { get; set; }
        [DataMember]
        public int IdCuenta { get; set; }
        [DataMember]
        public Ficha Ficha { get; set; }
        [DataMember]
        public int TurnosPerdidos { get; set; }
        [DataMember]
        public  List<InvitacionPartida> Invitaciones { get; set; }

        [DataMember]
        public List<Amistad> Amistades { get; set; }

        [DataMember]
        public IChatCallback CanalCallbackChat { get; set; }
        [DataMember]
        public ISalaCallback CanalCallbackSala { get; set; }
        [DataMember]
        public IPartidaCallback CanalCallbackPartida { get; set; }
        [DataMember]
        public string FichaAsignada { get; set; }

        [DataMember]
        public IJugadoresEnLineaCallback CanalCallbackJugadoresEnLinea { get; set; }

        [DataMember]
        public IBuzonCallback CanalCallbackBuzon { get; set; }

        public Jugador()
        {
            Invitaciones = new List<InvitacionPartida>();
            Amistades = new List<Amistad>();
            CasillasRecorridas = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Jugador jugador = (Jugador)obj;

            return IdJugador == jugador.IdJugador && 
                   NombreUsuario == jugador.NombreUsuario &&
                   IdFotoPerfil == jugador.IdFotoPerfil &&
                   IdPuntuacion == jugador.IdPuntuacion &&
                   IdCuenta == jugador.IdCuenta;
        }

        public override int GetHashCode()
        {
            return (IdJugador, NombreUsuario).GetHashCode();
        }
    }

    [DataContract]
    public class JugadorException
    {
        [DataMember]
        public string Mensaje { get; set; }

        public JugadorException()
        {
            Mensaje = "Error en la operación";
        }

        public JugadorException(string mensaje)
        {
            Mensaje = mensaje;
        }
    }

    [DataContract]
    public class CuentaException
    {
        [DataMember]
        public string Mensaje { get; set; }

        public CuentaException()
        {
            Mensaje = "Error en la operación";
        }

        public CuentaException(string mensaje)
        {
            Mensaje = mensaje;
        }
    }

    [DataContract]
    public class AspectoException
    {
        [DataMember]
        public string Mensaje { get; set; }

        public AspectoException()
        {
            Mensaje = "Error en la operación";
        }

        public AspectoException(string mensaje)
        {
            Mensaje = mensaje;
        }
    }

    [DataContract]
    public class PuntuacionException
    {
        [DataMember]
        public string Mensaje { get; set; }

        public PuntuacionException()
        {
            Mensaje = "Error en la operación";
        }

        public PuntuacionException(string mensaje)
        {
            Mensaje = mensaje;
        }
    }

        [DataContract]
    public class Aspecto
    {
        [DataMember]
        public int IdAspecto { get; set; }
        [DataMember]
        public string Tipo { get; set; }
        [DataMember]
        public string Referencia { get; set; }
    }
}
