using System;
using System.Linq;
using Xunit;
using LaOcaService.DAOs.AspectoFolder;
using LaOcaDataAccess;
using LaOcaService;

namespace LaOcaTests.AspectoDAO
{
    public class TestAspectoDAO : IDisposable
    {
        private readonly LaOcaBDEntities contexto;
        private LaOcaService.DAOs.AspectoFolder.AspectoDAO aspectoDAO;
        private int idAspectoPrueba;

        public TestAspectoDAO()
        {
            contexto = new LaOcaBDEntities();
            aspectoDAO = new LaOcaService.DAOs.AspectoFolder.AspectoDAO();
            PrepararBaseDeDatos();
        }

        private void PrepararBaseDeDatos()
        {
            var aspecto = new Aspectos
            {
                tipo = "FotoPerfil",
                referencia = "referenciaPrueba.jpg"
            };
            contexto.Aspectos.Add(aspecto);
            contexto.SaveChanges();
            idAspectoPrueba = aspecto.IdAspecto;
        }

        [Fact]
        public void PruebaCrearAspecto()
        {
            var nuevoAspecto = new Aspecto
            {
                Tipo = "Icono",
                Referencia = "iconoPrueba.png"
            };

            aspectoDAO.CrearAspecto(nuevoAspecto);

            var aspectoBD = contexto.Aspectos.FirstOrDefault(a => a.referencia == "iconoPrueba.png");
            Assert.True(aspectoBD != null && aspectoBD.tipo == "Icono");
        }

        [Fact]
        public void PruebaObtenerAspectoPorId()
        {
            var aspecto = aspectoDAO.ObtenerAspectoPorId(idAspectoPrueba);
            Assert.True(aspecto != null && aspecto.Referencia == "referenciaPrueba.jpg");
        }

        public void Dispose()
        {
            var aspecto = contexto.Aspectos.Find(idAspectoPrueba);
            if (aspecto != null)
            {
                contexto.Aspectos.Remove(aspecto);
                contexto.SaveChanges();
            }
            contexto.Dispose();
        }
    }
}