using System;
using System.Linq;
using Xunit;
using LaOcaService.DAOs.AspectoFolder;
using LaOcaDataAccess;
using LaOcaService;
using System.Data.Entity;
using System.ServiceModel;
using System.Collections.Generic;

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
        public void PruebaCrearAspectoExitoso()
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
        public void PruebaCrearAspectoFallido()
        {
            var aspectoInvalido = new Aspecto
            {
                Tipo = "",
                Referencia = ""
            };

            var ex = Assert.Throws<FaultException<AspectoException>>(() => _aspectoDAO.CrearAspecto(aspectoInvalido));
            var mensaje = ex.Detail.Mensaje;

            Assert.True(mensaje.Contains("El campo 'Tipo' es requerido.") && mensaje.Contains("El campo 'Referencia' es requerido."),
                        "El mensaje no contiene todos los errores esperados.");
        }

        [Fact]
        public void PruebaObtenerAspectoPorIdExitoso()
        {
            var aspecto = _aspectoDAO.ObtenerAspectoPorId(_idAspectoPrueba);
            Assert.True(aspecto != null && aspecto.Referencia == "referenciaPrueba.jpg");
        }

        [Fact]
        public void PruebaObtenerAspectoPorIdFallido()
        {
            var ex = Assert.Throws<KeyNotFoundException>(() => _aspectoDAO.ObtenerAspectoPorId(-1));
            Assert.Equal("Aspecto con ID -1 no encontrado.", ex.Message);
        }

        [Fact]
        public void PruebaModificarAspectoExitoso()
        {
            var aspectoOriginal = _aspectoDAO.ObtenerAspectoPorId(_idAspectoPrueba);
            aspectoOriginal.Tipo = "NuevoTipo";
            aspectoOriginal.Referencia = "nuevaReferencia.jpg";

            _aspectoDAO.ModificarAspecto(aspectoOriginal);

            var aspectoModificado = _contexto.Aspectos.FirstOrDefault(a => a.IdAspecto == _idAspectoPrueba);
            Assert.True(aspectoModificado != null && aspectoModificado.tipo == "NuevoTipo" && aspectoModificado.referencia == "nuevaReferencia.jpg");
        }

        [Fact]
        public void PruebaModificarAspectoFallido()
        {
            var aspectoInexistente = new Aspecto
            {
                IdAspecto = -1,
                Tipo = "TipoInvalido",
                Referencia = "ReferenciaInvalida.jpg"
            };

            var ex = Assert.Throws<KeyNotFoundException>(() => _aspectoDAO.ModificarAspecto(aspectoInexistente));
            Assert.Equal("Aspecto con ID -1 no encontrado.", ex.Message);
        }

        public void Dispose()
        {
            _transaccion.Rollback();
            _transaccion.Dispose();
            _contexto.Dispose();
        }
    }
}
