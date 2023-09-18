using System.Security.Cryptography.X509Certificates;
using Node.Certificate;
using Node.TunnelManagers;
using System.Net;
using Node.Certificate.Models;
using System.Runtime.InteropServices;
using Node.YggDrasil.cmd;
using Node.YggDrasil.models;

namespace Node
{
    class Program
    {
        
        static async Task Main()
        {
            CertificateManager? certificateManager = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                certificateManager = new OSXCertificateManager();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                certificateManager = new WinCertificateManager();
            }

            if (certificateManager == null)
            {
                //TODO exit
            }


            using YggdrasilTest client = new YggdrasilTest();
            
            client.Useconffile();
            var task = Task.Run(async () => await client.RunYggdrasilAsync());


            CertificateSettings certificateSettings = new();
            certificateSettings.Host = "201:a571:979b:7ece:d673:6ecd:7230:fc78";


            certificateManager.RootCertificate = certificateManager.GenerateCertificate(certificateSettings);
            
            X509Certificate2Collection collection = certificateManager.FindCertificates("CN=201:a571:979b:7ece:d673:6ecd:7230:fc78");
            if (collection.Count == 0)
            {
                certificateManager.RootCertificate = certificateManager.GenerateCertificate(certificateSettings);
                certificateManager.TrustRootCertificateAsAdmin();
            }
            else
            {
                certificateManager.RootCertificate = collection.FirstOrDefault();
            }

            if (task.IsCompleted)
            {
                Console.WriteLine($"Yggdrasil error: {task.Result.Item1}\n Yggdrasil message: {task.Result.Item2}");
            }
            int i = 1;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out i))
                {
                    if (i == 0)
                    {
                        break;
                    }
                }
            }






            //QuicStreamTunnelManager proxyServer = new(certificate: cert, ip: "201:a571:979b:7ece:d673:6ecd:7230:fc78"); //"201:a571:979b:7ece:d673:6ecd:7230:fc78"
            //proxyServer.ListeningAddress = IPAddress.Parse("172.20.10.7");
            //proxyServer.OpenSocketAsync();

            //MainYggdrasil.Exit();
        }

        

        /*controller.StartProxy();

        Console.Read();

        controller.Stop();*/

    }
}
