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

        public TestInicioSesionDAO()
        {
            contexto = new LaOcaBDEntities();
            transaccion = contexto.Database.BeginTransaction();
            inicioSesionDAO = new LaOcaService.DAOs.InicioSesionDAO(contexto);

            PrepararBaseDeDatos();
        }

        private void PrepararBaseDeDatos()
        {
            Cuentas cuentaDePrueba = new Cuentas()
            {
                correoElectronico = "correoejemplo@gmail.com",
                contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324" // Ej3mpl0_Contra53ñ4
            };
            contexto.Cuentas.Add(cuentaDePrueba);
            contexto.SaveChanges();

            idCuentaPrueba = cuentaDePrueba.IdCuenta;

            contexto.Jugadores.Add(new Jugadores()
            {
                IdCuenta = idCuentaPrueba,
                nombreUsuario = "jugadorDePrueba3928"
            });
            contexto.SaveChanges();
        }

        [Fact]
        public void PruebaIniciarSesionExitoso()
        {
            var jugadorEsperado = new Jugadores
            {
                IdCuenta = idCuentaPrueba,
                nombreUsuario = "jugadorDePrueba3928"
            };

            var cuentaQueSiExiste = new Cuenta()
            {
                CorreoElectronico = "correoejemplo@gmail.com",
                Contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324" // Ej3mpl0_Contra53ñ4
            };

            Jugador jugador = inicioSesionDAO.IniciarSesion(cuentaQueSiExiste);

            Assert.NotNull(jugador);
            Assert.Equal(jugadorEsperado.IdCuenta, jugador.IdCuenta);
            Assert.Equal(jugadorEsperado.nombreUsuario, jugador.NombreUsuario);
        }

        [Fact]
        public void PruebaIniciarSesionFallido() //No existe la cuenta
        {
            var cuentaQueNoExiste = new Cuenta() //Esta cuenta no existe en la bd
            {
                CorreoElectronico = "correoejemploinexistente@gmail.com",
                Contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324" // Ej3mpl0_Contra53ñ4
            };

            Jugador jugador = inicioSesionDAO.IniciarSesion(cuentaQueNoExiste);

            Assert.NotNull(jugador);
            Assert.Equal(0, jugador.IdCuenta);
            Assert.Null(jugador.NombreUsuario);
        }

        public void Dispose()
        {
            transaccion.Rollback();
            transaccion.Dispose();
            contexto.Dispose();
        }
    }
}
