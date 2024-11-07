using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Node.TunnelExecutors;
using Node.TunnelExecutors.Models;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tls;
using static System.Net.Mime.MediaTypeNames;

namespace Node.TunnelManagers
{
    //TODO: cancellationTokenSource Dispose
    public class SSLStreamTunnelManager : TunnelManager
    {
        protected readonly ILogger _logger;
        private readonly X509Certificate2Collection? collection;

        public SSLStreamTunnelManager(string? ip = null, X509Certificate2? certificate = null, X509Certificate2Collection? collection = null) : base(ip: ip, certificate: certificate)
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
            this.collection = collection;
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


        private SslStreamCertificateContext? BuildSslStreamCertificateContext() {
            if (this.ServerCertificate != null)
            {
                SslCertificateTrust trust = SslCertificateTrust.CreateForX509Collection(this.collection, false);
                
                var ssl = SslStreamCertificateContext.Create(this.ServerCertificate, this.collection, offline: true, trust: trust);

                Console.WriteLine($"Lenght: {ssl.IntermediateCertificates.Count}");
                foreach (var i in this.collection)
                {
                    Console.WriteLine(i.Subject);
                }
                return ssl;
            }
            return null;
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
                    SslStream sslStream = new SslStream(
                        innerStream: new NetworkStream(accept),
                        leaveInnerStreamOpen: false);
                    SslServerAuthenticationOptions options = new SslServerAuthenticationOptions()
                    {
                        //ServerCertificateContext = BuildSslStreamCertificateContext(),
                        ApplicationProtocols = new List<SslApplicationProtocol>
                        {
                            SslApplicationProtocol.Http11,
                        },
                        AllowRenegotiation = false,
                        ServerCertificate = ServerCertificate,
                        
                        EnabledSslProtocols = SslProtocols.Tls12,
                        ClientCertificateRequired = false,

                    };
                    
                    SSLTunnelExecutor executor = new(_logger);
                    await executor.AuthenticateAsServerAsync(sslStream, options);
                    tunnel.client = accept;
                    tunnel.sslClientStream = sslStream;
                    await ConnectAsync(tunnel, executor);
                    //ScanForExpiredTunneling();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.InnerException);
                    //await Tunneling.CloseSSLStreamAsync(tunnel.sslClientStream);
                    _logger.LogError($"Request error: {e}");
                }
            }
        }

        private async Task ConnectAsync(TunnelStructure tunnel, SSLTunnelExecutor executor)
        {
            string ForbiddenRequest = "HTTP/1.1 403 Forbidden\r\n\r\n";
            byte[] buffer = new byte[Settings.BufferSize];

          
            int? response = await executor.ReceiveAsync(tunnel.sslClientStream, buffer);
            if (response.Value == 0)
            {
                await executor.CloseSSLStreamAsync(tunnel.sslClientStream);
            }
            var result = await TryCreateTunnel(buffer, tunnelStruct: tunnel, executor);
            if (result == false)
            {
                buffer = Encoding.UTF8.GetBytes(ForbiddenRequest);
                await executor.SendAsync(tunnel.sslClientStream, buffer, buffer.Length);
                await executor.CloseSSLStreamAsync(tunnel.sslClientStream);
            }
        }

        public async Task<bool> TryCreateTunnel(byte[] buffer, TunnelStructure tunnelStruct, SSLTunnelExecutor executor)
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
                await executor.ConnectAsync(tunnelStruct.remote, request.Value.requestLine.endPoint);
                SslStream sslRemoteStream = new SslStream(new NetworkStream(tunnelStruct.remote), false);
                await executor.AuthenticateAsClientAsync(sslRemoteStream, options);
                tunnelStruct.sslRemoteStream = sslRemoteStream;

                buffer = Encoding.UTF8.GetBytes(EstablishedRequest);
                await executor.SendAsync(tunnelStruct.sslClientStream, buffer, buffer.Length);
                //tunnelStruct.sslRemoteStream.CopyToAsync();
            }
            catch (Exception e)
            {
                await executor.CloseSSLStreamAsync(tunnelStruct.sslClientStream);
                await executor.CloseSSLStreamAsync(tunnelStruct.sslRemoteStream);
                //await Tunneling.CloseSSLStreamAsync(sock.sslRemoteStream);
                //await Tunneling.CloseSSLStreamAsync(sock.sslClientStream);
                _logger.LogError($"TryCreateTunnel has created the exception: {e.Message}");
                throw;
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

