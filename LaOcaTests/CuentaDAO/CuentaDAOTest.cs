using System;
using System.Linq;
using Xunit;
using LaOcaDataAccess;
using LaOcaService.DAOs;
using LaOcaService.DAOs.CuentaFolder;
using LaOcaService;

public class TestCuentaDAO : IDisposable
{
    private readonly LaOcaBDEntities contexto;
    private readonly CuentaDAO cuentaDAO;
    private int idCuentaPrueba;

    public TestCuentaDAO()
    {
        contexto = new LaOcaBDEntities();
        cuentaDAO = new CuentaDAO();
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
        Assert.NotNull(cuentaBD);
        Assert.Equal("nuevo@correo.com", cuentaBD.correoElectronico);
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

            Assert.Equal("modificado@correo.com", cuentaBD.correoElectronico);
            Assert.Equal("contrasenaModificada", cuentaBD.contrasena);
        }
    }

    [Fact]
    public void PruebaObtenerCuentaPorId()
    {
        var cuenta = cuentaDAO.ObtenerCuentaPorId(idCuentaPrueba);
        Assert.NotNull(cuenta);
        Assert.Equal("prueba@correo.com", cuenta.CorreoElectronico);
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

        Assert.NotNull(cuentaObtenida); 
        Assert.Equal("prueba@correo.com", cuentaObtenida.CorreoElectronico); 
        Assert.Equal(cuentaId, cuentaObtenida.IdCuenta); 

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
