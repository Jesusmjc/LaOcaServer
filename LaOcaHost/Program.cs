﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaHost
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(LaOcaService.LaOcaService)))
            {
                host.Open();
                Console.WriteLine("Server is running. Press Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
