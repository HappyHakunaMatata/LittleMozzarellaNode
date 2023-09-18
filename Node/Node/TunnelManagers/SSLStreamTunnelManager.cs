using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;
using Node.TunnelExecutors;
using Node.TunnelExecutors.Models;

namespace Node.TunnelManagers
{

	public class SSLStreamTunnelManager : TunnelManager
    {
        protected readonly ILogger _logger;

        public SSLStreamTunnelManager(string? ip = null, X509Certificate2? certificate = null) : base(ip: ip, certificate: certificate)
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
                    SslStream sslStream = new SslStream(
                        innerStream: new NetworkStream(accept),
                    leaveInnerStreamOpen: false);

                    SslServerAuthenticationOptions options = new SslServerAuthenticationOptions()
                    {
                        ApplicationProtocols = new List<SslApplicationProtocol>
                        {
                            SslApplicationProtocol.Http11,
                        },
                        AllowRenegotiation = false,
                        ServerCertificate = ServerCertificate,
                        EnabledSslProtocols = SslProtocols.Tls12,
                        ClientCertificateRequired = false,

                    };
                    Func<CancellationToken, Task> AuthenticateAsServerAsyncDelegate = async cts =>
                        await sslStream.AuthenticateAsServerAsync(options, cancellationToken: cts);
                    await TunnelExecutor.Commit(action: AuthenticateAsServerAsyncDelegate);
                    tunnel.client = accept;
                    tunnel.sslClientStream = sslStream;
                    await ConnectAsync(tunnel);
                    //ScanForExpiredTunneling();
                }
                catch (Exception e)
                {
                    //await Tunneling.CloseSSLStreamAsync(tunnel.sslClientStream);
                    _logger.LogError($"Request error: {e}");
                }
            }
        }

        private async Task ConnectAsync(TunnelStructure tunnel)
        {
            string ForbiddenRequest = "HTTP/1.1 403 Forbidden\r\n\r\n";
            byte[] buffer = new byte[Settings.BufferSize];

            Func<CancellationToken, Task<int?>> ConnectAsyncDelegate = async cts =>
                await tunnel.sslClientStream.ReadAsync(buffer: buffer, cancellationToken: cts);
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
                //await Tunneling.CloseSSLStreamAsync(tunnel.sslClientStream);
            }
        }

        public async Task<bool> TryCreateTunnel(byte[] buffer, TunnelStructure tunnelStruct)
        {
            string EstablishedRequest = "HTTP/1.1 200 Connection Established\r\n\r\n";
            RequestStruct? request;
            TryCheckHttpMethod(buffer, out request);
            tunnelStruct.remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (request == null)
            {
                throw new ArgumentException("requst is null");
            }
            X509Certificate2Collection serverCertificates = new X509Certificate2Collection();
            serverCertificates.Add(ServerCertificate);
            SslClientAuthenticationOptions options = new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>
                {
                    SslApplicationProtocol.Http11,
                },
                AllowRenegotiation = false,
                ClientCertificates = serverCertificates,
                TargetHost = request.Value.requestLine.Host,
                EnabledSslProtocols = SslProtocols.None,
                RemoteCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate),
            };
            try
            {
                Func<CancellationToken, Task> ConnectAsyncDelegate = async cts =>
                    await tunnelStruct.remote.ConnectAsync(request.Value.requestLine.endPoint,
                    cancellationToken: cts);
                await TunnelExecutor.Commit(action: ConnectAsyncDelegate);

                SslStream sslRemoteStream = new SslStream(new NetworkStream(tunnelStruct.remote), false);
                Func<CancellationToken, Task> AuthenticateAsClientAsyncDelegate = async cts =>
                    await sslRemoteStream.AuthenticateAsClientAsync(options, cancellationToken: cts);
                await TunnelExecutor.Commit(action: AuthenticateAsClientAsyncDelegate);
                tunnelStruct.sslRemoteStream = sslRemoteStream;

                Func<CancellationToken, Task> SendAsyncDelegate = async cts => await tunnelStruct.sslClientStream.WriteAsync(
                    Encoding.UTF8.GetBytes(EstablishedRequest), cts);
                var sended = await TunnelExecutor.Commit(action: SendAsyncDelegate);
                //tunnelStruct.sslRemoteStream.CopyToAsync();
                if (sended != EstablishedRequest.Length)
                {
                    throw new ArgumentException("The length of the received data doesn't match the length of the sent data.");
                }
            }
            catch (Exception e)
            {
                await TunnelExecutor.CloseSocketAsync(tunnelStruct.remote);
                await TunnelExecutor.CloseSocketAsync(tunnelStruct.client);
                //await Tunneling.CloseSSLStreamAsync(sock.sslRemoteStream);
                //await Tunneling.CloseSSLStreamAsync(sock.sslClientStream);
                _logger.LogError($"TryCreateTunnel has created the exception: {e.Message}");
                throw;
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

