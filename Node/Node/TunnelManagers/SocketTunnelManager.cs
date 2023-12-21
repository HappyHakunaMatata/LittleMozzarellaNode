using Node.TunnelExecutors.Models;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using Node.TunnelExecutors;

namespace Node.TunnelManagers
{
    public class SocketTunnelManager : TunnelManager
    {
        protected readonly ILogger _logger;

        public SocketTunnelManager(X509Certificate2? certificate = null, string? ip = null) : base(ip:ip, certificate:certificate)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });
            _logger = loggerFactory.CreateLogger<TunnelManager>();
            cancellationTokenSource = new CancellationTokenSource();
        }

        private CancellationTokenSource cancellationTokenSource;
        internal override void StartAcceptSocket()
        {
            Task task = Task.Run(async () =>
            {
                await OnAcceptSocketAsync();
            },
                cancellationTokenSource.Token);
            task.Wait();
        }

        private async Task OnAcceptSocketAsync()
        {
            if (ListeningSocket == null)
            {
                cancellationTokenSource.Cancel();
                return;
            }
            while (true)
            {
                TunnelStructure tunnel = new TunnelStructure();
                try
                {
                    var accept = await ListeningSocket.AcceptAsync();
                    tunnel.client = accept;
                    await ConnectAsync(tunnel);
                    //ScanForExpiredTunneling();
                }
                catch (Exception e)
                {
                    _logger.LogError($"Request error: {e}");
                }
            }
        }

        private async Task ConnectAsync(TunnelStructure tunnel)
        {
            byte[] buffer = new byte[Settings.BufferSize];
            SocketTunnelExecutor executor = new(_logger);
            int response = await executor.ReceiveAsync(tunnel.client, buffer);
            if (response == 0)
            {
                await executor.CloseSocketAsync(tunnel.client);
                return;
            }
            var result = await TryCreateTunnel(buffer, tunnelStruct: tunnel, executor);
            if (result == false)
            {
                string ForbiddenRequest = "HTTP/1.1 403 Forbidden\r\n\r\n";
                buffer = Encoding.UTF8.GetBytes(ForbiddenRequest);
                await executor.SendAsync(tunnel.client, buffer, buffer.Length);
                await executor.CloseSocketAsync(tunnel.client);
            }
        }

        private async Task<bool> TryCreateTunnel(byte[] buffer, TunnelStructure tunnelStruct, SocketTunnelExecutor executor)
        {
            string EstablishedRequest = "HTTP/1.1 200 Connection Established\r\n\r\n";
            RequestStruct? request;
            var isHttp = TryCheckHttpMethod(buffer, out request);
            if (!isHttp || request == null)
            {
                return false;
            }
            tunnelStruct.remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await executor.ConnectAsync(tunnelStruct.remote, request.Value.requestLine.endPoint);
                buffer = Encoding.UTF8.GetBytes(EstablishedRequest);
                var sended = await executor.SendAsync(tunnelStruct.client, buffer, buffer.Length);
                if (sended != EstablishedRequest.Length)
                {
                    throw new ArgumentException("The length of the received data doesn't match the length of the sent data.");
                }
            }
            catch (Exception e)
            {
                await executor.CloseSocketAsync(tunnelStruct.remote);
                await executor.CloseSocketAsync(tunnelStruct.client);
                _logger.LogError($"TryCreateTunnel has created the exception: {e.Message}");
            }
            executor.SetSockets(tunnelStruct);
            //tunnelings.Add(tunnelStruct.key, tunnel);
            try
            {
                executor.StartTunneling();
            }
            catch (Exception e)
            {
                _logger.LogError($"Tunneling exception: {e.Message}");
            }
            return true;
        }
    }
}

