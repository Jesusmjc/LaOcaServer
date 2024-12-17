using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    public partial class LaOcaService : IServicioJugabilidad
    {
        public Juego juego { get; set; }
        private Jugador _jugador;

        private static readonly ILog _LoggerJugabilidad = LogManager.GetLogger(typeof(IServicioJugabilidad));

        public void InicializarJuego()
        {
            var casillas = InicializarCasillas();
            var tablero = new Tablero(casillas);
            juego = new Juego { Tablero = tablero, Ficha = new Ficha() };
            _jugador = new Jugador();
        }

        private static List<Casilla> InicializarCasillas()
        {
            var casillas = new List<Casilla>();
            for (int i = 0; i < 63; i++)
            {
                string tipo = Utilidades.VerificarCasillaEspecial(i);
                casillas.Add(new Casilla(i, tipo));
            }
            return casillas;
        }

        public void Mover(Ficha ficha, int pasos, List<Casilla> tablero)
        {
            try
            {
                if (!ValidarPrecondiciones(ficha, tablero))
                {
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

                    MoverFichaSegunCasilla(tipoCasilla, ficha);
                }
                else
                {
                    _LoggerJugabilidad.Error($"Error: Posición inválida ({ficha.PosicionActual}) después de mover. Verifique la lógica de movimiento.");
                    Console.WriteLine($"Error: Posición inválida ({ficha.PosicionActual}) después de mover. Verifique la lógica de movimiento.");
                }
            }
            catch (SqlException)
            {
                Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
            }
            catch (EntityException)
            {
                Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
            }
        }

        private static bool ValidarPrecondiciones(Ficha ficha, List<Casilla> tablero)
        {
            bool resultado = true;

            if (ficha == null)
            {
                Console.WriteLine("Error: La ficha es null. Asegúrate de que cada jugador tenga una ficha asignada antes de mover.");
                resultado = false;
            }

            if (tablero == null || tablero.Count == 0)
            {
                Console.WriteLine("Error: El tablero es null o está vacío.");
                resultado = false;
            }

            return resultado;
        }

        private static void MoverFichaSegunCasilla(string tipoCasilla, Ficha ficha)
        {
            try
            {
                switch (tipoCasilla)
                {
                    case Utilidades.OCA:
                        ficha.PosicionActual = Utilidades.ObtenerSiguienteOca(ficha.PosicionActual);
                        break;

                    case Utilidades.PUENTE:
                        ficha.PosicionActual = ficha.PosicionActual == 6 ? 12 : 6;
                        break;

                    case Utilidades.LABERINTO:
                        ficha.PosicionActual = 30;
                        break;

                    case Utilidades.CALAVERA:
                        ficha.PosicionActual = 1;
                        break;

                    default:
                        break;
                }
            }
            catch (SqlException)
            {
                Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
            }
            catch (EntityException)
            {
                Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
            }
        }

        public void JugarTurno(int pasos, string codigoSala, string nombreJugador)
        {
            if (_ListaSalasActivas.TryGetValue(codigoSala, out Sala sala) && sala.Jugadores.TryGetValue(nombreJugador, out Jugador jugador))
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
                        catch (SqlException)
                        {
                            Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
                        }
                        catch (EntityException)
                        {
                            Console.WriteLine("No fue posible conectarse a la base de datos, por favor intente más tarde.");
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