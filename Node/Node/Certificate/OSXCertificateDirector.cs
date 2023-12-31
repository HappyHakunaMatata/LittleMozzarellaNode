﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Node.Certificate.Models;

namespace Node.Certificate
{
	public class OSXCertificateDirector : CertificateDirector
    {
		

        public override bool RemoveTrustedRootCertificateAsAdmin()
        {
            string FileName = RootCertificate.Subject;

            string command = $"sudo security delete-certificate -c \"{CnRemoverRegex.Replace(FileName, string.Empty)}\" -t \"/Library/Keychains/System.keychain\"";
            var info = new ProcessStartInfo
            {
                FileName = @"/bin/zsh",
                Arguments = $"-c \"{command}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            try
            {
                var process = Process.Start(info);
                if (process == null)
                {
                    return false;
                }
                process.WaitForExit();
            }
            catch
            {
                return false;
            }
            RemoveFromStoreCertificate();
            return true;
        }

        public override bool TrustRootCertificateAsAdmin()
        {
            AddToStoreCertificate(storeName: StoreName.My, storeLocation: StoreLocation.CurrentUser);
            var pfxFileName = Path.GetTempFileName();
            File.WriteAllBytes(pfxFileName, RootCertificate.Export(X509ContentType.Cert));
            var command = $"sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain \"{pfxFileName}\"";
            var info = new ProcessStartInfo
            {
                FileName = @"/bin/zsh",
                Arguments = $"-c \"{command}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            try
            {
                var process = Process.Start(info);
                if (process == null)
                {
                    return false;
                }
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                File.Delete(pfxFileName);
            }
            return true;
        }
    }
}

