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
    }

    [DataContract]
    public class Jugador
    {
        [DataMember]
        public int IdJugador { get; set; }
        [DataMember]
        public string NombreUsuario { get; set; }
        [DataMember]
        public int IdFotoPerfil { get; set; }
        [DataMember]
        public int IdPuntuacion { get; set; }
        [DataMember]
        public int IdCuenta { get; set; }

        [DataMember]
        public IChatCallback CanalCallbackChat { get; set; }

        [DataMember]
        public ISalaCallback CanalCallbackSala { get; set; }
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
