using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Node.Examples
{
    public class Example
    {
        public void StartSSLClientExample()
        {
            string serverAddress = "www.bbc.com";
            int serverPort = 443;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(serverAddress, serverPort);
            NetworkStream networkStream = new NetworkStream(socket);
            SslStream sslStream = new SslStream(networkStream, false, ValidateServerCertificate);

            try
            {
                sslStream.AuthenticateAsClient(serverAddress);
                Console.WriteLine("Authenticated");
                string request = $"GET / HTTP/1.1\r\nHost: {serverAddress}:{serverPort}\r\n\r\n";
                //string request = $"CONNECT {serverAddress}:{serverPort} HTTP / 1.1\r\nHost: {serverAddress}:{serverPort}\r\n\r\n";
                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                sslStream.Write(requestBytes);
                Console.WriteLine("Sent");
                byte[] buffer = new byte[2048];
                int result = 0;
                Console.WriteLine("Start reading");
                do
                {
                    Console.WriteLine(result);
                    result = sslStream.Read(buffer);
                    string response = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine("Ответ от сервера:\n" + response);
                } while (result > 0);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Ошибка аутентификации: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
            }
            finally
            {
                sslStream.Close();
                networkStream.Close();
                socket.Close();
            }
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            Console.WriteLine("Ошибка проверки сертификата: " + sslPolicyErrors);
            return false;
        }

        public async Task StartQuicClientExample()
        {
            await Task.Run(async () =>
            {
                #pragma warning disable CA2252
                if (!QuicConnection.IsSupported)
                {
                    Console.WriteLine("QUIC is not supported, check for presence of libmsquic and support of TLS 1.3.");
                    return;
                }

                var clientConnectionOptions = new QuicClientConnectionOptions
                {
                    // End point of the server to connect to.
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse("172.20.10.7"), 8080),

                    // Used to abort stream if it's not properly closed by the user.
                    // See https://www.rfc-editor.org/rfc/rfc9000#section-20.2
                    DefaultStreamErrorCode = 0x0A, // Protocol-dependent error code.

                    // Used to close the connection if it's not done by the user.
                    // See https://www.rfc-editor.org/rfc/rfc9000#section-20.2
                    DefaultCloseErrorCode = 0x0B, // Protocol-dependent error code.

                    // Optionally set limits for inbound streams.
                    MaxInboundUnidirectionalStreams = 10,
                    MaxInboundBidirectionalStreams = 100,

                    // Same options as for client side SslStream.
                    ClientAuthenticationOptions = new SslClientAuthenticationOptions
                    {
                        // List of supported application protocols.
                        ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http11 }
                    }
                };

                // Initialize, configure and connect to the server.
                var connection = await QuicConnection.ConnectAsync(clientConnectionOptions);

                Console.WriteLine($"Connected {connection.LocalEndPoint} --> {connection.RemoteEndPoint}");
                #pragma warning restore CA2252
            });
        }

        public async Task StartSocketExample()
        {
            await Task.Run(async () =>
            {
                using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        Console.WriteLine("Connecting...");
                        await client.ConnectAsync(System.Net.IPAddress.Parse("172.20.10.7"), 8080);
                        await client.SendAsync(Encoding.UTF8.GetBytes("CONNECT www.example.com:443 HTTP/1.1\r\n\r\n"));
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка TLS: " + ex.Message);
                    }
                }
            });
        }
    }
}

