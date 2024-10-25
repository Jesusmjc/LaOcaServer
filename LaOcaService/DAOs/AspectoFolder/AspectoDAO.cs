using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaOcaDataAccess;

namespace LaOcaService.DAOs.AspectoFolder
{
    internal class AspectoDAO : IAspectoDAO
    {
        public AspectoDAO() {}
        public void CrearAspecto(Aspecto aspecto)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var aspectoBD = new Aspectos
                {
                    idAspecto = aspecto.idAspecto,
                    tipo = aspecto.tipo,
                    referencia = aspecto.referencia
                };
                contexto.Aspectos.Add(aspectoBD);
                contexto.SaveChanges();
            }
        }

        public Aspecto ObtenerAspectoPorId(int idAspecto)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var aspectoBD = contexto.Aspectos.FirstOrDefault(a => a.idAspecto == idAspecto);
                if (aspectoBD == null)
                {
                    Console.WriteLine($"Aspecto con ID {idAspecto} no encontrado.");
                    return null;
                }

                Console.WriteLine($"Aspecto encontrado: ID = {aspectoBD.idAspecto}, Referencia = {aspectoBD.referencia}, Tipo = {aspectoBD.tipo}");

                return new Aspecto
                {
                    idAspecto = aspectoBD.idAspecto,
                    referencia = aspectoBD.referencia,
                    tipo = aspectoBD.tipo
                };
            }
        }

        public void ModificarAspecto(Aspecto aspecto)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var aspectoBD = contexto.Aspectos.Find(aspecto.idAspecto);
                if (aspectoBD == null)
                {
                    return;
                }

                aspectoBD.referencia = aspecto.referencia;
                aspectoBD.tipo = aspecto.tipo;
                contexto.SaveChanges();
            }
        }
    }
}
