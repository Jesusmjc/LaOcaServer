using LaOcaDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    public partial class LaOcaService : ILoginService
    {
        public int GetUser(string username)
        {
            int result = 0;

            try
            {
                using (var context = new LaOcaBDEntities())
                {
                    var usuarioBD = context.Jugadores.Where(j => j.nombreUsuario == username).FirstOrDefault();

                    if (usuarioBD != null)
                    {
                        result = 1;
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return result;
        }
    }
}
