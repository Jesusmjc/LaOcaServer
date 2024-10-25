using Xunit;
using LaOcaService.DAOs.JugadorFolder;
using Moq;
using LaOcaDataAccess;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LaOcaService;

public class JugadorDAOTests
{
    [Fact]
    public void CrearJugador_CuentaNoExiste_LanzaExcepcion()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetCuentas = new Mock<DbSet<Cuentas>>();
        var mockSetAspectos = new Mock<DbSet<Aspectos>>();
        var mockSetPuntuaciones = new Mock<DbSet<Puntuaciones>>();
        var mockSetJugadores = new Mock<DbSet<Jugadores>>();

        mockContext.Setup(m => m.Cuentas).Returns(mockSetCuentas.Object);
        mockContext.Setup(m => m.Aspectos).Returns(mockSetAspectos.Object);
        mockContext.Setup(m => m.Puntuaciones).Returns(mockSetPuntuaciones.Object);
        mockContext.Setup(m => m.Jugadores).Returns(mockSetJugadores.Object);

        var jugadorDAO = new JugadorDAO();
        var jugador = new Jugador { IdCuenta = 1, NombreUsuario = "testUser", IdFotoPerfil = 1, IdPuntuacion = 1 };

        mockSetCuentas.Setup(m => m.Find(1)).Returns((Cuentas)null);

        // Act & Assert
        var exception = Assert.Throws<KeyNotFoundException>(() => jugadorDAO.CrearJugador(jugador, "imagen.jpg"));
        Assert.Equal("La cuenta con id 1 no existe.", exception.Message);
    }

    [Fact]
    public void ObtenerJugadorPorId_JugadorExiste_RetornaJugador()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetJugadores = new Mock<DbSet<Jugadores>>();
        var jugadorBD = new Jugadores { IdJugador = 1, nombreUsuario = "testUser", IdFotoPerfil = 1, IdPuntuacion = 1, IdCuenta = 1 };

        mockSetJugadores.Setup(m => m.Find(1)).Returns(jugadorBD);
        mockContext.Setup(m => m.Jugadores).Returns(mockSetJugadores.Object);

        var jugadorDAO = new JugadorDAO();

        // Act
        var result = jugadorDAO.ObtenerJugadorPorId(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.IdJugador);
        Assert.Equal("testUser", result.NombreUsuario);
    }

    [Fact]
    public void ObtenerJugadorPorId_JugadorNoExiste_RetornaNull()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetJugadores = new Mock<DbSet<Jugadores>>();

        mockSetJugadores.Setup(m => m.Find(1)).Returns((Jugadores)null);
        mockContext.Setup(m => m.Jugadores).Returns(mockSetJugadores.Object);

        var jugadorDAO = new JugadorDAO();

        // Act
        var result = jugadorDAO.ObtenerJugadorPorId(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ModificarJugador_JugadorExiste_ModificaJugador()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetJugadores = new Mock<DbSet<Jugadores>>();
        var jugadorBD = new Jugadores { IdJugador = 1, nombreUsuario = "testUser", IdFotoPerfil = 1 };

        mockSetJugadores.Setup(m => m.Find(1)).Returns(jugadorBD);
        mockContext.Setup(m => m.Jugadores).Returns(mockSetJugadores.Object);

        var jugadorDAO = new JugadorDAO();
        var jugador = new Jugador { IdJugador = 1, NombreUsuario = "newUser", IdFotoPerfil = 2 };

        // Act
        jugadorDAO.ModificarJugador(jugador);

        // Assert
        Assert.Equal("newUser", jugadorBD.nombreUsuario);
        Assert.Equal(2, jugadorBD.IdFotoPerfil);
    }

    [Fact]
    public void NombreUsuarioExiste_NombreUsuarioExiste_RetornaTrue()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetJugadores = new Mock<DbSet<Jugadores>>();
        var data = new List<Jugadores>
        {
            new Jugadores { nombreUsuario = "testUser" }
        }.AsQueryable();

        mockSetJugadores.As<IQueryable<Jugadores>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSetJugadores.As<IQueryable<Jugadores>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSetJugadores.As<IQueryable<Jugadores>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSetJugadores.As<IQueryable<Jugadores>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        mockContext.Setup(m => m.Jugadores).Returns(mockSetJugadores.Object);

        var jugadorDAO = new JugadorDAO();

        // Act
        var result = jugadorDAO.NombreUsuarioExiste("testUser");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NombreUsuarioExiste_NombreUsuarioNoExiste_RetornaFalse()
    {
        // Arrange
        var mockContext = new Mock<LaOcaBDEntities>();
        var mockSetJugadores = new Mock<DbSet<Jugadores>>();
        var data = new List<Jugadores>().AsQueryable();

        mockSetJugadores.As<IQueryable<Jugadores>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSetJugadores.As<IQueryable<Jugadores>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSetJugadores.As<IQueryable<Jugadores>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSetJugadores.As<IQueryable<Jugadores>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        mockContext.Setup(m => m.Jugadores).Returns(mockSetJugadores.Object);

        var jugadorDAO = new JugadorDAO();

        // Act
        var result = jugadorDAO.NombreUsuarioExiste("testUser");

        // Assert
        Assert.False(result);
    }
}
