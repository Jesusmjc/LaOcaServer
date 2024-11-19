using LaOcaDataAccess;
using LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LaOcaTests.InicioSesionDAO
{
    public class TestInicioSesionDAOExcepcion
    {
        private readonly LaOcaBDEntities _contexto;
        private LaOcaService.DAOs.InicioSesionDAO _inicioSesionDAO;

        public TestInicioSesionDAOExcepcion()
        {
            _contexto = new LaOcaBDEntities();
            _inicioSesionDAO = new LaOcaService.DAOs.InicioSesionDAO(_contexto);
        }

        [Fact]
        public void PruebaIniciarSesionExcepcion() // La cuenta sí existe, se apaga la bd
        {
            var cuentaQueSiExiste = new Cuenta()
            {
                CorreoElectronico = "correoejemplo@gmail.com",
                Contrasena = "96c63e8bf0a1abe4539fd3b6dcd269bfcd27929a4d6a28bcf82195feae5b3324" // Ej3mpl0_Contra53ñ4
            };

            var excepcion = Assert.Throws<FaultException<InicioSesionException>>(
                () => _inicioSesionDAO.IniciarSesion(cuentaQueSiExiste));

            Assert.Equal("Ocurrió un error al conectar con la Base de Datos. ", excepcion.Detail.Mensaje);
        }
    }
}
