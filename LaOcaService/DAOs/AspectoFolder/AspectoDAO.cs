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
                    IdAspecto = aspecto.IdAspecto,
                    tipo = aspecto.Tipo,
                    referencia = aspecto.Referencia
                };
                contexto.Aspectos.Add(aspectoBD);
                contexto.SaveChanges();
            }
        }

        public Aspecto ObtenerAspectoPorId(int idAspecto)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var aspectoBD = contexto.Aspectos.FirstOrDefault(a => a.IdAspecto == idAspecto);
                if (aspectoBD == null)
                {
                    Console.WriteLine($"Aspecto con ID {idAspecto} no encontrado.");
                    return null;
                }

                Console.WriteLine($"Aspecto encontrado: ID = {aspectoBD.IdAspecto}, Referencia = {aspectoBD.referencia}, Tipo = {aspectoBD.tipo}");

                return new Aspecto
                {
                    IdAspecto = aspectoBD.IdAspecto,
                    Referencia = aspectoBD.referencia,
                    Tipo = aspectoBD.tipo
                };
            }
        }

        public void ModificarAspecto(Aspecto aspecto)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var aspectoBD = contexto.Aspectos.Find(aspecto.IdAspecto);
                if (aspectoBD == null)
                {
                    return;
                }

                aspectoBD.referencia = aspecto.Referencia;
                aspectoBD.tipo = aspecto.Tipo;
                contexto.SaveChanges();
            }
        }
    }
}
