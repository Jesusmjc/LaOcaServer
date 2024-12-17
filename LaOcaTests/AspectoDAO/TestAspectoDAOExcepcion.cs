using Xunit;
using System;
using LaOcaService;
using LaOcaDataAccess;
using System.ServiceModel;

namespace LaOcaTests.AspectoDAO
{
    public class TestAspectoDAOExcepcion
    {
        private readonly LaOcaBDEntities _contexto;
        private LaOcaService.DAOs.AspectoFolder.AspectoDAO _aspectoDAO;

        public TestAspectoDAOExcepcion()
        {
            _contexto = new LaOcaBDEntities();
            _aspectoDAO = new LaOcaService.DAOs.AspectoFolder.AspectoDAO(_contexto);
        }

        [Fact]
        public void PruebaCrearAspectoExcepcion()
        {
            Aspecto aspecto = new Aspecto
            {
                IdAspecto = 999,
                Tipo = "Icono",
                Referencia = "path/to/icon.png"
            };

            var excepcion = Assert.Throws<FaultException<AspectoException>>(() =>
            {
                _aspectoDAO.CrearAspecto(aspecto);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaObtenerAspectoPorIdExcepcion()
        {
            int idAspectoInexistente = 999;

            var excepcion = Assert.Throws<FaultException<AspectoException>>(() =>
            {
                _aspectoDAO.ObtenerAspectoPorId(idAspectoInexistente);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaModificarAspectoExcepcion()
        {
            Aspecto aspecto = new Aspecto
            {
                IdAspecto = 999,
                Tipo = "NuevoTipo",
                Referencia = "path/to/updated/icon.png"
            };

            var excepcion = Assert.Throws<FaultException<AspectoException>>(() =>
            {
                _aspectoDAO.ModificarAspecto(aspecto);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaModificarAspectoValidacionExcepcion()
        {
            Aspecto aspecto = new Aspecto
            {
                IdAspecto = 999,
                Tipo = "",
                Referencia = ""
            };

            var excepcion = Assert.Throws<FaultException<AspectoException>>(() =>
            {
                _aspectoDAO.ModificarAspecto(aspecto);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }
    }
}
