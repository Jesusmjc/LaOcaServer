using Xunit;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using LaOcaService;
using LaOcaDataAccess;

namespace LaOcaTests.AspectoDAO
{
    public class TestAspectoDAOExcepcion
    {
        private readonly LaOcaBDEntities _contexto;
        private LaOcaService.DAOs.AspectoFolder.AspectoDAO _aspectoDAO;

        public TestAspectoDAOExcepcion()
        {
            _contexto = new LaOcaBDEntities();
            _aspectoDAO = new LaOcaService.DAOs.AspectoFolder.AspectoDAO();
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

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _aspectoDAO.CrearAspecto(aspecto);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }

        [Fact]
        public void PruebaObtenerAspectoPorIdExcepcion()
        {
            int idAspectoInexistente = 999;

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _aspectoDAO.ObtenerAspectoPorId(idAspectoInexistente);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
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

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _aspectoDAO.ModificarAspecto(aspecto);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }

        [Fact]
        public void PruebaModificarAspectoValidacionExcepcion()
        {
            Aspecto aspecto = new Aspecto
            {
                IdAspecto = 999,
                Tipo = null,
                Referencia = null
            };

            var excepcion = Assert.ThrowsAny<Exception>(() =>
            {
                _aspectoDAO.ModificarAspecto(aspecto);
            });

            Assert.True(excepcion is DbEntityValidationException || excepcion is EntityException);
            if (excepcion is DbEntityValidationException)
            {
                Assert.Contains("Error: ", excepcion.ToString());
            }
            else if (excepcion is EntityException)
            {
                Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
            }
        }

    }
}