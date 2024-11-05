using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    public partial class LaOcaService : IServicioJugabilidad
    {
        private Juego _juego;

        private void InicializarJuego()
        {
            var casillas = InicializarCasillas();
            var tablero = new Tablero(casillas);
            _juego = new Juego { Tablero = tablero, Ficha = new Ficha() };
        }

        private List<Casilla> InicializarCasillas()
        {
            var casillas = new List<Casilla>();
            for (int i = 0; i < 63; i++)
            {
                if (i == 5 || i == 9 || i == 14)
                {
                    casillas.Add(new Casilla(i, "Oca", i + 4));
                }
                else
                {
                    casillas.Add(new Casilla(i, "Normal"));
                }
            }
            return casillas;
        }

        public void Mover(Ficha ficha, int pasos, List<Casilla> tablero)
        {
            int nuevaPosicion = ficha.PosicionActual + pasos;

            if (nuevaPosicion >= tablero.Count)
            {
                nuevaPosicion = tablero.Count - 1;
            }

            ficha.PosicionActual = nuevaPosicion;

            Casilla casillaActual = tablero[ficha.PosicionActual];
            if (casillaActual.Tipo != "Normal" && casillaActual.Destino != -1)
            {
                ficha.PosicionActual = casillaActual.Destino;
            }
        }

        public void JugarTurno(int pasos)
        {
            Mover(_juego.Ficha, pasos, _juego.Tablero.Casillas);
        }

        public int ObtenerPosicionFicha()
        {
            return _juego.Ficha.PosicionActual;
        }
    }
}
