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
        /// <summary>
        /// Recupera la información guardada en la base de datos del jugador asociado a la cuenta proporcionada
        /// </summary>
        /// <param name="cuentaUsuario"> Correo electrónico y contraseña del jugador intentando inciar sesión </param>
        /// <returns>
        /// La información del jugador en caso de que exista una cuenta con las credenciales dadas,
        /// una excepción personalizada con un mensaje de error en caso contrario
        /// </returns>
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
