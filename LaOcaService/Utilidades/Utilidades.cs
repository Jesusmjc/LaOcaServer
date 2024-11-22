using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    public class Utilidades
    {
        public const string OCA = "oca";
        public const string PUENTE = "puente";
        public const string POSADA = "posada";
        public const string DADOS = "dados";
        public const string POZO = "pozo";
        public const string LABERINTO = "laberinto";
        public const string CARCEL = "cárcel";
        public const string CALAVERA = "calavera";
        public const string META = "meta";
        public const string NORMAL = "normal";

        private Dictionary<int, string> casillasEspeciales = new Dictionary<int, string>
        {
            {1, OCA},
            {5, OCA},
            {9, OCA},
            {14, OCA},
            {18, OCA},
            {23, OCA},
            {27, OCA},
            {32, OCA},
            {36, OCA},
            {41, OCA},
            {45, OCA},
            {50, OCA},
            {54, OCA},
            {59, OCA},
            {6, PUENTE},
            {12, PUENTE},
            {19, POSADA},
            {26, DADOS},
            {31, POZO},
            {42, LABERINTO},
            {52, CARCEL},
            {58, CALAVERA},
            {63, META}
        };

        public string VerificarCasillaEspecial(int posicion)
        {
            return casillasEspeciales.ContainsKey(posicion) ? casillasEspeciales[posicion] : NORMAL;
        }

        public int ObtenerSiguienteOca(int posicionActual)
        {
            int[] casillasOca = { 1, 5, 9, 14, 18, 23, 27, 32, 36, 41, 45, 50, 54, 59};
            foreach (int casilla in casillasOca)
            {
                if (casilla > posicionActual)
                {
                    return casilla;
                }
            }
            return posicionActual;
        }
    }
}
