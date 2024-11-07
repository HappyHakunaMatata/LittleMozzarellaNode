using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Common.Certificate.Interfaces;
using Common.Certificate.Models;

namespace Common.Certificate
{
    public abstract class CertificateDirector
    {

        public async Task<FullCertificateAuthority?> GenerateFullCertificateAuthority(AbstractCertificateBuilder builder, CertificateSettings settings, CancellationToken ctx = default)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(settings);
            try
            {
                await builder.GenerateKeys(settings.minDifficulty, settings.concurrency, ctx, settings.Difficulty);
                if (builder.PrivateKey == null || builder.PublicKey == null || builder.ID == null)
                {
                    return null;
                }
                var cert = builder.CreateSelfSignedCertificate(builder.PrivateKey, builder.PublicKey, settings);
                var ca = new FullCertificateAuthority(cert, builder.ID, builder.PrivateKey);
                return ca;
            }
            catch
            {
                throw;
            }
        }

        public X509Certificate2 CreateSelfSignedCertificate(AbstractCertificateBuilder builder, byte[] privateKey, byte[] publicKey, CertificateSettings settings)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(privateKey);
            ArgumentNullException.ThrowIfNull(publicKey);
            ArgumentNullException.ThrowIfNull(settings);

            try
            {
                return builder.CreateSelfSignedCertificate(privateKey, publicKey, settings);
            }
            catch
            {
                throw;
            }
        }

        public FullIdentity CreateIdentity(AbstractCertificateBuilder builder, string path, FullCertificateAuthority ca, CertificateSettings settings)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(ca);
            ArgumentNullException.ThrowIfNullOrEmpty(path);
            ArgumentNullException.ThrowIfNull(settings);
            try
            {
                var template = builder.CreateLeafTemplate(settings);
                var identity = builder.NewIdentity(ca.Cert, ca.PrivateKey.ExportECPrivateKey(), template, out var identityKey);
                return identity;
            }
            catch
            {
                throw;
            }
        }

        public X509Certificate2Collection FindCertificates(string findValue, StoreName storeName = StoreName.My,
            StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            ArgumentNullException.ThrowIfNull(findValue);
            var x509Store = new X509Store(storeName, storeLocation);
            try
            {
                x509Store.Open(OpenFlags.OpenExistingOnly);
                return x509Store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, findValue, false);
            }
            catch
            {
                throw;
            }
            finally
            {
                x509Store.Close();
            }
        }

        public void AddToStoreCertificate(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            ArgumentNullException.ThrowIfNull(certificate);
            var x509Store = new X509Store(storeName, storeLocation);
            try
            {
                x509Store.Open(OpenFlags.ReadWrite);
                x509Store.Add(certificate);
            }
            catch
            {
                throw;
            }
            finally
            {
                x509Store.Close();
            }
        }


        public void RemoveFromStoreCertificate(X509Certificate2 x509Certificate,
            StoreName storeName = StoreName.My,
            StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            var x509Store = new X509Store(storeName, storeLocation);
            try
            {
                x509Store.Open(OpenFlags.ReadWrite);
                x509Store.Remove(x509Certificate);
            }
            catch
            {
                throw;
            }
            finally
            {
                x509Store.Close();
            }
        }


        public abstract bool TrustRootCertificateAsAdmin(X509Certificate2 certificate);
        public abstract bool RemoveTrustedRootCertificateAsAdmin(X509Certificate2 certificate);
        internal readonly Regex CnRemoverRegex = new(@"^CN\s*=\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
