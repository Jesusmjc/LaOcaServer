using Xunit;
using System;
using LaOcaService;
using LaOcaDataAccess;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;

namespace LaOcaTests.AmistadDAO
{
    public class TestAmistadDAOExcepcion
    {
        private readonly LaOcaBDEntities _contexto;
        private LaOcaService.DAOs.AmistadDAO _amistadDAO;

        public TestAmistadDAOExcepcion()
        {
            _contexto = new LaOcaBDEntities();
            _amistadDAO = new LaOcaService.DAOs.AmistadDAO(_contexto);
        }

        [Fact]
        public void PruebaRecuperarAmistadExcepcion()
        {
            var excepcion = Assert.Throws<FaultException<AmistadException>>(
                                          () => _amistadDAO.RecuperarAmistad(1, 2));

            Assert.Equal("Ocurrió un error al conectar con la Base de Datos. ", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaRegistrarNuevaAmistadExcepcion()
        {
            Amistad nuevaAmistad = new Amistad
            {
                Estado = "Solicitud",
                Fecha = DateTime.Now,
                IdJugadorSolicitante = 1,
                IdJugadorReceptor = 2
            };

            var excepcion = Assert.Throws<FaultException<AmistadException>>(
                                          () => _amistadDAO.RegistrarNuevaAmistad(nuevaAmistad));

            Assert.Equal("Ocurrió un error al conectar con la Base de Datos. ", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaActualizarEstadoAmistadExcepcion()
        {
            Amistad amistadActualizada = new Amistad
            {
                IdAmistad = 1,
                Estado = "Amigos",
                Fecha = DateTime.Now,
            };

            var excepcion = Assert.Throws<FaultException<AmistadException>>(
                                          () => _amistadDAO.ActualizarEstadoAmistad(amistadActualizada));

            Assert.Equal("Ocurrió un error al conectar con la Base de Datos. ", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaRecuperarAmistadesDeJugadorExcepcion()
        {
            var excepcion = Assert.Throws<FaultException<AmistadException>>(
                                          () => _amistadDAO.RecuperarAmistadesDeJugador(1, "Solicitud"));

            Assert.Equal("Ocurrió un error al conectar con la Base de Datos. ", excepcion.Detail.Mensaje);
        }
    }
}
