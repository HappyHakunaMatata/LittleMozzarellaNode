using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ClientTest;

public class Client
{

    public void StartSSLClientExample()
    {
        var serverAddress = System.Net.IPAddress.Loopback;
        string web = "www.bbc.com";
        int serverPort = 8080;
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Console.WriteLine("Connecting");
        socket.Connect(serverAddress, serverPort);
        NetworkStream networkStream = new NetworkStream(socket);
        SslProtocols protocol = SslProtocols.Tls12;
        SslStream sslStream = new SslStream(networkStream, false, ValidateServerCertificate);
        try
        {
            Console.WriteLine(serverAddress.ToString());
            sslStream.AuthenticateAsClient(serverAddress.ToString(), null, protocol, false);
            Console.WriteLine("Authenticated");
            //string request = $"GET / HTTP/1.1\r\nHost: {serverAddress}:{serverPort}\r\n\r\n";
            string request = $"CONNECT {web}:{443} HTTP / 1.1\r\nHost: {web}:{443}\r\n\r\n";
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
                Console.WriteLine("Response:\n" + response);
            } while (result > 0);
        }
        catch (AuthenticationException e)
        {
            Console.WriteLine("Authentication exception: " + e.Message);
            if (e.InnerException != null)
            {
                Console.WriteLine("Inner Exception Details: " + e.InnerException.Message);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
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
        {
            return true;
        }


        X509ChainPolicy chainPolicy = new X509ChainPolicy();
        chainPolicy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority;
        chainPolicy.RevocationMode = X509RevocationMode.NoCheck;

        chain.ChainPolicy = chainPolicy;
        bool isValidChain = chain.Build(new X509Certificate2(certificate));
        foreach (var i in chain.ChainStatus)
        {
            Console.WriteLine(i.Status);
        }


        return isValidChain;
    }

    /*public async Task StartQuicClientExample()
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
    }*/

    public async Task StartSocketExample()
    {
        await Task.Run(async () =>
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    Console.WriteLine("Connecting...");
                    await client.ConnectAsync(System.Net.IPAddress.Parse("127.0.0.1"), 8080);
                    await client.SendAsync(Encoding.UTF8.GetBytes("CONNECT www.example.com:443 HTTP/1.1\r\n\r\n"));
                }

                catch (Exception ex)
                {
                    Console.WriteLine("TLS exception: " + ex.Message);
                }
            }
        });
    }

}

