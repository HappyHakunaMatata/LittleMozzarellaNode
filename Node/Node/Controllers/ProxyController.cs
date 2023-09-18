using System;
using Node.TunnelManagers;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace Node.Controllers
{
	public class ProxyController
	{
        private TunnelManager manager;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken CancellationToken => cancellationTokenSource.Token;

        private Thread consoleThread;
        private bool isConsoleRunning;

        public ProxyController()
		{
            isConsoleRunning = false;
        }

        public void Dispose()
        {

        }

        public void StartProxy()
        {
            if (manager == null)
            {
                Console.WriteLine("Please specify your server type with -t");
            }
            else
            {
                Console.WriteLine("Proxy is starting");
                Task.Run(() => manager.OpenSocketAsync(), CancellationToken);
            }
        }

        public void AddEndPoint()
        {

        }

        public void Stop()
        {

        }

        public void StartConsole()
        {
            if (!isConsoleRunning)
            {
                isConsoleRunning = true;
                consoleThread = new Thread(ReadCommands);
                consoleThread.Start();
            }
        }

        public void StopConsole()
        {
            isConsoleRunning = false;
        }

        private void ReadCommands()
        {
            while (isConsoleRunning)
            {
                string input = Console.ReadLine();
                switch (input.ToLower())
                {
                    case ("exit"):
                        StopConsole();
                        break;
                    case ("-t"):
                        this.manager = new SocketTunnelManager();
                        break;
                    case ("start"):
                        StartProxy();
                        break;
                }
            }
        }
    }
}

