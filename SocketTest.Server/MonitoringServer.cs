using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketTest.Shared;

namespace SocketTest.Server
{
    public class MonitoringServer
    {
        private readonly ManualResetEvent _allDone = new ManualResetEvent(false);
        private readonly IList<Client> _clients = new List<Client>();

        #region Client setup

        public void SetupPermissions()
        {
            var permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, Dns.GetHostName(), 8080);
            permission.Demand();
        }

        public void Start()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, 11000);

            var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    _allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(AcceptCallback, listener);

                    // Wait until a connection is made before continuing.  
                    _allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Stop()
        {
            Parallel.ForEach(_clients, client =>
            {
                client.Disconnect();
            });
        }

        #endregion

        #region Client callbacks

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            _allDone.Set();

            // Get the socket that handles the client request.  
            var listener = (Socket)ar.AsyncState;
            var clientSocket = listener.EndAccept(ar);

            // Create the state object.  
            var client = new Client(clientSocket);
            Console.WriteLine("New client...");

            _clients.Add(client);
            //Start listening...
            client.Receive();
            //Send announce
            var carObject = new Car()
            {
                Horn = "Toet toet",
                Company = "Audi",
                Type = "Q5"
            };
            var bytes = Serialize(carObject);

            while (true)
            {
                Console.WriteLine($"Sending {bytes.Length} bytes...");
                Thread.Sleep(5000);
                client.Send(bytes);
            }
            
        }

        private byte[] Serialize(object item)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, item);
            return stream.ToArray();
        }

        #endregion
    }
}
