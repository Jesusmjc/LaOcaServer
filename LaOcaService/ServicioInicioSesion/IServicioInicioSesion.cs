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
    public class InicioSesionException
    {
        [DataMember]
        public string Mensaje { get; set; }

        public InicioSesionException() 
        {
            Mensaje = "No se ha encontrado una cuenta que coincida con las credenciales ingresadas.";
        }

        public InicioSesionException(string mensaje) 
        {
            Mensaje = mensaje;
        }
    }
}
