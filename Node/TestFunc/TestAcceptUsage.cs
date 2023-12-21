using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace TestFunc
{
    public abstract class TestAcceptUsage
    {
        public List<Socket> sockets = new List<Socket>();

        public Socket Start(int port)
        {
            Socket ListeningSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            ListeningSocket.Bind(new IPEndPoint(ipAddress, port));
            ListeningSocket.Listen(10);
            return ListeningSocket;
        }


        public long EstimateFunctionMemoryUsage(Socket ListeningSocket)
        {
            Task.Run(() => Run(ListeningSocket));
            CreateClients();
            return GC.GetTotalMemory(true);
        }

        public int count = 10;
        public void CreateClients()
        {
            for (var i = 0; i < count; i++)
            {
                Socket socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
            }
        }

        public bool OK()
        {
            if (sockets.Count == count)
            {
                return true;
            }
            return false; 
        }

        public abstract void Run(Socket socket);
    }
}

