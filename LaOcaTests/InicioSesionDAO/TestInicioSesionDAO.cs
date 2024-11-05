using Xunit;
using System;
using LaOcaService;
using LaOcaDataAccess;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;

namespace LaOcaTests.InicioSesionDAO
{
    public class TestInicioSesionDAO : IDisposable
    {
        private readonly LaOcaBDEntities contexto;
        private DbContextTransaction transaccion;

        private LaOcaService.DAOs.InicioSesionDAO inicioSesionDAO;
        private int idCuentaPrueba;
        private int idFotoPerfilPrueba;
        private int idPuntuacionPrueba;
        private int idJugadorPrueba;

        public TestInicioSesionDAO()
        {
            contexto = new LaOcaBDEntities();
            transaccion = contexto.Database.BeginTransaction();
            inicioSesionDAO = new LaOcaService.DAOs.InicioSesionDAO(contexto);

            PrepararBaseDeDatos();
        }

        private void PrepararBaseDeDatos()
        {
            Aspectos fotoPerfil = new Aspectos();
            contexto.Aspectos.Add(fotoPerfil);
            contexto.SaveChanges();

            Puntuaciones puntuaciones = new Puntuaciones();
            contexto.Puntuaciones.Add(puntuaciones);
            contexto.SaveChanges();

            Cuentas cuentaDePrueba = new Cuentas()
            {
                correoElectronico = "correoejemplo@gmail.com",
                contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324", // Ej3mpl0_Contra53ñ4
            };
            contexto.Cuentas.Add(cuentaDePrueba);
            contexto.SaveChanges();

            idFotoPerfilPrueba = fotoPerfil.IdAspecto;
            idPuntuacionPrueba = puntuaciones.IdPuntuacion;
            idCuentaPrueba = cuentaDePrueba.IdCuenta;

            Jugadores jugadorPrueba = new Jugadores()
            {
                IdCuenta = idCuentaPrueba,
                nombreUsuario = "jugadorDePrueba3928",
                IdFotoPerfil = idFotoPerfilPrueba,
                IdPuntuacion = idPuntuacionPrueba
            };
            contexto.Jugadores.Add(jugadorPrueba);
            contexto.SaveChanges();

            idJugadorPrueba = jugadorPrueba.IdJugador;
        }

        [Fact]
        public void PruebaIniciarSesionExitoso()
        {
            var jugadorEsperado = new Jugador
            {
                IdJugador = idJugadorPrueba,
                IdCuenta = idCuentaPrueba,
                NombreUsuario = "jugadorDePrueba3928",
                IdFotoPerfil = idFotoPerfilPrueba,
                IdPuntuacion = idPuntuacionPrueba
            };

            var cuentaQueSiExiste = new Cuenta
            {
                CorreoElectronico = "correoejemplo@gmail.com",
                Contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324" // Ej3mpl0_Contra53ñ4
            };

            Jugador jugador = inicioSesionDAO.IniciarSesion(cuentaQueSiExiste);

            Assert.True(jugadorEsperado.Equals(jugador));
        }

        [Fact]
        public void PruebaIniciarSesionFallido() //No existe la cuenta
        {
            var jugadorEsperado = new Jugador();

            var cuentaQueNoExiste = new Cuenta() //Esta cuenta no existe en la bd
            {
                CorreoElectronico = "correoejemploinexistente@gmail.com",
                Contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324" // Ej3mpl0_Contra53ñ4
            };

            Jugador jugador = inicioSesionDAO.IniciarSesion(cuentaQueNoExiste);

            Assert.True(jugadorEsperado.Equals(jugador));
        }

        public void Dispose()
        {
            transaccion.Rollback();
            transaccion.Dispose();
            contexto.Dispose();
        }
    }
}
