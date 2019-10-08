using System.Net.Sockets;
using SocketTest.Shared;

namespace SocketTest.Server
{
    public class Client : BaseClient
    {
        public Client(Socket clientSocket) : base(clientSocket)
        {
        }

        protected override void ReceiveCompleted(object data)
        {
            
        }

        protected override void ConnectionFailed()
        {
            
        }
    }
}
