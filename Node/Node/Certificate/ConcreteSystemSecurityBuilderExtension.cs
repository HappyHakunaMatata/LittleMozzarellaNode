using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Node.Certificate.Delegates;
using Node.Certificate.Models;
using Node.Certificate.Template;

namespace Node.Certificate
{
    public static class ConcreteSystemSecurityBuilderExtension
	{

        

        public static void GenerateKeys(this ConcreteSystemSecurityBuilder builder, FoundCallBackDelegate FoundCallBack, ushort minDifficulty = 8, ushort concurrency = 4, CancellationToken cancellationToken = default)
        {
            var ctx = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            for (var i = 0; i < concurrency; i++)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        var k = builder.GenerateKey(ctx.Token, minDifficulty: minDifficulty);
                        if (ctx.IsCancellationRequested)
                        {
                            break;
                        }
                        if (k.GetPublicKey() == null || k.GetId() == null)
                        {
                            break;
                        }
                        var done = FoundCallBack(k.GetPublicKey(), k.GetId());
                        if (done)
                        {
                            break;
                        }
                    }
                });
            }
        }

        public static byte[]? GetPublicKeyFromPrivate(RSA rsa)
        {
            return rsa.ExportSubjectPublicKeyInfo();
        }

        public static byte[] SignWithoutHashing(this ConcreteSystemSecurityBuilder builder, RSA privKey, byte[] digest)
        {
            return builder.SignRSAWithoutHashing(privKey, digest);
        }

        public static X509Certificate2 CreateSelfSignedCertificate(this ConcreteSystemSecurityBuilder builder, byte[] PrivateKey, byte[] PublicKey, CertificateTemplate template)
        {
            ArgumentNullException.ThrowIfNull(PrivateKey);
            ArgumentNullException.ThrowIfNull(PublicKey);
            ArgumentNullException.ThrowIfNull(template);
            var key = ECDsa.Create();
            try
            {
                key.ImportSubjectPublicKeyInfo(PublicKey, out _);
                key.ImportPkcs8PrivateKey(PrivateKey, out _);
                return builder.CreateCertificate(key, template);
            }
            finally
            {
                key.Dispose();
            }
        }

    }
}

