using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LaOcaService
{
    [ServiceContract]
    public interface IServicioJugabilidad
    {
        /// <summary>
        /// Actualiza la posición de la ficha del jugador correspondiente,
        /// y notifica al resto de jugadores en la partida del cambio
        /// </summary>
        /// <param name="pasos"> El número de casillas avanzadas por la ficha </param>
        /// <param name="codigoSala"> El código de la sala en cuestión </param>
        /// <param name="nombreJugador"> El nombre del jugador que acaba de jugar su turno </param>
        [OperationContract]
        void JugarTurno(int pasos, string codigoSala, string nombreJugador);

        /// <summary>
        /// Recupera la posición actual de la ficha
        /// </summary>
        /// <returns>  La posición actual de la ficha </returns>
        [OperationContract]
        int ObtenerPosicionFicha();
    }

    [DataContract]
    public class Casilla
    {
        [DataMember]
        public int Numero { get; set; }

        [DataMember]
        public string Tipo { get; set; }

        [DataMember]
        public int Destino { get; set; }

        public Casilla(int numero, string tipo, int destino = -1)
        {
            Numero = numero;
            Tipo = tipo;
            Destino = destino;
        }
    }

    [DataContract]
    public class Ficha
    {
        [DataMember]
        public int PosicionActual { get; set; }

        [DataMember]
        public int PosicionAnterior { get; set; }

        public Ficha()
        {
            PosicionActual = 0;
            PosicionAnterior = 0;
        }
    }

    [DataContract]
    public class Juego
    {
        [DataMember]
        public Tablero Tablero { get; set; }

        [DataMember]
        public Ficha Ficha { get; set; }

        public Juego()
        {
            Tablero = new Tablero(new List<Casilla>());
            Ficha = new Ficha();
        }
    }

    [DataContract]
    public class Tablero
    {
        [DataMember]
        public List<Casilla> Casillas { get; set; }

        public Tablero(List<Casilla> casillas)
        {
            Casillas = casillas ?? new List<Casilla>();
        }
    }
}