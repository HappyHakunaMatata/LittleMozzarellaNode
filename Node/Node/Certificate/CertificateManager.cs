using System.Security.Cryptography.X509Certificates;
using Node.Certificate.Models;
using System.Text.RegularExpressions;

namespace Node.Certificate
{
    public abstract class CertificateManager : IDisposable
    {

        private X509Certificate2? rootCertificate;

        public CertificateSettings certificateSettings;

        ICertificate rootEngine = new CertificateMaker();

        public CertificateManager()
        {
            //this.certificateSettings = certificateSettings;
        }


        public X509Certificate2 RootCertificate
        {
            get
            {
                return rootCertificate;
            }
            set
            {
                rootCertificate = value;
            }
        }

        public X509Certificate2 GenerateCertificate(CertificateSettings certificateSettings,
            ICertificate? certificateEngine = null)
        {
            ICertificate Engine = rootEngine;
            if (certificateEngine != null)
            {
                Engine = certificateEngine;
            }
            return Engine.MakeCertificate(
                certificateSettings.Host,
                certificateSettings.RootCertificateName,
                RootCertificate);
        }

        public X509Certificate2Collection FindCertificates(string findValue, StoreName storeName = StoreName.My,
            StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            var x509Store = new X509Store(storeName, storeLocation);
            try
            {
                x509Store.Open(OpenFlags.OpenExistingOnly);
                return x509Store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, findValue, false);
            }
            finally
            {
                x509Store.Close();
            }
        }

        public void AddToStoreCertificate(X509Certificate2? x509certificate = null,
            StoreName storeName = StoreName.My,
            StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            X509Certificate2 certificate = RootCertificate;
            if (x509certificate != null)
            {
                certificate = x509certificate;
            }
            var x509Store = new X509Store(storeName, storeLocation);
            try
            {
                x509Store.Open(OpenFlags.ReadWrite);
                x509Store.Add(certificate);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                x509Store.Close();
            }
        }

        public void RemoveFromStoreCertificate(X509Certificate2? certificate = null,
            StoreName storeName = StoreName.My,
            StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            X509Certificate2 x509Certificate = this.RootCertificate;
            if (certificate != null)
            {
                x509Certificate = certificate;
            }
            var x509Store = new X509Store(storeName, storeLocation);
            try
            {
                x509Store.Open(OpenFlags.ReadWrite);
                x509Store.Remove(x509Certificate);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                x509Store.Close();
            }
        }

        public abstract bool TrustRootCertificateAsAdmin();
        public abstract bool RemoveTrustedRootCertificateAsAdmin();
        

        internal readonly Regex CnRemoverRegex =
        new(@"^CN\s*=\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
               
            }

            disposed = true;
        }

        ~CertificateManager()
        {
            Dispose(false);
        }
       
    }
}
