using System;
using System.Linq;
using Xunit;
using LaOcaDataAccess;
using LaOcaService.DAOs.CuentaFolder;
using LaOcaService;
using System.Data.Entity;

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
        public void PruebaCrearCuenta()
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
        public void PruebaModificarCuenta()
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
        public void PruebaObtenerCuentaPorId()
        {
            var cuenta = _cuentaDAO.ObtenerCuentaPorId(_idCuentaPrueba);
            Assert.True(cuenta != null && cuenta.CorreoElectronico == "prueba@correo.com");
        }

        [Fact]
        public void PruebaObtenerCuentaPorCorreo()
        {
            var cuenta = _cuentaDAO.ObtenerCuentaPorCorreo("prueba@correo.com");

            Assert.True(cuenta != null, "La cuenta no fue encontrada.");
            Assert.Equal("prueba@correo.com", cuenta.CorreoElectronico);
        }

        [Fact]
        public void PruebaCorreoExiste()
        {
            var existe = _cuentaDAO.CorreoExiste("prueba@correo.com");
            Assert.True(existe);
        }

        public void Dispose()
        {
            _transaccion.Rollback();
            _transaccion.Dispose();
            _contexto.Dispose();
        }
    }
}
