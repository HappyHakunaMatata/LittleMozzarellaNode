﻿using System.Security.Cryptography.X509Certificates;
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

namespace Node
{
    class Program
    {
        static void Main()
        {
            string address = "0.0.0.0";
            string name = "0.0.0.0";
            CertificateSettings certificateSettings = new();
            certificateSettings.Host = address;
            certificateSettings.RootCertificateName = name;
            certificateSettings.IssuerName = name;
            var certificateDirector = new OSXCertificateDirector();
            ConcreteBouncyCastleCertificateBuilder builder = new(certificateSettings);
            certificateDirector.GenerateCertificate(builder);
            certificateDirector.RootCertificate = builder.GetResult();
            
            var t = certificateDirector.TrustRootCertificateAsAdmin();
            Console.WriteLine(t);
        }
        static void Main2()
        {
            string address = "127.0.0.1";
            string name = "127.0.0.1";
            CertificateSettings certificateSettings = new();
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
                certificateDirector.GenerateCertificate(builder);
                certificateDirector.RootCertificate = builder.GetResult();
                certificateDirector.TrustRootCertificateAsAdmin();
                collection = certificateDirector.FindCertificates($"CN={name}");
                value = collection.FirstOrDefault();
            }
            if (task.IsCompleted)
            {
                Console.WriteLine($"Yggdrasil error: {task.Result.Item1}\n Yggdrasil message: {task.Result.Item2}");
            }


            /*catch
            {
                //Environment.Exit(1);
            }*/



            SSLStreamTunnelManager tunnel = new(certificate: value, ip: "127.0.0.1"); //address 172.20.10.7
            tunnel.ListeningAddress = System.Net.IPAddress.Parse("127.0.0.1");

            Task.Run(() => tunnel.OpenSocketAsync());
            Example example = new();
            example.StartSSLClientExample();
            //MainYggdrasil.Exit();
        }

    }
}
