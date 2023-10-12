using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Security.Cryptography.Pkcs;

namespace Node.Certificate
{
    public abstract class CertificateDirector : IDisposable
    {

        private X509Certificate2? rootCertificate;


        public X509Certificate2 RootCertificate
        {
            get
            {
                if (rootCertificate != null)
                {
                    return rootCertificate;
                }
                throw new ArgumentNullException();
            }
            set
            {
                rootCertificate = value;
            }
        }

        public void GenerateCertificate(AbstractCertificateBuilder builder)
        {
            builder.CreateCertificate();
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

        public void Sign(string path, X509Certificate2 certificateToSign)
        {

            string yourCertificatePassword = "pass";
            X509Certificate2 Signer = new X509Certificate2(path, yourCertificatePassword, X509KeyStorageFlags.Exportable);
            CmsSigner cmsSigner = new CmsSigner(Signer);


            byte[] certificateBytes = certificateToSign.Export(X509ContentType.Cert);
            SignedCms signedCms = new SignedCms(new ContentInfo(certificateBytes));
            signedCms.ComputeSignature(cmsSigner);

            // Сохраните подписанный сертификат в файл
            byte[] signedData = signedCms.Encode();
            File.WriteAllBytes("путь_к_подписанному_сертификату.p7b", signedData);

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
        

        internal readonly Regex CnRemoverRegex = new(@"^CN\s*=\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

        ~CertificateDirector()
        {
            Dispose(false);
        }
       
    }
}
