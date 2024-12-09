using log4net;
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

        private static readonly ILog _loggerJugabilidad = LogManager.GetLogger(typeof(IServicioJugabilidad));

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

        public void Mover(Ficha ficha, int pasos, List<Casilla> tablero, bool movimientoEspecial = false)
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
                        if (!movimientoEspecial)
                        {
                            ficha.PosicionActual = _utilidades.ObtenerSiguienteOca(ficha.PosicionActual);
                        }
                        break;

                    case Utilidades.PUENTE:
                        if (!movimientoEspecial)
                        {
                            ficha.PosicionActual = ficha.PosicionActual == 6 ? 12 : 6;
                            return;
                        }
                        break;

                    case Utilidades.POSADA:
                        // No sa maneja nada en el servicio para esta casilla especial.
                        break;

                    case Utilidades.DADOS:
                        break;

                    case Utilidades.POZO:
                        // Falta la implementación para el rescate de jugadores en el pozo.
                        break;

                    case Utilidades.LABERINTO:
                        ficha.PosicionActual = 30;
                        break;

                    case Utilidades.CARCEL:
                        // No sa maneja nada en el servicio para esta casilla especial.
                        break;

                    case Utilidades.CALAVERA:
                        ficha.PosicionActual = 1;
                        break;

                    case Utilidades.META:
                        // Falta la implementación para llegar a la casilla final.
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
                        catch (CommunicationException ex)
                        {
                            _loggerJugabilidad.Error("Error al comunicarse con un cliente. El cliente se desconectó de forma inesperada.", ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _loggerJugabilidad.Error("Error al comunicarse con un cliente. La conexión tardó demasiado.", ex);
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