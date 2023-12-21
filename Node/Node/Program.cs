using System.Security.Cryptography.X509Certificates;
using Node.Certificate;
using Node.TunnelManagers;
using System.Net;
using Node.Certificate.Models;
using System.Runtime.InteropServices;
using Node.YggDrasil.cmd;
using Node.YggDrasil.models;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;
using Org.BouncyCastle.Utilities.Net;
using Node.Examples;
using Org.BouncyCastle.Tls;
using System.Security.Cryptography;
using System.Collections;
using System.Net.Sockets;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;
using System.Text;
using static System.Environment;

namespace Node
{
    class Program
    {
        static async Task Main()
        {

            CertificateSettings certificateSettings = new CertificateSettings(Certificate.Models.CertificateType.CA);
            CertificateAuthorityConfig certificateAuthorityConfig = new CertificateAuthorityConfig();
            certificateAuthorityConfig.Difficulty = 14;
            ConcreteSystemSecurityBuilder builder = new ConcreteSystemSecurityBuilder(certificateAuthorityConfig, certificateSettings);
            CertificateDirector director = new OSXCertificateDirector();
            var certificate = await director.GenerateFullCertificateAuthority(builder);
            certificateSettings = new CertificateSettings(Certificate.Models.CertificateType.Identity);
            builder = new ConcreteSystemSecurityBuilder(certificateSettings);
            director.CreateIdentity(builder, certificate);

            CertificateAuthorizer authorizer = new CertificateAuthorizer();
            var ca = authorizer.LoadFullCAConfig();
            //Console.WriteLine($"Serial: {ca.Cert}");
            Console.WriteLine($"Algorithm: {ca.algorithm.SignatureAlgorithm}");
            Console.WriteLine($"NodeID: {ca.NodeID.Length}");
            Console.WriteLine($"Rest chain: {ca.RestChain.Length}");
            
            authorizer = new CertificateAuthorizer(SpecialFolder.UserProfile);
            ca = authorizer.LoadFullCAConfig();
            //Console.WriteLine($"Serial: {ca.Cert}");
            Console.WriteLine($"Algorithm: {ca.algorithm.SignatureAlgorithm}");
            Console.WriteLine($"NodeID: {ca.NodeID.Length}");
            Console.WriteLine($"Rest chain: {ca.RestChain.Length}");




            /*Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8080));
            socket.Listen(10);
            Console.WriteLine("Running");
            await socket.AcceptAsync();
            Console.WriteLine("Accepted");
            */

            /*CertificateSettings certificateSettings = new();
            ConcreteSystemSecurityBuilder builder = new(certificateSettings);
            var request = builder.CreateCertificateRequest("CN=localhost", HashAlgorithmName.SHA256);
            var base_64 = builder.GetBase64(request);
            CertificateRequestManager manager = new("localhost", base_64);
            var file = manager.ReadYawl();
            if (file != null)
            {
                manager.CreateRequest(file);
            }
            else
            {
                Environment.Exit(1);
            }
            var client = manager.CreateClient();*/
            //await manager.Approve(client);
        }

        static void Main2()
        {
            string address = "127.0.0.1";
            string name = "127.0.0.1";
            CertificateSettings certificateSettings = new CertificateSettings(Certificate.Models.CertificateType.CA);
            certificateSettings.Host = address;
            certificateSettings.RootCertificateName = name;
            certificateSettings.IssuerName = name;


            CertificateDirector? certificateDirector = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                certificateDirector = new OSXCertificateDirector();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                certificateDirector = new WinCertificateDirector(certificateSettings);
            }
            if (certificateDirector == null)
            {
                return;
            }


            using YggdrasilTest client = new YggdrasilTest();
            client.Useconffile();
            var task = Task.Run(async () => await client.RunYggdrasilAsync());
            /*var ip = client.GetIPAddress();
            if (ip.Item1 == 0)
            {
                address = ip.Item2;
            }*/

           
            X509Certificate2Collection collection = certificateDirector.FindCertificates($"CN={name}");
            var value = collection.FirstOrDefault();
            if (value != null)
            {
                certificateDirector.RootCertificate = value;
            }
            else
            {
                ConcreteBouncyCastleCertificateBuilder builder = new(certificateSettings);
                //certificateDirector.GenerateCertificate(builder);
                certificateDirector.RootCertificate = builder.GetCertificate();
                certificateDirector.TrustRootCertificateAsAdmin();
                collection = certificateDirector.FindCertificates($"CN={name}");
                value = collection.FirstOrDefault();
            }
            if (task.IsCompleted)
            {
                Console.WriteLine($"Yggdrasil error: {task.Result.Item1}\n Yggdrasil message: {task.Result.Item2}");
            }



            SSLStreamTunnelManager tunnel = new(certificate: value, ip: "127.0.0.1"); //address 172.20.10.7
            tunnel.ListeningAddress = System.Net.IPAddress.Parse("127.0.0.1");

            Task.Run(() => tunnel.OpenSocketAsync());
            Example example = new();
            example.StartSSLClientExample();
            //MainYggdrasil.Exit();
        }

    }
}
