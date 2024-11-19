using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService.DAOs.AmistadFolder
{
    internal interface IAmistadDAO
    {
        Amistad RecuperarAmistad(int idJugadorSolicitante, int idJugadorReceptor);

        int RegistrarNuevaAmistad(Amistad nuevaAmistad);

        int ActualizarEstadoAmistad(Amistad amistad);

        List<Amistad> RecuperarAmistadesDeJugador(int idJugador, string estado);
    }
}
