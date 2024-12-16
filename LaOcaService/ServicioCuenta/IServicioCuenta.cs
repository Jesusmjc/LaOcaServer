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
        /// <summary>
        /// Guarda en la base de datos la información de un jugador y su cuenta asociada
        /// </summary>
        /// <param name="cuenta"> La información de la cuenta del jugador </param>
        /// <param name="jugador"> La información del jugador </param>
        /// <param name="referenciaImagen"> La referencia para asignar una foto de perfil al jugador </param>
        [OperationContract]
        void CrearCuenta(Cuenta cuenta, Jugador jugador, string referenciaImagen);

        /// <summary>
        /// Actualiza la información de la cuenta proporcionada en la base de datos
        /// </summary>
        /// <param name="cuenta"> La cuenta cuya información será actualizada </param>
        [OperationContract]
        void ModificarCuenta(Cuenta cuenta);

        /// <summary>
        /// Recupera la cuenta que coincida con el identificador proporcionado de la base de datos
        /// </summary>
        /// <param name="idCuenta"> El identificador de la cuenta </param>
        /// <returns> La información de la cuenta que coincide con el identificador </returns>
        [OperationContract]
        Cuenta ObtenerCuentaPorId(int idCuenta); 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCuenta"></param>
        /// <param name="contraseñaActual"></param>
        /// <returns></returns>
        [OperationContract]
        bool VerificarContraseñaActual(int idCuenta, string contraseñaActual);

        /// <summary>
        /// Envia un código al correo proporcionado para que el jugador pueda
        /// recuperar su contraseña
        /// </summary>
        /// <param name="correoElectronico"> El correo electrónico al que se enviará el código </param>
        [OperationContract]
        void SolicitarRecuperacionContrasena(string correoElectronico);

        /// <summary>
        /// Se actualiza la contraseña de la cuenta correspondiente
        /// </summary>
        /// <param name="idCuenta"> Identificador de la cuenta cuya constraseña será actualizada </param>
        /// <param name="nuevaContrasena"> El nuevo valor de la contraseña de la cuenta </param>
        [OperationContract]
        void ModificarContraseña(int idCuenta, string nuevaContrasena);

        /// <summary>
        /// Recupera la cuenta asociada al correo al cual se envió el código de verificación proporcionado
        /// </summary>
        /// <param name="codigoVerificacion"> Código de verificación enviado al correo electrónico </param>
        /// <returns> La información de la cuenta </returns>
        [OperationContract]
        Cuenta ObtenerCuentaPorCodigoVerificacion(string codigoVerificacion);

        /// <summary>
        /// Verifica si el correo electrónico ya está asociado a una cuenta
        /// </summary>
        /// <param name="correoElectronico"> Correo electrónico que será validado </param>
        /// <returns> true si el correo ya está asociado a una cuenta, false en caso contrario </returns>
        [OperationContract]
        bool CorreoExiste(string correoElectronico);

        /// <summary>
        /// Verifica si el nombre de usuario ya está en uso por otro jugador
        /// </summary>
        /// <param name="nombreUsuario"> Nombre de usuario que será validado </param>
        /// <returns> true si el nombre de usuario ya está ocupado, false en caso contrario </returns>
        [OperationContract]
        bool NombreUsuarioExisteCrear(string nombreUsuario);

        /// <summary>
        /// Verifica si ya existe otro usuario con el nombre de usuario proporcionado
        /// </summary>
        /// <param name="nombreUsuario"> El nombre de usuario que será validado </param>
        /// <param name="idJugadorActual"> El identificador del jugador que quiere usar el nombre </param>
        /// <returns> tr true si el nombre de usuario ya está ocupado, false en caso contrario </returns>
        [OperationContract]
        bool NombreUsuarioExisteModificar(string nombreUsuario, int idJugadorActual);

        /// <summary>
        /// Recupera de la base de datos una lista de jugadores que ocupan los primeros puestos
        /// dentro del ranking global
        /// </summary>
        /// <returns> Una lista con los 10 jugadores con las puntuaciones más altas </returns>
        [OperationContract]
        List<Jugador> ObtenerRankingGlobal();
    }

    [ServiceContract]
    public interface IServicioJugador
    {
        /// <summary>
        /// Modifica la información del jugador correspondiente en la base de datos
        /// </summary>
        /// <param name="jugador"> El jugador cuya información será modificada </param>
        [OperationContract]
        void ModificarJugador(Jugador jugador);

        /// <summary>
        /// Recupera la información de un jugador de la base de datos
        /// </summary>
        /// <param name="idJugador"> El identificador del jugador cuya información será recuperada </param>
        /// <returns> El jugador recuperado de la base de datos </returns>
        [OperationContract]
        Jugador ObtenerJugadorPorId(int idJugador);

        /// <summary>
        /// Recupera las estadísticas del jugador correspondiente de la base de datos
        /// </summary>
        /// <param name="idJugador"> El identificador del jugador cuyas estadísticas serán recuperadas </param>
        /// <returns> Una cadena con la concatenación de las casillas recorridas y victorias totales del jugador </returns>
        [OperationContract]
        string ConsultarEstadisticasJugador(int idJugador);
    }

    [ServiceContract]
    public interface IServicioAspecto
    {
        /// <summary>
        /// Modifica la información del aspecto correpondiente en la base de datos
        /// </summary>
        /// <param name="aspecto"> El aspecto cuya información será modificada </param>
        [OperationContract]
        void ModificarAspecto(Aspecto aspecto);

        /// <summary>
        /// Registra un nuevo aspecto en la base de datos
        /// </summary>
        /// <param name="aspecto"> El aspecto cuya información se guardará en la base de datos </param>
        [OperationContract]
        void CrearAspecto(Aspecto aspecto);

        /// <summary>
        /// Recupera la información de un aspecto de la base de datos.
        /// </summary>
        /// <param name="idAspecto"> El identificador del aspecto cuya información será recuperada </param>
        /// <returns> La información del aspecto </returns>
        [OperationContract]
        Aspecto ObtenerAspectoPorId(int idAspecto);

        /// <summary>
        /// Registra la información de uno o más fotos de perfil en la base de datos
        /// </summary>
        /// <param name="referenciaToIdMap"> Diccionario con las fotos de perfil que se guardarán en la base de datos </param>
        [OperationContract]
        void SincronizarAspectos(Dictionary<string, int> referenciaToIdMap);
    }

    [ServiceContract]
    public interface IServicioCodigo
    {
        /// <summary>
        /// Envía un código de verificación al correo proporcionado
        /// </summary>
        /// <param name="correoElectronico"> El correo electrónico al que se enviará el código de verificación </param>
        [OperationContract]
        void EnviarCodigoVerificacion(string correoElectronico);

        /// <summary>
        /// Valida si el código proporcionado es igual al que se envió al correo proporcionado
        /// </summary>
        /// <param name="correo"> El correo al que se envió el código </param>
        /// <param name="codigo"> El código que será validado </param>
        /// <returns> true si el código proporcionado coincide con el que se envió al correo, false en caso contrario </returns>
        [OperationContract]
        bool VerificarCodigoCrearCuenta(string correo, string codigo);

        /// <summary>
        /// Valida si el código proporcionado es igual al que se envió al correo proporcionado
        /// </summary>
        /// <param name="correo"> El correo al que se envió el código </param>
        /// <param name="codigo"> El código que será validado </param>
        /// <returns> 
        /// El identificador de la cuenta correspondiente al correo si el código coincide con el que se envió al correo,
        /// -1 en caso contrario.
        /// </returns>
        [OperationContract]
        int VerificarCodigoRecuperarContraseña(string correo, string codigo);
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
        public bool EsInvitado { get; set; }

        [DataMember]
        public int CasillasRecorridas { get; set; }
        [DataMember]
        public int PartidasGanadas { get; set; }
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
        public IActualizacionJugadoresEnSalaCallback CanalCallbackJugadoresEnSala { get; set; }

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
            PartidasGanadas = 0;
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
