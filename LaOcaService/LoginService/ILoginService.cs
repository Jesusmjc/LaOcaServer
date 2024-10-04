using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaService
{
    [ServiceContract]
    internal interface ILoginService
    {
        [OperationContract]
        int GetUser(string username);
    }
}
