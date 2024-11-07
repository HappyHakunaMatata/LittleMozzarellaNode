using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;


namespace Common.Certificate
{
	public class OSXCertificateDirector : CertificateDirector
    {
		

        public override bool RemoveTrustedRootCertificateAsAdmin(X509Certificate2 certificate)
        {
            ArgumentNullException.ThrowIfNull(certificate);
            string FileName = certificate.Subject;
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
                RemoveFromStoreCertificate(certificate);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public override bool TrustRootCertificateAsAdmin(X509Certificate2 certificate)
        {
            var pfxFileName = Path.GetTempFileName();
            try
            {
                AddToStoreCertificate(certificate, storeName: StoreName.My, storeLocation: StoreLocation.CurrentUser);
                File.WriteAllBytes(pfxFileName, certificate.Export(X509ContentType.Cert));
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
                var process = Process.Start(info);
                if (process == null)
                {
                    return false;
                }
                process.WaitForExit();
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                File.Delete(pfxFileName);
            }
        }
    }
}

