using System.Collections.Generic;
using Xunit;

namespace LaOcaService.Tests
{
    public class LaOcaServiceTests
    {
        [Fact]
        public void TestMovimientoEspecialOca()
        {
            // Arrange
            LaOcaService servicio = new LaOcaService();
            servicio.InicializarJuego();
            Ficha ficha = new Ficha();
            List<Casilla> tablero = servicio._juego.Tablero.Casillas;

            // Lista de posiciones de las casillas de OCA
            int[] posicionesOca = { 1, 5, 9, 14, 18, 23, 27, 32, 36, 41, 45, 50, 54, 59, 63 };

            for (int i = 0; i < posicionesOca.Length - 1; i++)
            {
                // Act
                ficha.PosicionActual = posicionesOca[i];
                servicio.Mover(ficha, 0, tablero);

                // Assert
                Assert.Equal(posicionesOca[i + 1], ficha.PosicionActual);
            }
        }
    }
}
