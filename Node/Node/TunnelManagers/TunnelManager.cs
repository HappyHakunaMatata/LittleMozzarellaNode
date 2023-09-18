using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Node.TunnelExecutors.Models;
using Microsoft.Extensions.Internal;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Node.TunnelExecutors;

namespace Node.TunnelManagers
{
    public abstract class TunnelManager : IDisposable
    {
        protected readonly ILogger _logger;
        protected static Socket? ListeningSocket;
        public IPAddress? ListeningAddress;
        protected int ListeningPort { get; set; } = 8080;
        private X509Certificate2? _serverCertificate;


        /// <summary>
        ///     Is the proxy currently running?
        /// </summary>
        public bool Status { get; private set; }
        protected TunnelStructureStore tunnelings = new TunnelStructureStore();



        public TunnelManager(string? ip = null, X509Certificate2? certificate = null)
        {
            ArgumentNullException.ThrowIfNull(certificate);
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });
            _logger = loggerFactory.CreateLogger<TunnelManager>();
            this.Status = true;
            _systemClock = new SystemClock();
            ServerCertificate = certificate;
            //AddEndPoint(ip);
        }

        public X509Certificate2 ServerCertificate
        {
            get
            {
                ArgumentNullException.ThrowIfNull(_serverCertificate);
                return _serverCertificate;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                _serverCertificate = value;
            }
        }

        public void AddEndPoint(string? ipAddress = null)
        {
            switch (ipAddress)
            {
                case null:
                    {
                        var result = NetworkInterface.GetAllNetworkInterfaces().
                        Where(m => m.OperationalStatus == OperationalStatus.Up).
                        Select(m => m.GetIPProperties()).
                        SelectMany(m => m.UnicastAddresses.
                        Where(m => m.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).
                        Select(m => m.Address).Where(m => m.GetAddressBytes()[0] == 2)).FirstOrDefault();
                        this.ListeningAddress = result;
                        break;
                    }
                default:
                    {
                        try
                        {
                            var ip = IPAddress.Parse(ipAddress);
                            IsValid(ip, out this.ListeningAddress);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"AddEndPoint caused next error: {e.Message}");
                        }
                        break;
                    }
            }
            if (this.ListeningAddress == null)
            {
                this.Dispose();
                _logger.LogError("No available IP");
            }
        }

        public bool IsValid(IPAddress address, out IPAddress? ValidAddress)
        {
            try
            {
                byte[] ipAdressBytes = address.GetAddressBytes();
                if (ipAdressBytes[0] == 2)
                {
                    ValidAddress = address;
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"IsValid caused next error {e.Message}");
            }
            ValidAddress = null;
            return false;
        }


        public void OpenSocketAsync(AddressFamily addressFamily = AddressFamily.InterNetwork, //InterNetworkV6
            SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp)
        {
            ArgumentNullException.ThrowIfNull(ListeningAddress);
            try
            {
                ListeningSocket = new Socket(addressFamily, socketType, protocolType);
                ListeningSocket.Bind(new IPEndPoint(ListeningAddress, ListeningPort));
                ListeningSocket.Listen(10);
                _logger.LogInformation("Listening: {0}:{1}", ListeningAddress, ListeningPort);
                StartAcceptSocket();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }


        internal abstract void StartAcceptSocket();


        public bool TryCheckHttpMethod(byte[] buffer, out RequestStruct? requestStruct)
        {
            try
            {
                string VersionPattern = @"^HTTP\/(1\.1|2|3)\b$";
                requestStruct = new RequestStruct(buffer);
                Match versionMatch = Regex.Match(requestStruct.Value.requestLine.Version, VersionPattern);
                if (requestStruct.Value.requestLine.Method != "CONNECT" || !versionMatch.Success)
                {
                    requestStruct = null;
                    return false;
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        

        private byte[] GetConnectBytes(string host, int port)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("CONNECT {0}:{1} HTTP/1.1", host, port));
            sb.AppendLine(string.Format("Host: {0}:{1}", host, port));
            /*if (!string.IsNullOrEmpty(Username))
            {
                var auth =
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Username, Password)));
                sb.AppendLine(string.Format("Proxy-Authorization: Basic {0}", auth));
            }*/

            sb.AppendLine();
            var buffer = Encoding.UTF8.GetBytes(sb.ToString());
            return buffer;
        }

        public bool ValidateServerCertificate(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            _logger.LogError("Certificate error: {0}", sslPolicyErrors);
            return false;
        }

        public static X509Certificate SelectLocalCertificate(
            object sender,
            string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate,
            string[] acceptableIssuers)
        {
            Console.WriteLine("Client is selecting a local certificate.");
            if (acceptableIssuers != null &&
                acceptableIssuers.Length > 0 &&
                localCertificates != null &&
                localCertificates.Count > 0)
            {
                foreach (X509Certificate certificate in localCertificates)
                {
                    string issuer = certificate.Issuer;
                    if (Array.IndexOf(acceptableIssuers, issuer) != -1)
                        return certificate;
                }
            }
            if (localCertificates != null &&
                localCertificates.Count > 0)
                return localCertificates[0];

            return null;
        }
        

        /*
        private void Log(string logMessage)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine(logMessage);
            }
        }

        private void ClearLog()
        {
            File.WriteAllText("log.txt", string.Empty);
        }*/

        private bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            if (Status)
                try
                {
                    Stop();
                }
                catch
                {
                    throw;
                }
            if (disposing)
            {
                ListeningSocket?.Dispose();
                Parallel.ForEach(tunnelings, tunnel =>
                {
                    tunnel.Value.Dispose();
                    tunnelings.Remove(tunnel.Key);
                });
            }
        }

        public void Stop()
        {
            if (!Status)
            {
                throw new Exception("Proxy is not running.");
            }
            if (ListeningSocket != null)
            {
                Task.Run(async () => await TunnelExecutor.CloseSocketAsync(ListeningSocket));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
        private readonly Object _mutex = new Object();
        private ISystemClock _systemClock;
        private DateTimeOffset _lastExpirationScan;

        // Called by multiple actions to see how long it's been since we last checked for expired items.
        // If sufficient time has elapsed then a scan is initiated on a background task.
        private void ScanForExpiredTunneling()
        {
            lock (_mutex)
            {
                var utcNow = _systemClock.UtcNow;
                if ((utcNow - _lastExpirationScan) > Settings.ExpiredItemsDeletionInterval)
                {
                    _lastExpirationScan = utcNow;
                    Task.Run(() => {
                        foreach(var i in tunnelings)
                        {
                            if (i.Value.IsExpired())
                            {
                                i.Value.Dispose();
                            }
                        }
                    });
                }
            }
        }
    }
}

