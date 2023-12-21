using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Security.Cryptography.Pkcs;
using Node.Certificate.Models;
using Node.Certificate.Template;
using System.Security.Cryptography;

namespace Node.Certificate
{
    public abstract class CertificateDirector
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

        public async Task<FullCertificateAuthority> GenerateFullCertificateAuthority(AbstractCertificateBuilder builder, GenerateTypes generateTypes = GenerateTypes.Threads)
        {
            await builder.GenerateKeys(generateTypes: generateTypes);
            var cert = builder.CreateSelfSignedCertificate(builder.GetPrivate(), builder.GetPublicKey());
            RootCertificate = cert;
            builder.SaveCertificate(cert);
            builder.SaveKey(builder.GetPrivate());
            return builder.GetCertificateAuthority();
        }

        public X509Certificate2 CreateSelfSignedCertificate(AbstractCertificateBuilder builder, byte[] privateKey, byte[] publicKey)
        {
            return builder.CreateSelfSignedCertificate(privateKey, publicKey);
        }

        public void CreateIdentity(AbstractCertificateBuilder builder, FullCertificateAuthority ca)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(ca);
            ArgumentNullException.ThrowIfNull(ca.PrivateKey);
            ArgumentNullException.ThrowIfNull(ca.Cert);
            try
            {
                var template = builder.CreateLeafTemplate();
                byte[]? identityKey = null;
                var identity = builder.NewIdentity(ca.Cert, ca.PrivateKey, template, out identityKey);
                builder.SaveIdentity(identity);
                if (identityKey != null)
                {
                    builder.SaveKey(identityKey);
                }
            }
            catch
            {
                throw;
            }
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
        internal readonly Regex CnRemoverRegex = new(@"^CN\s*=\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
