using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTest.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            MonitoringServer server = new MonitoringServer();
            server.SetupPermissions();
            server.Start();

            Console.WriteLine("Press any key to stop...");
            Console.ReadLine();
        }
    }
}
