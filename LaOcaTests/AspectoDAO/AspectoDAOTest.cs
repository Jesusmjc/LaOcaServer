using System;
using System.Linq;
using Xunit;
using LaOcaService.DAOs.AspectoFolder;
using LaOcaDataAccess;
using LaOcaService;

public class AspectoDAOTest : IDisposable
{
    private readonly LaOcaBDEntities contexto;
    private readonly AspectoDAO aspectoDAO;
    private int idAspectoPrueba;

    public AspectoDAOTest()
    {
        contexto = new LaOcaBDEntities();
        aspectoDAO = new AspectoDAO();
        PrepararBaseDeDatos();
    }

    private void PrepararBaseDeDatos()
    {
        var aspecto = new Aspectos
        {
            tipo = "FotoPerfil",
            referencia = "referenciaPrueba.jpg"
        };
        contexto.Aspectos.Add(aspecto);
        contexto.SaveChanges();
        idAspectoPrueba = aspecto.IdAspecto;
    }

    [Fact]
    public void PruebaCrearAspecto()
    {
        var nuevoAspecto = new Aspecto
        {
            Tipo = "Icono",
            Referencia = "iconoPrueba.png"
        };

        aspectoDAO.CrearAspecto(nuevoAspecto);

        var aspectoBD = contexto.Aspectos.FirstOrDefault(a => a.referencia == "iconoPrueba.png");
        Assert.NotNull(aspectoBD);
        Assert.Equal("Icono", aspectoBD.tipo);
    }

    [Fact]
    public void PruebaObtenerAspectoPorId()
    {
        var aspecto = aspectoDAO.ObtenerAspectoPorId(idAspectoPrueba);
        Assert.NotNull(aspecto);
        Assert.Equal("referenciaPrueba.jpg", aspecto.Referencia);
    }

    public void Dispose()
    {
        var aspecto = contexto.Aspectos.Find(idAspectoPrueba);
        if (aspecto != null)
        {
            contexto.Aspectos.Remove(aspecto);
            contexto.SaveChanges();
        }
        contexto.Dispose();
    }
}
