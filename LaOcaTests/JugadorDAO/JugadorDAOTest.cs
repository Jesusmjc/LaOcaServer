using System;
using Xunit;
using LaOcaDataAccess;
using LaOcaService.DAOs.JugadorFolder;
using LaOcaService;
using System.Linq;

public class JugadorDAOTest : IDisposable
{
    private readonly LaOcaBDEntities contexto;
    private readonly JugadorDAO jugadorDAO;
    private int idJugadorPrueba;

    public JugadorDAOTest()
    {
        contexto = new LaOcaBDEntities();
        jugadorDAO = new JugadorDAO();
        PrepararBaseDeDatos();
    }

    private void PrepararBaseDeDatos()
    {
        var puntuacion = new Puntuaciones
        {
            casillasRecorridasGlobal = 0,
            monedasObtenidasGlobal = 0,
            partidasGanadasGlobal = 0,
            monedasActuales = 0
        };
        contexto.Puntuaciones.Add(puntuacion);
        contexto.SaveChanges();

        var cuenta = new Cuentas
        {
            correoElectronico = "prueba@correo.com",
            contrasena = "contrasenaPrueba"
        };
        contexto.Cuentas.Add(cuenta);
        contexto.SaveChanges();

        var jugador = new Jugadores
        {
            nombreUsuario = "usuarioPrueba",
            IdCuenta = cuenta.IdCuenta,
            IdFotoPerfil = 1,
            IdPuntuacion = puntuacion.IdPuntuacion
        };
        contexto.Jugadores.Add(jugador);
        contexto.SaveChanges();
        idJugadorPrueba = jugador.IdJugador;
    }

    [Fact]
    public void PruebaCrearJugador()
    {
        var nuevoJugador = new Jugador
        {
            NombreUsuario = "nuevoUsuario",
            IdCuenta = contexto.Cuentas.First().IdCuenta,
            IdFotoPerfil = 2,
            IdPuntuacion = 2
        };

        jugadorDAO.CrearJugador(nuevoJugador, "imagen.jpg");

        var jugadorBD = contexto.Jugadores.FirstOrDefault(j => j.nombreUsuario == "nuevoUsuario");
        Assert.NotNull(jugadorBD);
        Assert.Equal("nuevoUsuario", jugadorBD.nombreUsuario);
    }

    [Fact]
    public void PruebaModificarJugador()
    {
        var jugador = new Jugador
        {
            IdJugador = idJugadorPrueba, 
            NombreUsuario = "usuarioModificado",
            IdFotoPerfil = 1 
        };

        jugadorDAO.ModificarJugador(jugador);

        using (var nuevoContexto = new LaOcaBDEntities())
        {
            var jugadorBD = nuevoContexto.Jugadores.Find(idJugadorPrueba);
            Assert.NotNull(jugadorBD);

            Console.WriteLine($"NombreUsuario Actualizado: {jugadorBD.nombreUsuario}");
            Assert.Equal("usuarioModificado", jugadorBD.nombreUsuario);
        }
    }

    [Fact]
    public void PruebaObtenerJugadorPorId()
    {
        var jugador = jugadorDAO.ObtenerJugadorPorId(idJugadorPrueba);
        Assert.NotNull(jugador);
        Assert.Equal("usuarioPrueba", jugador.NombreUsuario);
    }

    [Fact]
    public void PruebaNombreUsuarioExiste()
    {
        var existe = jugadorDAO.NombreUsuarioExiste("usuarioPrueba");
        Assert.True(existe);
    }

    public void Dispose()
    {
        contexto.Dispose();
    }
}
