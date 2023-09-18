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
        public SocketTunnelManager(string? ip = null, X509Certificate2? certificate = null): base(ip:ip, certificate:certificate)
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
        }

        private CancellationTokenSource? cancellationTokenSource;
        internal override void StartAcceptSocket()
        {
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                await OnAcceptSocketAsync();
            },
            cancellationTokenSource.Token);
        }

        private async Task OnAcceptSocketAsync()
        {
            while (ListeningSocket != null)
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
            string ForbiddenRequest = "HTTP/1.1 403 Forbidden\r\n\r\n";
            byte[] buffer = new byte[Settings.BufferSize];

            Func<CancellationToken, Task<int?>> ConnectAsyncDelegate = async cts =>
                await tunnel.client.ReceiveAsync(buffer: buffer, cancellationToken: cts);
            int? response = await TunnelExecutor.Commit(method: ConnectAsyncDelegate);

            if (response.Value == 0)
            {
                await TunnelExecutor.CloseSocketAsync(tunnel.client);
                await TunnelExecutor.CloseSSLStreamAsync(tunnel.sslClientStream);
            }
            var result = await TryCreateTunnel(buffer, tunnelStruct: tunnel);
            if (result == false)
            {
                Func<CancellationToken, Task> SendAsyncDelegate = async cts =>
                await tunnel.client.SendAsync(Encoding.UTF8.GetBytes(ForbiddenRequest), cts);
                await TunnelExecutor.Commit(action: SendAsyncDelegate);
                await TunnelExecutor.CloseSocketAsync(tunnel.client);
            }
        }

        private async Task<bool> TryCreateTunnel(byte[] buffer, TunnelStructure tunnelStruct)
        {
            string EstablishedRequest = "HTTP/1.1 200 Connection Established\r\n\r\n";
            RequestStruct? request;
            TryCheckHttpMethod(buffer, out request);
            tunnelStruct.remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (request == null)
            {
                throw new ArgumentException("requst is null");
            }
            try
            {
                Func<CancellationToken, Task> ConnectAsyncDelegate = async cts =>
                    await tunnelStruct.remote.ConnectAsync(request.Value.requestLine.endPoint,
                    cancellationToken: cts);
                await TunnelExecutor.Commit(action: ConnectAsyncDelegate);
                Func<CancellationToken, Task<int?>> SendAsyncDelegate = async cts => await tunnelStruct.client.SendAsync(
                    Encoding.UTF8.GetBytes(EstablishedRequest), cts);
                var sended = await TunnelExecutor.Commit(method: SendAsyncDelegate);
                if (sended != EstablishedRequest.Length)
                {
                    throw new ArgumentException("The length of the received data doesn't match the length of the sent data.");
                }
            }
            catch (Exception e)
            {
                await TunnelExecutor.CloseSocketAsync(tunnelStruct.remote);
                await TunnelExecutor.CloseSocketAsync(tunnelStruct.client);
                _logger.LogError($"TryCreateTunnel has created the exception: {e.Message}");
            }
            TunnelExecutor tunnel = new TunnelExecutor(tunnelStruct, _logger, certificate: ServerCertificate);
            tunnelings.Add(tunnelStruct.key, tunnel);
            try
            {
                tunnel.StartTunneling();
            }
            catch (Exception e)
            {
                _logger.LogError($"Tunneling exception: {e.Message}");
            }
            return true;
        }
    }
}

