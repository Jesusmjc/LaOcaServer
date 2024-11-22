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
        public Juego juego;
        private Utilidades _utilidades;
        private Jugador _jugador;

        public void InicializarJuego()
        {
            _utilidades = new Utilidades();
            var casillas = InicializarCasillas();
            var tablero = new Tablero(casillas);
            juego = new Juego { Tablero = tablero, Ficha = new Ficha() };
            _jugador = new Jugador();
        }

        private List<Casilla> InicializarCasillas()
        {
            var casillas = new List<Casilla>();
            for (int i = 0; i < 63; i++)
            {
                string tipo = _utilidades.VerificarCasillaEspecial(i);
                casillas.Add(new Casilla(i, tipo));
            }
            return casillas;
        }

        public void Mover(Ficha ficha, int pasos, List<Casilla> tablero)
        {
            if (ficha == null)
            {
                Console.WriteLine("Error: La ficha es null. Asegúrate de que cada jugador tenga una ficha asignada antes de mover.");
                return;
            }

            if (tablero == null || tablero.Count == 0)
            {
                Console.WriteLine("Error: El tablero es null o está vacío.");
                return;
            }

            int nuevaPosicion = ficha.PosicionActual + pasos;

            if (nuevaPosicion < 0)
            {
                nuevaPosicion = 1;
            }
            else if (nuevaPosicion >= tablero.Count)
            {
                nuevaPosicion = tablero.Count - 1;
            }

            ficha.PosicionActual = nuevaPosicion;

            if (ficha.PosicionActual >= 0 && ficha.PosicionActual < tablero.Count)
            {
                Casilla casillaActual = tablero[ficha.PosicionActual];
                string tipoCasilla = casillaActual.Tipo;

                switch (tipoCasilla)
                {
                    case Utilidades.OCA:
                        if (ficha.PosicionActual == 59)
                        {
                            ficha.PosicionActual = 63;
                        }
                        else
                        {
                            ficha.PosicionActual = _utilidades.ObtenerSiguienteOca(ficha.PosicionActual);
                            return;
                        }
                        break;

                    case Utilidades.PUENTE:
                        ficha.PosicionActual = ficha.PosicionActual == 6 ? 12 : 6;
                        break;

                    case Utilidades.POSADA:
                        break;

                    case Utilidades.DADOS:
                        break;

                    case Utilidades.POZO:
                        break;

                    case Utilidades.LABERINTO:
                        ficha.PosicionActual = 30;
                        break;

                    case Utilidades.CARCEL:
                        break;

                    case Utilidades.CALAVERA:
                        ficha.PosicionActual = 1;
                        break;

                    case Utilidades.META:
                        break;

                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine($"Error: Posición inválida ({ficha.PosicionActual}) después de mover. Verifique la lógica de movimiento.");
            }
        }

        public void JugarTurno(int pasos, string codigoSala, string nombreJugador)
        {
            if (listaSalasActivas.TryGetValue(codigoSala, out Sala sala) && sala.Jugadores.TryGetValue(nombreJugador, out Jugador jugador))
            {
                if (jugador.Ficha == null)
                {
                    jugador.Ficha = new Ficha();
                }

                if (jugador.TurnosPerdidos > 0)
                {
                    jugador.TurnosPerdidos--;
                }
                else
                {
                    Mover(jugador.Ficha, pasos, juego.Tablero.Casillas);
                }

                foreach (var j in sala.Jugadores.Values)
                {
                    if (j.CanalCallbackPartida != null && jugador.Ficha != null)
                    {
                        try
                        {
                            j.CanalCallbackPartida.ActualizarPosicionFicha(jugador.Ficha.PosicionActual, nombreJugador);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al actualizar la posición de ficha para {j.NombreUsuario}: {ex.Message}");
                        }
                    }
                }
            }
        }

        public int ObtenerPosicionFicha()
        {
            return juego.Ficha.PosicionActual;
        }
    }
}