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
        public int idCuenta { get; set; }
        [DataMember]
        public string correoElectronico { get; set; }
        [DataMember]
        public string contrasena { get; set; }
        [DataMember]
        public int idJugador { get; set; }
    }

    [DataContract]
    public class Jugador
    {
        [DataMember]
        public int idJugador { get; set; }
        [DataMember]
        public string nombreUsuario { get; set; }
        [DataMember]
        public int idFotoPerfil { get; set; }
        [DataMember]
        public int idPuntuacion { get; set; }
        [DataMember]
        public int idCuenta { get; set; }
    }

    [DataContract]
    public class Aspecto
    {
        [DataMember]
        public int idAspecto { get; set; }
        [DataMember]
        public string tipo { get; set; }
        [DataMember]
        public string referencia { get; set; }
    }
}
