using System;
using SocketTest.Shared;

namespace SocketTest.Client
{
    public class MonitoringClient : BaseClient
    {
        public void Connect()
        {
            Connect("PO160002638", 11000);
            Receive();
        }

        protected override void ReceiveCompleted(object data)
        {
            Console.WriteLine(data);
        }

        protected override void ConnectionFailed()
        {
            
        }
    }
}
