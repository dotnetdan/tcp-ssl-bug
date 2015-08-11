using System;
using System.ServiceModel;

namespace ReproService
{
    class ReproService
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting service...");

            using (var host = new ServiceHost(typeof(NetTcpService)))
            {
                host.Open();
                Console.WriteLine("Endpoint opened at {0}", host.Description.Endpoints[0].Address);
                Console.WriteLine("Press Enter to stop.");
                Console.ReadLine();
            }
        }
    }
}
