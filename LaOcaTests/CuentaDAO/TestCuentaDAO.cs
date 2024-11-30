using System;
using System.Linq;
using Xunit;
using LaOcaDataAccess;
using LaOcaService.DAOs;
using LaOcaService.DAOs.CuentaFolder;
using LaOcaService;

namespace LaOcaTests.CuentaDAO
{
    public class TestCuentaDAO : IDisposable
    {
        private readonly LaOcaBDEntities contexto;
        private LaOcaService.DAOs.CuentaDAO cuentaDAO;
        private int idCuentaPrueba;

        public TestCuentaDAO()
        {
            contexto = new LaOcaBDEntities();
            cuentaDAO = new LaOcaService.DAOs.CuentaDAO();
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
            contexto.Cuentas.Add(cuenta);
            contexto.SaveChanges();
            idCuentaPrueba = cuenta.IdCuenta;
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

            cuentaDAO.CrearCuenta(nuevaCuenta);

            var cuentaBD = contexto.Cuentas.Find(nuevaCuenta.IdCuenta);
            Assert.True(cuentaBD != null && cuentaBD.correoElectronico == "nuevo@correo.com");
        }

        [Fact]
        public void PruebaModificarCuenta()
        {
            var cuenta = new Cuenta
            {
                IdCuenta = idCuentaPrueba,
                CorreoElectronico = "modificado@correo.com",
                Contrasena = "contrasenaModificada"
            };

            cuentaDAO.ModificarCuenta(cuenta);

            using (var contexto = new LaOcaBDEntities())
            {
                var cuentaBD = contexto.Cuentas.Find(idCuentaPrueba);
                Assert.True(cuentaBD != null && cuentaBD.correoElectronico == "modificado@correo.com" && cuentaBD.contrasena == "contrasenaModificada");
            }
        }

        [Fact]
        public void PruebaObtenerCuentaPorId()
        {
            var cuenta = cuentaDAO.ObtenerCuentaPorId(idCuentaPrueba);
            Assert.True(cuenta != null && cuenta.CorreoElectronico == "prueba@correo.com");
        }

        [Fact]
        public void PruebaObtenerCuentaPorCorreo()
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var cuentasExistentes = contexto.Cuentas.Where(c => c.correoElectronico == "prueba@correo.com");
                contexto.Cuentas.RemoveRange(cuentasExistentes);
                contexto.SaveChanges();
            }

            int cuentaId;
            using (var contexto = new LaOcaBDEntities())
            {
                var cuenta = new Cuentas
                {
                    correoElectronico = "prueba@correo.com",
                    contrasena = "contrasenaPrueba",
                    IdJugador = 1
                };
                contexto.Cuentas.Add(cuenta);
                contexto.SaveChanges();

                cuentaId = cuenta.IdCuenta;
            }

            var cuentaObtenida = cuentaDAO.ObtenerCuentaPorCorreo("prueba@correo.com");

            Assert.True(cuentaObtenida != null && cuentaObtenida.CorreoElectronico == "prueba@correo.com" && cuentaObtenida.IdCuenta == cuentaId);

            Console.WriteLine($"Cuenta obtenida: IdCuenta = {cuentaObtenida.IdCuenta}, Correo = {cuentaObtenida.CorreoElectronico}");
        }

        [Fact]
        public void PruebaCorreoExiste()
        {
            var existe = cuentaDAO.CorreoExiste("prueba@correo.com");
            Assert.True(existe);
        }

        public void Dispose()
        {
            contexto.Dispose();
        }
    }

}