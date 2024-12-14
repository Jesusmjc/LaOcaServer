using System;
using System.Linq;
using Xunit;
using LaOcaDataAccess;
using LaOcaService.DAOs.CuentaFolder;
using LaOcaService;
using System.Data.Entity;
using System.ServiceModel;
using System.Collections.Generic;

namespace LaOcaTests.CuentaDAO
{
    public class TestCuentaDAO : IDisposable
    {
        private readonly LaOcaBDEntities _contexto;
        private readonly DbContextTransaction _transaccion;
        private readonly LaOcaService.DAOs.CuentaFolder.CuentaDAO _cuentaDAO;
        private int _idCuentaPrueba;

        public TestCuentaDAO()
        {
            _contexto = new LaOcaBDEntities();
            _transaccion = _contexto.Database.BeginTransaction();
            _cuentaDAO = new LaOcaService.DAOs.CuentaFolder.CuentaDAO(_contexto);
            PrepararBaseDeDatos();
        }

        private void PrepararBaseDeDatos()
        {
            var cuenta = new Cuentas
            {
                correoElectronico = "prueba@correo.com",
                contrasena = "contrasenaPrueba",
                IdJugador = 1
            };
            _contexto.Cuentas.Add(cuenta);
            _contexto.SaveChanges();
            _idCuentaPrueba = cuenta.IdCuenta;
        }

        [Fact]
        public void PruebaCrearCuentaExitoso()
        {
            var nuevaCuenta = new Cuenta
            {
                CorreoElectronico = "nuevo@correo.com",
                Contrasena = "nuevaContrasena",
                IdJugador = 2
            };

            _cuentaDAO.CrearCuenta(nuevaCuenta);

            var cuentaBD = _contexto.Cuentas.FirstOrDefault(c => c.correoElectronico == "nuevo@correo.com");
            Assert.True(cuentaBD != null && cuentaBD.correoElectronico == "nuevo@correo.com");
        }

        [Fact]
        public void PruebaCrearCuentaFallido()
        {
            var cuentaInvalida = new Cuenta
            {
                CorreoElectronico = null,
                Contrasena = "sinCorreo",
                IdJugador = 3
            };

            var ex = Assert.Throws<FaultException<CuentaException>>(() => _cuentaDAO.CrearCuenta(cuentaInvalida));
            Assert.Contains("El campo 'CorreoElectronico' es requerido.", ex.Detail.Mensaje);
        }

        [Fact]
        public void PruebaModificarCuentaExitoso()
        {
            var cuenta = new Cuenta
            {
                IdCuenta = _idCuentaPrueba,
                CorreoElectronico = "modificado@correo.com",
                Contrasena = "contrasenaModificada"
            };

            _cuentaDAO.ModificarCuenta(cuenta);

            var cuentaBD = _contexto.Cuentas.FirstOrDefault(c => c.IdCuenta == _idCuentaPrueba);
            Assert.True(cuentaBD != null && cuentaBD.correoElectronico == "modificado@correo.com" && cuentaBD.contrasena == "contrasenaModificada");
        }

        [Fact]
        public void PruebaModificarCuentaFallido()
        {
            var cuentaInexistente = new Cuenta
            {
                IdCuenta = -1,
                CorreoElectronico = "correo@invalido.com",
                Contrasena = "invalida"
            };

            var ex = Assert.Throws<KeyNotFoundException>(() => _cuentaDAO.ModificarCuenta(cuentaInexistente));
            Assert.Contains("Cuenta con ID -1 no encontrada.", ex.Message);
        }

        [Fact]
        public void PruebaObtenerCuentaPorIdExitoso()
        {
            var cuenta = _cuentaDAO.ObtenerCuentaPorId(_idCuentaPrueba);
            Assert.True(cuenta != null && cuenta.CorreoElectronico == "prueba@correo.com");
        }

        [Fact]
        public void PruebaObtenerCuentaPorIdFallido()
        {
            var cuenta = _cuentaDAO.ObtenerCuentaPorId(-1); 

            Assert.Null(cuenta);
        }

        [Fact]
        public void PruebaObtenerCuentaPorCorreoExitoso()
        {
            var cuenta = _cuentaDAO.ObtenerCuentaPorCorreo("prueba@correo.com");

            Assert.True(cuenta != null, "La cuenta no fue encontrada.");
            Assert.Equal("prueba@correo.com", cuenta.CorreoElectronico);
        }

        [Fact]
        public void PruebaObtenerCuentaPorCorreoFallido()
        {
            var ex = Assert.Throws<KeyNotFoundException>(() => _cuentaDAO.ObtenerCuentaPorCorreo("correo@inexistente.com"));
            Assert.Contains("Cuenta con correo correo@inexistente.com no encontrada.", ex.Message);
        }

        [Fact]
        public void PruebaCorreoExisteExitoso()
        {
            var existe = _cuentaDAO.CorreoExiste("prueba@correo.com");
            Assert.True(existe);
        }

        [Fact]
        public void PruebaCorreoExisteFallido()
        {
            var existe = _cuentaDAO.CorreoExiste("correo@inexistente.com");

            Assert.False(existe);
        }

        public void Dispose()
        {
            _transaccion.Rollback();
            _transaccion.Dispose();
            _contexto.Dispose();
        }
    }
}
