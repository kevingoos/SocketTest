using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SocketTest.Shared
{
    public abstract class BaseClient
    {
        protected Socket ClientSocket { get; set; }
        private const int BufferSize = 1024;

        // Receive buffer.  
        private MemoryStream _bufferStream = new MemoryStream();
        private readonly byte[] _buffer = new byte[BufferSize];
        private ushort _messageLength;

        public BaseClient()
        {

        }

        public BaseClient(Socket clientSocket)
        {
            ClientSocket = clientSocket;
        }

        public void Connect(string url, int port)
        {
            var ipHost = Dns.GetHostEntry(url);
            var ipAddress = ipHost.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, port);

            ClientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                ClientSocket.Connect(localEndPoint);
                Console.WriteLine(@"Socket connected to -> {0}", ClientSocket.RemoteEndPoint);
                Receive();

                if (ClientSocket.Connected)
                {
                    Connected();
                }
            }
            catch (SocketException ex)
            {
                ConnectionFailed();
            }
        }

        public void Send(byte[] data)
        {
            if (ClientSocket != null && ClientSocket.Connected)
            {
                var message = AddSize(data);
                ClientSocket.BeginSend(message, 0, message.Length, 0, ResponseSend, this);
            }
            else
            {
                ConnectionFailed();
            }
        }

        public void Receive()
        {
            if (ClientSocket != null && ClientSocket.Connected)
            {
                ClientSocket.BeginReceive(_buffer, 0, BufferSize, SocketFlags.None, ReceiveCommand, this);
            }
            else
            {
                ConnectionFailed();
            }
        }

        private byte[] AddSize(byte[] data)
        {
            var newData = BitConverter.GetBytes(Convert.ToUInt16(data.Length));
            Array.Resize(ref newData, sizeof(ushort) + data.Length);
            Array.Copy(data, 0, newData, sizeof(ushort), data.Length);
            return newData;
        }

        public void Disconnect()
        {
            if (ClientSocket != null && ClientSocket.Connected)
            {
                ClientSocket.Disconnect(false);
            }
        }

        #region Callbacks

        private void ResponseSend(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.  
                var bytesSent = ClientSocket.EndSend(ar);

                SendCompleted();
            }
            catch (SocketException ex)
            {
                ConnectionFailed();
            }
        }

        private void ReceiveCommand(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.  
            var client = (BaseClient)ar.AsyncState;

            try
            {
                var bytesRead = ClientSocket.EndReceive(ar);
                if (bytesRead <= 0)
                {
                    ConnectionFailed();
                    return;
                }

                var messageSizeOffset = 0;
                if (_messageLength == 0)
                {
                    _messageLength = GetMessageLength(client._buffer);
                    messageSizeOffset = sizeof(ushort);
                }

                _bufferStream.Write(client._buffer, messageSizeOffset, bytesRead - messageSizeOffset);

                if (_bufferStream.Length >= _messageLength)
                {
                    var data = Deserialize(_bufferStream);

                    //Cleanup
                    _messageLength = 0;
                    _bufferStream.Dispose();
                    _bufferStream = new MemoryStream();

                    ReceiveCompleted(data);
                }

                Receive();
            }
            catch (SocketException ex)
            {
                ConnectionFailed();
            }
        }

        public static object Deserialize(MemoryStream stream)
        {
            var binaryFormatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            return binaryFormatter.Deserialize(stream);
        }


        private ushort GetMessageLength(byte[] buffer)
        {
            return BitConverter.ToUInt16(buffer, 0);
        }

        protected virtual void Connected()
        {

        }

        protected virtual void SendCompleted()
        {

        }

        protected abstract void ReceiveCompleted(object data);
        protected abstract void ConnectionFailed();

        #endregion
    }
}
