using Xunit;
using System;
using LaOcaService;
using LaOcaDataAccess;
using System.ServiceModel;

namespace LaOcaTests.CuentaDAO
{
    public class TestCuentaDAOExcepcion
    {
        private readonly LaOcaBDEntities _contexto;
        private LaOcaService.DAOs.CuentaFolder.CuentaDAO _cuentaDAO;

        public TestCuentaDAOExcepcion()
        {
            _contexto = new LaOcaBDEntities();
            _cuentaDAO = new LaOcaService.DAOs.CuentaFolder.CuentaDAO(_contexto);
        }

        [Fact]
        public void PruebaCrearCuentaExcepcion()
        {
            Cuenta cuenta = new Cuenta
            {
                CorreoElectronico = "correo@ejemplo.com",
                Contrasena = "123456",
                IdJugador = 999
            };

            var excepcion = Assert.Throws<FaultException<CuentaException>>(() =>
            {
                _cuentaDAO.CrearCuenta(cuenta);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaModificarCuentaExcepcion()
        {
            Cuenta cuenta = new Cuenta
            {
                IdCuenta = 999,
                CorreoElectronico = "correo@actualizado.com",
                Contrasena = "nuevaContraseña"
            };

            var excepcion = Assert.Throws<FaultException<CuentaException>>(() =>
            {
                _cuentaDAO.ModificarCuenta(cuenta);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaObtenerCuentaPorIdExcepcion()
        {
            int idCuentaInexistente = 999;

            var excepcion = Assert.Throws<FaultException<CuentaException>>(() =>
            {
                _cuentaDAO.ObtenerCuentaPorId(idCuentaInexistente);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaObtenerCuentaPorCorreoExcepcion()
        {
            string correoInexistente = "noexiste@correo.com";

            var excepcion = Assert.Throws<FaultException<CuentaException>>(() =>
            {
                _cuentaDAO.ObtenerCuentaPorCorreo(correoInexistente);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }

        [Fact]
        public void PruebaCorreoExisteExcepcion()
        {
            string correoInexistente = "noexiste@correo.com";

            var excepcion = Assert.Throws<FaultException<CuentaException>>(() =>
            {
                _cuentaDAO.CorreoExiste(correoInexistente);
            });

            Assert.Contains("Ocurrió un error al conectar con la Base de Datos.", excepcion.Detail.Mensaje);
        }
    }
}
