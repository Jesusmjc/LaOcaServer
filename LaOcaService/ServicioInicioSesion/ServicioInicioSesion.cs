using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using LaOcaService.DAOs;

namespace LaOcaService
{
    public partial class LaOcaService : IServicioInicioSesion
    {
        public Jugador IniciarSesion(Cuenta cuentaUsuario)
        {
            InicioSesionDAO inicioSesionDAO = new InicioSesionDAO();

            Jugador jugadorInicioSesion = new Jugador();

            jugadorInicioSesion = inicioSesionDAO.IniciarSesion(cuentaUsuario);

            if (jugadorInicioSesion.IdJugador == 0)
            {
                throw new FaultException<InicioSesionException>(
                    new InicioSesionException(),
                    new FaultReason("Credenciales incorrectas.")
                );
            }

            //try
            //{
            //    jugadorInicioSesion = inicioSesionDAO.IniciarSesion(cuentaUsuario);

            //    if (jugadorInicioSesion.IdJugador == 0)
            //    {
            //        throw new FaultException<InicioSesionException>(
            //            new InicioSesionException()
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //throw new FaultException<InicioSesionException>(
            //    //    new InicioSesionException(ex.Message),
            //    //    new FaultReason("Credenciales incorrectas.")
            //    //);

            //    Console.WriteLine( ex.ToString() );
            //}

            return jugadorInicioSesion;
        }
    }
}
