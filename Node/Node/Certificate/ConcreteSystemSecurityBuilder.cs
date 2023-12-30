using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Node.Certificate.Models;
using Node.Certificate.Template;

///TODO: MAKE TEST FOR CreateCertificate() 
/// Generated 216_290_839 keys; best difficulty so far: 36

namespace Node.Certificate
{
    public class ConcreteSystemSecurityBuilder : AbstractCertificateBuilder
    {
        
        private X509Certificate2? _certificate;


        public ConcreteSystemSecurityBuilder(CertificateAuthorityConfig certificateAuthority):base(certificateAuthority)
        {
        }


        public ConcreteSystemSecurityBuilder(CertificateSettings certificateSettings):base(certificateSettings)
        {
        }

        public ConcreteSystemSecurityBuilder(CertificateAuthorityConfig certificateAuthority, CertificateSettings certificateSettings): base(certificateAuthority, certificateSettings)
        {
        }

        public ConcreteSystemSecurityBuilder()
        {
        }

        private X509Certificate2 Certificate2
        {
            get
            {
                if (_certificate != null)
                {
                    return _certificate;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _certificate = value;
            }
        }


        public override X509Certificate2 GetCertificate()
        {
            try
            {
                return Certificate2;
            }
            catch
            {
                throw;
            }
        }


        public AsymmetricAlgorithm GeneratePrivateKey(AsymmetricAlgorithmName name = AsymmetricAlgorithmName.ECDSA)
        {
            if (name == AsymmetricAlgorithmName.RSA)
            {
                return GeneratePrivateRSAKey();
            }
            return GeneratePrivateECDSAKey();
        }


        public string GetBase64(CertificateRequest request)
        {
            var SigningRequest = request.CreateSigningRequestPem();
            string base64 = SigningRequest;
            byte[] dataBytes = Encoding.UTF8.GetBytes(base64);
            var base64Data = Convert.ToBase64String(dataBytes);
            return base64Data;
        }


        public RSA GeneratePrivateRSAKey()
        {
            var RSAKey = RSA.Create(_certificateSettings.KeyStrength);
            try
            {
                return RSAKey;
            }
            finally
            {
                RSAKey.Dispose();
            }
        }


        public CertificateTemplate CreateCertificateTemplate()
        {
            try
            {
                var serialNumber = newSerialNumber();
                var builder = new X500DistinguishedNameBuilder();
                builder.AddOrganizationName(_certificateSettings.OrganizationName);
                builder.AddCommonName(_certificateSettings.RootCertificateName);
                var name = builder.Build();
                var sanBuilder = new SubjectAlternativeNameBuilder();
                sanBuilder.AddDnsName(_certificateSettings.Host);
                var alternativeName = sanBuilder.Build();
                var basics = new X509BasicConstraintsExtension(_certificateSettings.IsCA, true, 0, _certificateSettings.BasicConstraintsValid);
                var asn = new AsnEncodedData(new Oid("2.999.2.1"), new byte[] {0}); //TODO add ID VERSION
                var VersionExtension = new X509Extension(asn, false);

                List<X509Extension> x509Extensions = new()
                {
                    alternativeName,
                    new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true),
                    basics,
                    VersionExtension
                };
                return new CertificateTemplate(serialNumber.ToByteArray(), name, x509Extensions);
            }
            catch
            {
                throw;
            }
        }


        public override CertificateTemplate CreateLeafTemplate()
        {
            try
            {
                var serialNumber = newSerialNumber();
                var builder = new X500DistinguishedNameBuilder();
                builder.AddOrganizationName(_certificateSettings.OrganizationName);
                builder.AddCommonName(_certificateSettings.RootCertificateName);
                var name = builder.Build();
                var sanBuilder = new SubjectAlternativeNameBuilder();
                sanBuilder.AddDnsName(_certificateSettings.Host);
                var alternativeName = sanBuilder.Build();


                OidCollection oc = new OidCollection();
                oc.Add(new Oid("1.3.6.1.5.5.7.3.1")); //ExtKeyUsageServerAuth
                oc.Add(new Oid("1.3.6.1.5.5.7.3.2")); //ExtKeyUsageClientAuth
                var KeyUsageExtension = new X509EnhancedKeyUsageExtension(oc, false);
                var basics = new X509BasicConstraintsExtension(_certificateSettings.IsCA, true, 0, _certificateSettings.BasicConstraintsValid);


                List<X509Extension> x509Extensions = new()
                {
                    alternativeName,
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature|X509KeyUsageFlags.KeyEncipherment, true),
                    basics,
                    KeyUsageExtension,
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
            return request.CreateSelfSigned(validFrom, validTo);
        }


        public override void SaveKey(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (TrySaveEcdsaKey(key))
            {
                return;
            }
            TrySaveRSAKey(key);
        }


        public bool TrySaveRSAKey(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);
            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportRSAPrivateKey(key, out _);
                    var pem = rsa.ExportPkcs8PrivateKeyPem();
                    File.WriteAllText(_certificateSettings.KeyPath, pem);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool TrySaveEcdsaKey(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);
            try
            {
                using (ECDsa ecdsa = ECDsa.Create())
                {
                    ecdsa.ImportPkcs8PrivateKey(key, out _);
                    var pem = ecdsa.ExportPkcs8PrivateKeyPem();
                    File.WriteAllText(_certificateSettings.KeyPath, pem);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }



        public ECDsa GeneratePrivateECDSAKey()
        {
            ECCurve curve = ECCurve.NamedCurves.nistP256;
            var key = ECDsa.Create(curve);
            return key;
        }


        public byte[] HashAndSign(ECDsa key, byte[] data)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var digest = sha256Hash.ComputeHash(data);
                var signature = SignWithoutHashing(key, digest);
                return signature;
            }
        }

        
        public bool HashAndVerifySignature(byte[] key, byte[] data, byte[] signature)
        {
            var sha256Hash = SHA256.Create();
            try
            {
                var digest = sha256Hash.ComputeHash(data);
                return VerifySignatureWithoutHashing(key, digest, signature);
            }
            finally
            {
                sha256Hash.Dispose();
            }
        }

        

        public override bool VerifyECDSASignatureWithoutHashing(byte[] key, byte[] data, byte[] signature)
        {
            var eCDsa = ECDsa.Create();
            try
            {
                eCDsa.ImportSubjectPublicKeyInfo(key, out _);
                return eCDsa.VerifyHash(data, signature);
            }
            finally
            {
                eCDsa.Dispose();
            }
        }


        public byte[] SignWithoutHashing(ECDsa privKey, byte[] digest)
        {
            return SignECDSAWithoutHashing(privKey, digest);
        }

        public byte[] SignECDSAWithoutHashing(ECDsa privKey, byte[] digest)
        {
            var key = privKey.SignHash(digest);
            return key;
        }

        
        public byte[]? GetPublicKeyFromPrivate(ref ECDsa ecdsa, out byte[]? pkey)
        {
            pkey = ecdsa.ExportPkcs8PrivateKey();
            return ecdsa.ExportSubjectPublicKeyInfo();
        }

        public byte[] SignRSAWithoutHashing(RSA privKey, byte[] digest)
        {
            var key = privKey.SignHash(digest, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            return key;
        }

        
        public override GeneratedKeys GenerateKey(CancellationToken ctx, UInt16 minDifficulty = 8)
        {
            UInt16 d;
            byte[]? id = null;
            byte[]? pkey = null;
            //ECDsa? key = null;
            //try
            //{
                while (true)
                {
                    using (ECDsa ecdsa = GeneratePrivateECDSAKey())
                    {
                        var ecdsaCopy = ecdsa;
                        var pubKey = GetPublicKeyFromPrivate(ref ecdsaCopy, out pkey);
                        try
                        {
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
                        catch
                        {
                            break;
                        }
                        finally
                        {
                            ecdsa.Dispose();
                        }
                    }
                }
                /*if (key != null)
                {
                    return new Tuple<byte[]?, byte[]?, byte[]?>(key.ExportSubjectPublicKeyInfo(), id, key.ExportPkcs8PrivateKey());
                }*/
                return new GeneratedKeys(null, null, null);
            //}
            /*finally
            {
                if (key != null)
                {
                    key.Dispose();
                }
            }*/
        }


        public override X509Certificate2 CreateSelfSignedCertificate(byte[] PrivateKey, byte[] PublicKey)
        {
            ArgumentNullException.ThrowIfNull(PrivateKey);
            ArgumentNullException.ThrowIfNull(PublicKey);
            var key = ECDsa.Create();
            try
            {
                key.ImportSubjectPublicKeyInfo(PublicKey, out _);
                key.ImportPkcs8PrivateKey(PrivateKey, out _);
                var template = CreateCertificateTemplate();
                return CreateCertificate(key, template);
            }
            finally
            {
                key.Dispose();
            }
        }


        public override byte[]? DoubleSHA256PublicKey(byte[] publickey)
        {
            byte[]? end;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var mid = sha256Hash.ComputeHash(publickey);
                end = sha256Hash.ComputeHash(mid);
            }
            return end;
        }


        public override X509Certificate2Collection NewIdentity(X509Certificate2 Certificate, byte[] privateKey, CertificateTemplate template, out byte[]? privateEcdsaKey)
        {

            using (AsymmetricAlgorithm asymmetricAlgorithm = GeneratePrivateKey())
            {
                var ecdsa = (ECDsa)asymmetricAlgorithm;
                var PublicKey = GetPublicKeyFromPrivate(ref ecdsa, out privateEcdsaKey);
                ecdsa.Dispose();
                return SignCertificate(Certificate, privateKey, PublicKey, template);
            }
        }

        /// <summary>
        /// TODO: Change according with identity class 
        /// </summary>
        /// <param name="privateKey">Private key of parents certificate</param>
        /// <param name="publicKey">Public key of certificate</param>
        /// <param name="template"></param>
        /// <returns></returns>
        public X509Certificate2Collection SignCertificate(X509Certificate2 issuer, byte[] privateKey, byte[] publicKey, CertificateTemplate template)
        {
            using (var ecdsa = ECDsa.Create())
            {
                ICertificateTemplate builder = new CertificateBuilder(template);
                ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
                ecdsa.ImportPkcs8PrivateKey(privateKey, out _);
                var certificateRequest = builder.GetRequest(ecdsa, HashAlgorithmName.SHA256);
                var signatureGenerator = builder.GetX509SignatureGenerator(ecdsa);
                var issuerName = builder.GetIssuerName();
                var serial = builder.GetSerialNumber();
                var certificate = certificateRequest.Create(issuerName, signatureGenerator, _certificateSettings.date, _certificateSettings.date, serial);
                X509Certificate2Collection certificateCollection = new X509Certificate2Collection();
                certificateCollection.Add(certificate);
                certificateCollection.Add(issuer);
                return certificateCollection;
            }
        }


        public X509Certificate2 SignCertificate(X509Certificate2 issuer, byte[] privateKey, byte[] publicKey, X509Certificate2 template)
        {
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
                ecdsa.ImportPkcs8PrivateKey(privateKey, out _);
                var certificateRequest = new CertificateRequest(template.SubjectName, ecdsa, HashAlgorithmName.SHA256);
                return certificateRequest.Create(issuer, _certificateSettings.date, _certificateSettings.date, template.SerialNumberBytes.ToArray());
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="PrivateKey"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public X509Certificate2 CreateCertificate(ECDsa PrivateKey, CertificateTemplate template)
        {
            if (!CheckOIDTemplate(template))
            {
                AddExtension(template, PrivateKey.ExportSubjectPublicKeyInfo());
            }
            ICertificateTemplate builder = new CertificateBuilder(template);
            var certificateRequest = builder.GetRequest(PrivateKey, HashAlgorithmName.SHA256);
            var signatureGenerator = builder.GetX509SignatureGenerator(PrivateKey);
            var issuerName = builder.GetIssuerName();
            var serial = builder.GetSerialNumber();
            var certificate = certificateRequest.Create(issuerName, signatureGenerator, _certificateSettings.date, _certificateSettings.date, serial);
            this.Certificate2 = certificate;
            return certificate;
        }


        /// <summary>
        /// Here we check subjectKeyId extension for CA
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public bool CheckOIDTemplate(CertificateTemplate template)
        {
            foreach (var i in template.X509Extensions)
            {
                //Check len?
                if(i.Oid != null && i.Oid.Value == "2.5.29.14")
                {
                    return true;
                }
            }
            return false;
        }

        public void AddExtension(CertificateTemplate template, byte[] PublicKey)
        {
            byte[] subjectKeyIdentifier = GenerateSubjectKeyIdentifier(PublicKey);
            X509SubjectKeyIdentifierExtension skiExtension = new X509SubjectKeyIdentifierExtension(subjectKeyIdentifier, critical: false);
            template.X509Extensions.Add(skiExtension);
        }

        public byte[] GenerateSubjectKeyIdentifier(byte[] PublicKey)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(PublicKey);
            }
        }

    }
}

