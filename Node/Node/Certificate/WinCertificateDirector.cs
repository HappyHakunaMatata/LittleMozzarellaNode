using System;
using Node.Certificate.Models;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;

namespace Node.Certificate
{
	public class WinCertificateDirector : CertificateDirector
    {

        private readonly CertificateSettings _certificateSettings;
        public WinCertificateDirector(CertificateSettings certificateSettings)
        {
            _certificateSettings = certificateSettings;
        }

        public override bool TrustRootCertificateAsAdmin()
        {

            AddToStoreCertificate(storeName: StoreName.My, storeLocation: StoreLocation.CurrentUser);
            var pfxFileName = Path.GetTempFileName();
            File.WriteAllBytes(pfxFileName, RootCertificate!.Export(X509ContentType.Pkcs12, _certificateSettings.PfxPassword));


            var info = new ProcessStartInfo
            {
                FileName = "certutil.exe",
                CreateNoWindow = true,
                UseShellExecute = true,
                Verb = "runas",
                ErrorDialog = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            if (!machineTrusted)
                info.Arguments = "-f -user -p \"" + _certificateSettings.PfxPassword + "\" -importpfx root \"" + pfxFileName + "\"";
            else
                info.Arguments = "-importPFX -p \"" + _certificateSettings.PfxPassword + "\" -f \"" + pfxFileName + "\"";

            try
            {
                var process = Process.Start(info);
                if (process == null) return false;

                process.WaitForExit();
            }
            catch
            {

                return false;
            }
            finally
            {
                File.Delete(pfxFileName);
            }

            return true;
        }

        public bool machineTrusted = false;

        public override bool RemoveTrustedRootCertificateAsAdmin()
        {
            
            RemoveFromStoreCertificate(RootCertificate, StoreName.My, StoreLocation.CurrentUser);

            var infos = new List<ProcessStartInfo>();
            if (!machineTrusted)
                infos.Add(new ProcessStartInfo
                {
                    FileName = "certutil.exe",
                    Arguments = "-delstore -user Root \"" + _certificateSettings.RootCertificateName + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    Verb = "runas",
                    ErrorDialog = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            else
                infos.AddRange(
                    new List<ProcessStartInfo>
                    {
                    new()
                    {
                        FileName = "certutil.exe",
                        Arguments = "-delstore My \"" + _certificateSettings.RootCertificateName + "\"",
                        CreateNoWindow = true,
                        UseShellExecute = true,
                        Verb = "runas",
                        ErrorDialog = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    },

                    new()
                    {
                        FileName = "certutil.exe",
                        Arguments = "-delstore Root \"" + _certificateSettings.RootCertificateName + "\"",
                        CreateNoWindow = true,
                        UseShellExecute = true,
                        Verb = "runas",
                        ErrorDialog = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                    });

            var success = true;
            try
            {
                foreach (var info in infos)
                {
                    var process = Process.Start(info);

                    if (process == null)
                    {
                        success = false;
                    }

                    process?.WaitForExit();
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }
    }
}

