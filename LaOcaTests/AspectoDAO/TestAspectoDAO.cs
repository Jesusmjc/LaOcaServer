using System;
using System.Linq;
using Xunit;
using LaOcaService.DAOs.AspectoFolder;
using LaOcaDataAccess;
using LaOcaService;
using System.Data.Entity;

namespace LaOcaTests.AspectoDAO
{
    public class TestAspectoDAO : IDisposable
    {
        private readonly LaOcaBDEntities _contexto;
        private readonly DbContextTransaction _transaccion;
        private readonly LaOcaService.DAOs.AspectoFolder.AspectoDAO _aspectoDAO;
        private int _idAspectoPrueba;

        public TestAspectoDAO()
        {
            _contexto = new LaOcaBDEntities();
            _transaccion = _contexto.Database.BeginTransaction();
            _aspectoDAO = new LaOcaService.DAOs.AspectoFolder.AspectoDAO(_contexto);
            PrepararBaseDeDatos();
        }

        private void PrepararBaseDeDatos()
        {
            var aspecto = new Aspectos
            {
                tipo = "FotoPerfil",
                referencia = "referenciaPrueba.jpg"
            };
            _contexto.Aspectos.Add(aspecto);
            _contexto.SaveChanges();
            _idAspectoPrueba = aspecto.IdAspecto;
        }

        [Fact]
        public void PruebaCrearAspecto()
        {
            var nuevoAspecto = new Aspecto
            {
                Tipo = "Icono",
                Referencia = "iconoPrueba.png"
            };

            _aspectoDAO.CrearAspecto(nuevoAspecto);

            var aspectoBD = _contexto.Aspectos.FirstOrDefault(a => a.referencia == "iconoPrueba.png");
            Assert.True(aspectoBD != null && aspectoBD.tipo == "Icono");
        }

        [Fact]
        public void PruebaObtenerAspectoPorId()
        {
            var aspecto = _aspectoDAO.ObtenerAspectoPorId(_idAspectoPrueba);
            Assert.True(aspecto != null && aspecto.Referencia == "referenciaPrueba.jpg");
        }

        public void Dispose()
        {
            _transaccion.Rollback();
            _transaccion.Dispose();
            _contexto.Dispose();
        }
    }
}
