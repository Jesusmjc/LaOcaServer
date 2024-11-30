using Xunit;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using LaOcaService;
using LaOcaDataAccess;

namespace LaOcaTests.CuentaDAO
{
    public class TestCuentaDAOExcepcion
    {
        private readonly LaOcaBDEntities _contexto;
        private LaOcaService.DAOs.CuentaDAO _cuentaDAO;

        public TestCuentaDAOExcepcion()
        {
            _contexto = new LaOcaBDEntities();
            _cuentaDAO = new LaOcaService.DAOs.CuentaDAO();
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

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _cuentaDAO.CrearCuenta(cuenta);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
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

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _cuentaDAO.ModificarCuenta(cuenta);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }

        [Fact]
        public void PruebaObtenerCuentaPorIdExcepcion()
        {
            int idCuentaInexistente = 999;

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _cuentaDAO.ObtenerCuentaPorId(idCuentaInexistente);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }

        [Fact]
        public void PruebaObtenerCuentaPorCorreoExcepcion()
        {
            string correoInexistente = "noexiste@correo.com";

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _cuentaDAO.ObtenerCuentaPorCorreo(correoInexistente);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }

        [Fact]
        public void PruebaCorreoExisteExcepcion()
        {
            string correoInexistente = "noexiste@correo.com";

            var excepcion = Assert.Throws<EntityException>(() =>
            {
                _cuentaDAO.CorreoExiste(correoInexistente);
            });

            Assert.Contains("Error relacionado con la red o específico de la instancia", excepcion.ToString());
        }
    }
}
