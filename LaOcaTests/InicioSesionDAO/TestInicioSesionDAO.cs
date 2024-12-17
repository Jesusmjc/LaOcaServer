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
        private readonly LaOcaBDEntities _contexto;
        private DbContextTransaction _transaccion;

        private LaOcaService.DAOs.InicioSesionDAO _inicioSesionDAO;
        private int _idCuentaPrueba;
        private int _idFotoPerfilPrueba;
        private int _idPuntuacionPrueba;
        private int _idJugadorPrueba;

        public TestInicioSesionDAO()
        {
            _contexto = new LaOcaBDEntities();
            _transaccion = _contexto.Database.BeginTransaction();
            _inicioSesionDAO = new LaOcaService.DAOs.InicioSesionDAO(_contexto);

            PrepararBaseDeDatos();
        }

        private void PrepararBaseDeDatos()
        {
            Aspectos fotoPerfil = new Aspectos();
            _contexto.Aspectos.Add(fotoPerfil);
            _contexto.SaveChanges();

            Puntuaciones puntuaciones = new Puntuaciones();
            _contexto.Puntuaciones.Add(puntuaciones);
            _contexto.SaveChanges();

            Cuentas cuentaDePrueba = new Cuentas()
            {
                correoElectronico = "correoejemplo@gmail.com",
                contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324", // Ej3mpl0_Contra53ñ4
            };
            _contexto.Cuentas.Add(cuentaDePrueba);
            _contexto.SaveChanges();

            _idFotoPerfilPrueba = fotoPerfil.IdAspecto;
            _idPuntuacionPrueba = puntuaciones.IdPuntuacion;
            _idCuentaPrueba = cuentaDePrueba.IdCuenta;

            Jugadores jugadorPrueba = new Jugadores()
            {
                IdCuenta = _idCuentaPrueba,
                nombreUsuario = "jugadorDePrueba3928",
                IdFotoPerfil = _idFotoPerfilPrueba,
                IdPuntuacion = _idPuntuacionPrueba
            };
            _contexto.Jugadores.Add(jugadorPrueba);
            _contexto.SaveChanges();

            _idJugadorPrueba = jugadorPrueba.IdJugador;
        }

        [Fact]
        public void PruebaIniciarSesionExitoso()
        {
            var jugadorEsperado = new Jugador
            {
                IdJugador = _idJugadorPrueba,
                IdCuenta = _idCuentaPrueba,
                NombreUsuario = "jugadorDePrueba3928",
                IdFotoPerfil = _idFotoPerfilPrueba,
                IdPuntuacion = _idPuntuacionPrueba
            };

            var cuentaQueSiExiste = new Cuenta
            {
                CorreoElectronico = "correoejemplo@gmail.com",
                Contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324" // Ej3mpl0_Contra53ñ4
            };

            Jugador jugador = _inicioSesionDAO.IniciarSesion(cuentaQueSiExiste);

            Assert.True(jugadorEsperado.Equals(jugador));
        }

        [Fact]
        public void PruebaIniciarSesionFallido()
        {
            var jugadorEsperado = new Jugador();

            var cuentaQueNoExiste = new Cuenta()
            {
                CorreoElectronico = "correoejemploinexistente@gmail.com",
                Contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324" // Ej3mpl0_Contra53ñ4
            };

            Jugador jugador = _inicioSesionDAO.IniciarSesion(cuentaQueNoExiste);

            Assert.True(jugadorEsperado.Equals(jugador));
        }

        public void Dispose()
        {
            _transaccion.Rollback();
            _transaccion.Dispose();
            _contexto.Dispose();
        }
    }
}
