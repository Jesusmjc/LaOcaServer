using Xunit;
using LaOcaService.DAOs;
using Moq;
using LaOcaDataAccess;
using System.Collections.Generic;
using LaOcaService;

public class CuentaDAOTests
{
    [Fact]
    public void ObtenerCuentaPorId_CuentaExiste_RetornaCuenta()
    {
        var mockContext = new Mock<LaOcaBDEntities>();
        var cuentaDAO = new CuentaDAO();
        var cuenta = new Cuentas { IdCuenta = 1, correoElectronico = "test@example.com", contrasena = "password", IdJugador = 1 };
        mockContext.Setup(m => m.Cuentas.Find(1)).Returns(cuenta);

        var result = cuentaDAO.ObtenerCuentaPorId(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.IdCuenta);
        Assert.Equal("test@example.com", result.CorreoElectronico);
    }

    [Fact]
    public void ObtenerCuentaPorId_CuentaNoExiste_RetornaNull()
    {
        var mockContext = new Mock<LaOcaBDEntities>();
        var cuentaDAO = new CuentaDAO();
        mockContext.Setup(m => m.Cuentas.Find(1)).Returns((Cuentas)null);

        var result = cuentaDAO.ObtenerCuentaPorId(1);

        Assert.Null(result);
    }

    [Fact]
    public void ModificarCuenta_CuentaNoExiste_LanzaExcepcion()
    {
        var mockContext = new Mock<LaOcaBDEntities>();
        var cuentaDAO = new CuentaDAO();
        var cuenta = new Cuenta { IdCuenta = 1, CorreoElectronico = "test@example.com", Contrasena = "password", IdJugador = 1 };
        mockContext.Setup(m => m.Cuentas.Find(1)).Returns((Cuentas)null);

        var exception = Assert.Throws<KeyNotFoundException>(() => cuentaDAO.ModificarCuenta(cuenta));
        Assert.Equal("Cuenta con ID 1 no encontrada.", exception.Message);
    }
}
