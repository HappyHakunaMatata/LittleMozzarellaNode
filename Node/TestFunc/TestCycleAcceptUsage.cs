using System;
using System.Net.Sockets;

namespace TestFunc
{
	public class TestCycleAcceptUsage: TestAcceptUsage
    {
        public override void Run(Socket ListeningSocket)
        {
            while (ListeningSocket != null)
            {
                try
                {
                    Socket listener = ListeningSocket.Accept();
                    sockets.Add(listener);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}

