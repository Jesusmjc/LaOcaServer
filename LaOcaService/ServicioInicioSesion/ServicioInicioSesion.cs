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
            InicioSesionDAO inicioSesionDAO = new InicioSesionDAO(new LaOcaDataAccess.LaOcaBDEntities());

            Jugador jugadorInicioSesion = inicioSesionDAO.IniciarSesion(cuentaUsuario);

            if (jugadorInicioSesion.IdJugador == 0)
            {
                throw new FaultException<InicioSesionException>(
                    new InicioSesionException(),
                    new FaultReason("Credenciales incorrectas.")
                );
            }

            if (_ListaJugadoresConectados.ContainsKey(jugadorInicioSesion.NombreUsuario))
            {
                throw new FaultException<InicioSesionException>(
                    new InicioSesionException("No puedes iniciar otra sesión."),
                    new FaultReason("Parece que ya iniciaste sesión desde otro dispositivo. Si no eres tú, por favor contacta a nuestro equipo de desarrollo.")
                );
            }

            return jugadorInicioSesion;
        }
    }
}
