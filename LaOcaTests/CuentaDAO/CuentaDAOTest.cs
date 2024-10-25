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
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var cuentaDAO = new CuentaDAO();
        var cuenta = new Cuentas { IdCuenta = 1, correoElectronico = "test@example.com", contrasena = "password", IdJugador = 1 };
        mockContext.Setup(m => m.Cuentas.Find(1)).Returns(cuenta);

        // Act
        var result = cuentaDAO.ObtenerCuentaPorId(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.IdCuenta);
        Assert.Equal("test@example.com", result.CorreoElectronico);
    }

    [Fact]
    public void ObtenerCuentaPorId_CuentaNoExiste_RetornaNull()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var cuentaDAO = new CuentaDAO();
        mockContext.Setup(m => m.Cuentas.Find(1)).Returns((Cuentas)null);

        // Act
        var result = cuentaDAO.ObtenerCuentaPorId(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ModificarCuenta_CuentaNoExiste_LanzaExcepcion()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var cuentaDAO = new CuentaDAO();
        var cuenta = new Cuenta { IdCuenta = 1, CorreoElectronico = "test@example.com", Contrasena = "password", IdJugador = 1 };
        mockContext.Setup(m => m.Cuentas.Find(1)).Returns((Cuentas)null);

        // Act & Assert
        var exception = Assert.Throws<KeyNotFoundException>(() => cuentaDAO.ModificarCuenta(cuenta));
        Assert.Equal("Cuenta con ID 1 no encontrada.", exception.Message);
    }
}
