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

namespace Node
{
    class Program
    {
        
        static void Main()
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
                return;
            }


            using YggdrasilTest client = new YggdrasilTest();
            string address = "";
            client.Useconffile();
            var task = Task.Run(async () => await client.RunYggdrasilAsync());
            var ip = client.GetIPAddress();
            if (ip.Item1 == 0)
            {
                address = ip.Item2;
            }

           
            X509Certificate2Collection collection = certificateManager.FindCertificates($"CN={address}");
            var value = collection.FirstOrDefault();
            if (value != null)
            {
                certificateManager.RootCertificate = value;
            }
            else
            {
                CertificateSettings certificateSettings = new();
                certificateSettings.Host = address;
                certificateManager.RootCertificate = certificateManager.GenerateCertificate(certificateSettings);
                certificateManager.TrustRootCertificateAsAdmin();
            }
            if (task.IsCompleted)
            {
                Console.WriteLine($"Yggdrasil error: {task.Result.Item1}\n Yggdrasil message: {task.Result.Item2}");
            }


            /*catch
            {
                //Environment.Exit(1);
            }*/



            SocketTunnelManager tunnel = new(certificate: value, ip: "172.20.10.7"); //address
            tunnel.ListeningAddress = System.Net.IPAddress.Parse("172.20.10.7");
            tunnel.OpenSocketAsync();
            //MainYggdrasil.Exit();
        }

    }
}
