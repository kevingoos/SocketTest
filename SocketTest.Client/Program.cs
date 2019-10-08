using System;
using System.Threading;

namespace SocketTest.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(500);
            MonitoringClient client = new MonitoringClient();
            client.Connect();

            Console.WriteLine("Press any key to stop...");
            Console.ReadLine();
        }
    }
}
