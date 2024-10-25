using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService.DAOs.AspectoFolder
{
    public interface IAspectoDAO
    {
        void CrearAspecto(Aspecto aspecto);
        Aspecto ObtenerAspectoPorId(int idAspecto);
        void ModificarAspecto(Aspecto aspecto);
    }
}
