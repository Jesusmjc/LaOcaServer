using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaOcaDataAccess;

namespace LaOcaService.DAOs.AspectoFolder
{
    public class AspectoDAO : IAspectoDAO
    {
        //private readonly LaOcaBDEntities contexto;
        public AspectoDAO() {}
        /*public AspectoDAO(LaOcaBDEntities contexto)
        {
            this.contexto = contexto;
        }*/

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
                    throw new KeyNotFoundException($"Aspecto con ID {idAspecto} no encontrado.");
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
            try
            {
                using (var contexto = new LaOcaBDEntities())
                {
                    var aspectoBD = contexto.Aspectos.Find(aspecto.IdAspecto);
                    if (aspectoBD == null)
                    {
                        throw new KeyNotFoundException($"Aspecto con ID {aspecto.IdAspecto} no encontrado.");
                    }

                    aspectoBD.referencia = aspecto.Referencia;
                    aspectoBD.tipo = aspecto.Tipo;
                    contexto.SaveChanges();
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                    }
                }
            }
        }

    }
}
