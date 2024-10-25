using Xunit;
using LaOcaService.DAOs.AspectoFolder;
using Moq;
using LaOcaDataAccess;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LaOcaService;

public class AspectoDAOTests
{
    [Fact]
    public void CrearAspecto_AspectoNuevo_CreaAspecto()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetAspectos = new Mock<DbSet<Aspectos>>();
        mockContext.Setup(m => m.Aspectos).Returns(mockSetAspectos.Object);

        var aspectoDAO = new AspectoDAO();
        var aspecto = new Aspecto { IdAspecto = 1, Tipo = "FotoPerfil", Referencia = "imagen.jpg" };

        // Act
        aspectoDAO.CrearAspecto(aspecto);

        // Assert
        mockSetAspectos.Verify(m => m.Add(It.IsAny<Aspectos>()), Times.Once());
        mockContext.Verify(m => m.SaveChanges(), Times.Once());
    }

    [Fact]
    public void ObtenerAspectoPorId_AspectoExiste_RetornaAspecto()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetAspectos = new Mock<DbSet<Aspectos>>();
        var aspectoBD = new Aspectos { IdAspecto = 1, tipo = "FotoPerfil", referencia = "imagen.jpg" };

        mockSetAspectos.Setup(m => m.Find(1)).Returns(aspectoBD);
        mockContext.Setup(m => m.Aspectos).Returns(mockSetAspectos.Object);

        var aspectoDAO = new AspectoDAO();

        // Act
        var result = aspectoDAO.ObtenerAspectoPorId(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.IdAspecto);
        Assert.Equal("FotoPerfil", result.Tipo);
        Assert.Equal("imagen.jpg", result.Referencia);
    }

    [Fact]
    public void ObtenerAspectoPorId_AspectoNoExiste_LanzaExcepcion()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetAspectos = new Mock<DbSet<Aspectos>>();

        mockSetAspectos.Setup(m => m.Find(1)).Returns((Aspectos)null);
        mockContext.Setup(m => m.Aspectos).Returns(mockSetAspectos.Object);

        var aspectoDAO = new AspectoDAO();

        // Act & Assert
        var exception = Assert.Throws<KeyNotFoundException>(() => aspectoDAO.ObtenerAspectoPorId(1));
        Assert.Equal("Aspecto con ID 1 no encontrado.", exception.Message);
    }

    [Fact]
    public void ModificarAspecto_AspectoExiste_ModificaAspecto()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetAspectos = new Mock<DbSet<Aspectos>>();
        var aspectoBD = new Aspectos { IdAspecto = 1, tipo = "FotoPerfil", referencia = "imagen.jpg" };

        mockSetAspectos.Setup(m => m.Find(1)).Returns(aspectoBD);
        mockContext.Setup(m => m.Aspectos).Returns(mockSetAspectos.Object);

        var aspectoDAO = new AspectoDAO();
        var aspecto = new Aspecto { IdAspecto = 1, Tipo = "Icono", Referencia = "icono.jpg" };

        // Act
        aspectoDAO.ModificarAspecto(aspecto);

        // Assert
        Assert.Equal("Icono", aspectoBD.tipo);
        Assert.Equal("icono.jpg", aspectoBD.referencia);
        mockContext.Verify(m => m.SaveChanges(), Times.Once());
    }
}
