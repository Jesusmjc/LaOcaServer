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
    internal interface IServicioInicioSesion
    {
        [OperationContract]
        [FaultContract(typeof(InicioSesionException))]
        Jugador IniciarSesion(Cuenta cuentaUsuario);    
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
        public string Nombre { get; set; }

        [DataMember]
        public string ApellidoPaterno { get; set; }

        [DataMember]
        public string ApellidoMaterno { get; set; }

        [DataMember]
        public int IdPuntuacion { get; set; }

        [DataMember]
        public int IdFotoPerfil { get; set; }

        [DataMember]
        public int IdCuenta { get; set; }
    }

    [DataContract]
    //public class InicioSesionException : Exception
    //{
    //    public InicioSesionException() : base("No se encontró una cuenta con las credenciales ingresadas.") { }

    //    public InicioSesionException(string mensaje) : base(mensaje) { }

    //    public InicioSesionException(string mensaje, Exception innerException) : base(mensaje, innerException) { }

    //}

    public class InicioSesionException
    {
        [DataMember]
        public string Mensaje { get; set; }

        public InicioSesionException() 
        {
            Mensaje = "No se ha encontrado una cuenta que concida con las credenciales ingresadas.";
        }

        public InicioSesionException(string mensaje) 
        {
            Mensaje = mensaje;
        }
    }
}
