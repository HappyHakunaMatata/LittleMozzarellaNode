using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Common.Delegates;
using Org.BouncyCastle.Crypto;


namespace Common.Certificate
{
    public static class ConcreteSystemSecurityBuilderExtension
	{

        public static byte[]? GetPublicKeyFromPrivate(RSA rsa)
        {
            ArgumentNullException.ThrowIfNull(rsa);
            try
            {
                return rsa.ExportSubjectPublicKeyInfo();
            }
            catch
            {
                throw;
            }
        }

        public static byte[] SignWithoutHashing(this ConcreteSystemSecurityBuilder builder, RSA privKey, byte[] digest)
        {
            ArgumentNullException.ThrowIfNull(privKey);
            ArgumentNullException.ThrowIfNull(digest);
            try
            {
                return builder.SignRSAWithoutHashing(privKey, digest);
            }
            catch
            {
                throw;
            }
        }

        public static X509Certificate2 CreateSelfSignedCertificate(this ConcreteSystemSecurityBuilder builder, byte[] PrivateKey, byte[] PublicKey, CertificateTemplate template)
        {
            ArgumentNullException.ThrowIfNull(PrivateKey);
            ArgumentNullException.ThrowIfNull(PublicKey);
            ArgumentNullException.ThrowIfNull(template);
            try
            {
                using (var key = ECDsa.Create())
                {
                    key.ImportSubjectPublicKeyInfo(PublicKey, out _);
                    key.ImportPkcs8PrivateKey(PrivateKey, out _);
                    return builder.CreateSelfSignedCertificate(key, template);
                }
            }
            catch
            {
                throw;
            }
        }

    }
}

