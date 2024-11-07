using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Common.Certificate.Enums;
using Common.Certificate.Interfaces;
using Common.Certificate.Models;
using static System.Security.Cryptography.ECCurve;


///TODO: MAKE TEST FOR CreateCertificate() 
///TODO: WHY WE HAVE DNS in certificate? 

namespace Common.Certificate
{
    public class ConcreteSystemSecurityBuilder : AbstractCertificateBuilder
    {


        public ConcreteSystemSecurityBuilder()
        {
        }



        public AsymmetricAlgorithm GeneratePrivateKey(AsymmetricAlgorithmName name = AsymmetricAlgorithmName.ECDSA)
        {
            try
            {
                if (name == AsymmetricAlgorithmName.RSA)
                {
                    return GeneratePrivateRSAKey();
                }
                return GeneratePrivateECDSAKey();
            }
            catch
            {
                throw;
            }
        }


        public RSA GeneratePrivateRSAKey(int keySizeInBits = 2048)
        {
            try
            {
                return RSA.Create(keySizeInBits);
            }
            catch
            {
                throw;
            }
        }


        public CertificateTemplate CreateCertificateTemplate(CertificateSettings settings)
        {
            try
            {
                var serialNumber = NewSerialNumber();

                var builder = new X500DistinguishedNameBuilder();
                if (!string.IsNullOrEmpty(settings.OrganizationName))
                {
                    builder.AddOrganizationName(settings.OrganizationName);
                }
                if (!string.IsNullOrEmpty(settings.RootCertificateName))
                {
                    builder.AddCommonName(settings.RootCertificateName);
                }
                var name = builder.Build();
                var sanBuilder = new SubjectAlternativeNameBuilder();
                if (settings.IP != null)
                {
                    sanBuilder.AddIpAddress(settings.IP);
                }
                var alternativeName = sanBuilder.Build();

               
                List<X509Extension> x509Extensions = new()
                {
                    alternativeName,
                    new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true),
                    new X509BasicConstraintsExtension(true, true, 0, true),
                    new X509Extension(new AsnEncodedData(new Oid(OidTypes.ExtVersion), new byte[] {settings.versionID}), false)
                };
                return new CertificateTemplate(serialNumber.ToByteArray(), name, x509Extensions);
            }
            catch
            {
                throw;
            }
        }


        public override CertificateTemplate CreateLeafTemplate(CertificateSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            try
            {
                var serialNumber = NewSerialNumber();

                var builder = new X500DistinguishedNameBuilder();
                if (!string.IsNullOrEmpty(settings.OrganizationName))
                {
                    builder.AddOrganizationName(settings.OrganizationName);
                }
                if (!string.IsNullOrEmpty(settings.RootCertificateName))
                {
                    builder.AddCommonName(settings.RootCertificateName);
                }
                var name = builder.Build();

                var sanBuilder = new SubjectAlternativeNameBuilder();
                if (settings.IP != null)
                {
                    sanBuilder.AddIpAddress(settings.IP);
                }
                var alternativeName = sanBuilder.Build();


                OidCollection oc = new OidCollection();
                oc.Add(new Oid(OidTypes.ExtKeyUsageServerAuth));
                oc.Add(new Oid(OidTypes.ExtKeyUsageClientAuth));
                List<X509Extension> x509Extensions = new()
                {
                    alternativeName,
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature|X509KeyUsageFlags.KeyEncipherment, true),
                    new X509BasicConstraintsExtension(false, true, 0, true),
                    new X509EnhancedKeyUsageExtension(oc, false),
                };
                return new CertificateTemplate(serialNumber.ToByteArray(), name, x509Extensions);
            }
            catch
            {
                throw;
            }
        }


        public X509Certificate CreateSelfSigned(CertificateRequest request, DateTime validFrom, DateTime validTo)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(validFrom);
            ArgumentNullException.ThrowIfNull(validTo);
            try
            {
                return request.CreateSelfSigned(validFrom, validTo);
            }
            catch
            {
                throw;
            }
        }


        /*public override void SaveKey(byte[] key, CertificateType type, string path)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(path);

            try
            {
                if (TrySaveEcdsaKey(key, type, path))
                {
                    return;
                }
                TrySaveRSAKey(key, type, path);
            }
            catch
            {
                throw;
            }
        }*/


        /*public bool TrySaveRSAKey(byte[] key, CertificateType type, string savepath)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(savepath);
            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportRSAPrivateKey(key, out _);
                    var pem = rsa.ExportPkcs8PrivateKeyPem();
                    if (type == CertificateType.CA)
                    {
                        File.WriteAllText(savepath, pem);
                    }
                    else
                    {
                        File.WriteAllText(savepath, pem);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }*/


        /*public bool TrySaveEcdsaKey(byte[] key, CertificateType type, string path)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(type);
            try
            {
                using (ECDsa ecdsa = ECDsa.Create())
                {
                    ecdsa.ImportPkcs8PrivateKey(key, out _);
                    var pem = ecdsa.ExportPkcs8PrivateKeyPem();
                    if (type == CertificateType.CA)
                    {
                        File.WriteAllText(path, pem);
                    }
                    else
                    {
                        File.WriteAllText(path, pem);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }*/



        public ECDsa GeneratePrivateECDSAKey()
        {
            try
            {
                ECCurve curve = NamedCurves.nistP256;
                return ECDsa.Create(curve);
            }
            catch
            {
                throw;
            }
        }




        public byte[] HashAndSign(ECDsa key, byte[] data)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(data);
            try
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    var digest = sha256Hash.ComputeHash(data);
                    return SignWithoutHashing(key, digest);
                }
            }
            catch
            {
                throw;
            }
        }

        
        public bool HashAndVerifySignature(byte[] key, byte[] data, byte[] signature)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(signature);
            try
            {
                using (var sha256Hash = SHA256.Create())
                {
                    var digest = sha256Hash.ComputeHash(data);
                    return VerifySignatureWithoutHashing(key, digest, signature);
                }
            }
            catch
            {
                throw;
            }
        }

        

        public override bool VerifyECDSASignatureWithoutHashing(byte[] key, byte[] data, byte[] signature)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(signature);
            try
            {
                using (var eCDsa = ECDsa.Create())
                {
                    eCDsa.ImportSubjectPublicKeyInfo(key, out _);
                    return eCDsa.VerifyHash(data, signature);
                }
            }
            catch
            {
                throw;
            }
        }


        public byte[] SignWithoutHashing(ECDsa privKey, byte[] digest)
        {
            ArgumentNullException.ThrowIfNull(privKey);
            ArgumentNullException.ThrowIfNull(digest);
            try
            {
                return SignECDSAWithoutHashing(privKey, digest);
            }
            catch
            {
                throw;
            }
        }

        public byte[] SignECDSAWithoutHashing(ECDsa privKey, byte[] digest)
        {
            ArgumentNullException.ThrowIfNull(privKey);
            ArgumentNullException.ThrowIfNull(digest);
            try
            {
                return privKey.SignHash(digest);
            }
            catch
            {
                throw;
            }
        }

        
        public byte[] GetPublicKeyFromPrivate(ECDsa ecdsa, out byte[] pkey)
        {
            ArgumentNullException.ThrowIfNull(ecdsa);
            try
            {
                pkey = ecdsa.ExportPkcs8PrivateKey();
                return ecdsa.ExportSubjectPublicKeyInfo();
            }
            catch
            {
                throw;
            }
        }

        public byte[] SignRSAWithoutHashing(RSA privKey, byte[] digest)
        {
            ArgumentNullException.ThrowIfNull(privKey);
            ArgumentNullException.ThrowIfNull(digest);
            try
            {
                return privKey.SignHash(digest, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            }
            catch
            {
                throw;
            }
        }

        
        public override GeneratedKeys? GenerateKey(CancellationToken ctx, UInt16 minDifficulty = 8)
        {
            UInt16 d;
            byte[]? id = null;
            byte[]? pkey = null;
            try
            {
                while (true)
                {
                    using (ECDsa ecdsa = GeneratePrivateECDSAKey())
                    {
                        var pubKey = GetPublicKeyFromPrivate(ecdsa, out pkey);
                        if (ctx.IsCancellationRequested)
                        {
                            break;
                        }
                        if (pubKey == null)
                        {
                            break;
                        }
                        id = NodeIDFromKey(pubKey);
                        d = GetDifficulty(id);
                        if (d >= minDifficulty)
                        {
                            return new GeneratedKeys(id, pkey, pubKey);
                        }
                    }
                }
                return null;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Creates self signed certificates with given private and public keys. ECDsa is only supported
        /// </summary>
        /// <param name="PrivateKey">Private key of ECDsa algorithm</param>
        /// <param name="PublicKey">Public key of ECDsa algorithm</param>
        /// <returns>X509Certificate2 certificate</returns>
        public override X509Certificate2 CreateSelfSignedCertificate(byte[] PrivateKey, byte[] PublicKey, CertificateSettings settings)
        {
            ArgumentNullException.ThrowIfNull(PrivateKey);
            ArgumentNullException.ThrowIfNull(PublicKey);
            ArgumentNullException.ThrowIfNull(settings);
            try
            {
                using (var key = ECDsa.Create())
                {
                    key.ImportSubjectPublicKeyInfo(PublicKey, out _);
                    key.ImportPkcs8PrivateKey(PrivateKey, out _);
                    var template = CreateCertificateTemplate(settings);
                    return CreateSelfSignedCertificate(key, template);
                }
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Creates double sha256 of given public key
        /// </summary>
        /// <param name="publickey">Is a given byte array</param>
        /// <returns>Byte array</returns>
        public override byte[] DoubleSHA256PublicKey(byte[] publickey)
        {
            ArgumentNullException.ThrowIfNull(publickey);
            try
            {
                byte[]? end;
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    var mid = sha256Hash.ComputeHash(publickey);
                    end = sha256Hash.ComputeHash(mid);
                }
                return end;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Creates new identity
        /// </summary>
        /// <param name="Certificate"></param>
        /// <param name="privateKey"></param>
        /// <param name="template"></param>
        /// <param name="privateEcdsaKey"></param>
        /// <returns>FullIdentity class</returns>
        public override FullIdentity NewIdentity(X509Certificate2 Certificate, byte[] privateKey, CertificateTemplate template, out byte[]? privateEcdsaKey)
        {
            ArgumentNullException.ThrowIfNull(Certificate);
            ArgumentNullException.ThrowIfNull(privateKey);
            ArgumentNullException.ThrowIfNull(template);
            try
            {
                using (ECDsa ecdsa = (ECDsa)GeneratePrivateKey())
                {
                    var PublicKey = GetPublicKeyFromPrivate(ecdsa, out _);
                    return SignCertificate(Certificate, privateKey, PublicKey, template, out privateEcdsaKey);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// TODO: Change according with identity class 
        /// </summary>
        /// <param name="privateKey">Private key of parents certificate</param>
        /// <param name="publicKey">Public key of certificate</param>
        /// <param name="template"></param>
        /// <returns></returns>
        public FullIdentity SignCertificate(X509Certificate2 issuer, byte[] privateKey, byte[] publicKey, CertificateTemplate template, out byte[]? privateEcdsaKey)
        {
            ArgumentNullException.ThrowIfNull(issuer);
            ArgumentNullException.ThrowIfNull(privateKey);
            ArgumentNullException.ThrowIfNull(publicKey);
            ArgumentNullException.ThrowIfNull(template);
            try
            {
                var ecdsa = ECDsa.Create();

                ICertificateTemplate builder = new CertificateBuilder(template);
                //ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
                //ecdsa.ImportPkcs8PrivateKey(privateKey, out _);
                privateEcdsaKey = ecdsa.ExportPkcs8PrivateKey();
                var certificateRequest = builder.GetRequest(ecdsa, HashAlgorithmName.SHA256);
                var signatureGenerator = builder.GetX509SignatureGenerator(ecdsa);
                var issuerName = builder.GetIssuerName();
                var serial = builder.GetSerialNumber();
                Console.WriteLine(serial.First());
                //var certificate = certificateRequest.Create(issuerName, signatureGenerator, _certificateSettings.date, _certificateSettings.date, serial);
                //var certificate = certificateRequest.Create(issuerName, signatureGenerator, DateTime.Now, DateTime.Now.AddDays(100), serial);
                var certificate = certificateRequest.Create(issuer, DateTime.Now, DateTime.Now.AddDays(50), new byte[] { 0 });

                FullIdentity identity = new FullIdentity();
                identity.Leaf = certificate;
                identity.Cert = issuer;
                identity.PrivateKey = ecdsa;
                return identity;
            }
            catch
            {
                throw;
            }
        }


        public override X509Certificate2 SignCertificate(X509Certificate2 issuer, byte[] privateKey, byte[] publicKey, X509Certificate2 template, out byte[] NewPrivateKey)
        {
            ArgumentNullException.ThrowIfNull(issuer);
            ArgumentNullException.ThrowIfNull(privateKey);
            ArgumentNullException.ThrowIfNull(publicKey);
            ArgumentNullException.ThrowIfNull(template);
            try
            {
                using (var ecdsa = ECDsa.Create())
                {
                    ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
                    ecdsa.ImportPkcs8PrivateKey(privateKey, out _);
                    NewPrivateKey = ecdsa.ExportPkcs8PrivateKey();
                    var certificateRequest = new CertificateRequest(template.SubjectName, ecdsa, HashAlgorithmName.SHA256);
                    return certificateRequest.Create(issuer, new DateTime(0001, 01, 01, 00, 50, 00), new DateTime(0001, 01, 01, 00, 50, 00), template.SerialNumberBytes.ToArray());
                }
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="PrivateKey"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public X509Certificate2 CreateSelfSignedCertificate(ECDsa PrivateKey, CertificateTemplate template)
        {
            ArgumentNullException.ThrowIfNull(PrivateKey);
            ArgumentNullException.ThrowIfNull(template);
            try
            {
                if (!CheckOIDTemplate(template))
                {
                    AddSubjectKeyIdentifierExtension(template, PrivateKey.ExportSubjectPublicKeyInfo());
                }
                ICertificateTemplate builder = new CertificateBuilder(template);
                var certificateRequest = builder.GetRequest(PrivateKey, HashAlgorithmName.SHA256);
                var signatureGenerator = builder.GetX509SignatureGenerator(PrivateKey);
                var issuerName = builder.GetIssuerName();
                var serial = builder.GetSerialNumber();
                //var certificate = certificateRequest.Create(issuerName, signatureGenerator, _certificateSettings.date, _certificateSettings.date, serial);
                //var certificate = certificateRequest.Create(issuerName, signatureGenerator, DateTimeOffset.Now, DateTimeOffset.Now.AddDays(100), serial);
                return certificateRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(100));
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Here we check subjectKeyId extension for CA
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public bool CheckOIDTemplate(CertificateTemplate template)
        {
            ArgumentNullException.ThrowIfNull(template);
            try
            {
                foreach (var i in template.X509Extensions)
                {
                    if (i.Oid != null && i.Oid.Value == "2.5.29.14")
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        public void AddSubjectKeyIdentifierExtension(CertificateTemplate template, byte[] PublicKey)
        {
            ArgumentNullException.ThrowIfNull(template);
            ArgumentNullException.ThrowIfNull(PublicKey);
            try
            {
                byte[] subjectKeyIdentifier = GenerateSubjectKeyIdentifier(PublicKey);
                X509SubjectKeyIdentifierExtension extension = new X509SubjectKeyIdentifierExtension(subjectKeyIdentifier, critical: false);
                template.X509Extensions.Add(extension);
            }
            catch
            {
                throw;
            }
        }

        public byte[] GenerateSubjectKeyIdentifier(byte[] PublicKey)
        {
            ArgumentNullException.ThrowIfNull(PublicKey);
            try
            {
                using (SHA1 sha1 = SHA1.Create())
                {
                    return sha1.ComputeHash(PublicKey);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}

