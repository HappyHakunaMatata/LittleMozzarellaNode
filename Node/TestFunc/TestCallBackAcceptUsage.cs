using System;
using System.Net.Sockets;

namespace TestFunc
{
	public class TestCallBackAcceptUsage: TestAcceptUsage
    {

        public override void Run(Socket ListeningSocket)
        {
            ListeningSocket.BeginAccept(
                new AsyncCallback(DoAcceptSocketCallback), ListeningSocket);
        }

        private void DoAcceptSocketCallback(IAsyncResult async)
        {
            ArgumentNullException.ThrowIfNull(async.AsyncState);
            Socket listener = (Socket)async.AsyncState;
            try
            {
                Socket clientSocket = listener.EndAccept(async);
                sockets.Add(clientSocket);
                listener.BeginAccept(new AsyncCallback(DoAcceptSocketCallback), listener);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            
        }
    }
}

