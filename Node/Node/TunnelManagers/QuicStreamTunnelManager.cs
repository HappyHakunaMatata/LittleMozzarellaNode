using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace Node.TunnelManagers
{
	public class QuicStreamTunnelManager : TunnelManager
    {
        protected readonly ILogger _logger;

        public QuicStreamTunnelManager(string? ip = null, X509Certificate2? certificate = null) : base(ip: ip, certificate: certificate)
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
#pragma warning disable CA2252 // Этот API требует согласия на использование предварительных функций
            var serverConnectionOptions = new QuicServerConnectionOptions
            {

                DefaultStreamErrorCode = 0x0A, // Protocol-dependent error code.
                DefaultCloseErrorCode = 0x0B, // Protocol-dependent error code.
                ServerAuthenticationOptions = new SslServerAuthenticationOptions
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http11},
                    ServerCertificate = ServerCertificate
                }
            };

            var listener = await QuicListener.ListenAsync(new QuicListenerOptions
            {
                // Listening endpoint, port 0 means any port.
                ListenEndPoint = new IPEndPoint(ListeningAddress, ListeningPort),
                // List of all supported application protocols by this listener.
                ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http11 },
                // Callback to provide options for the incoming connections, it gets called once per each connection.
                ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(serverConnectionOptions)
            });
            while (true)
            {
                // Accept will propagate any exceptions that occurred during the connection establishment,
                // including exceptions thrown from ConnectionOptionsCallback, caused by invalid QuicServerConnectionOptions or TLS handshake failures.
                var connection = await listener.AcceptConnectionAsync();
                Console.WriteLine($"Connection: {connection.RemoteEndPoint}");
                // Process the connection...
            }
#pragma warning restore CA2252 // Этот API требует согласия на использование предварительных функций
        }
    }
}

