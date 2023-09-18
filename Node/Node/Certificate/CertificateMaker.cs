using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace Node.Certificate;

internal class CertificateMaker : ICertificate
{
    private const int CertificateGraceDays = 366;
    private static bool _doNotSetFriendlyName;
    private readonly int certificateValidDays;

    internal CertificateMaker()
    {
        
    }

    private X509Certificate2 GenerateCertificate(string? hostName,
        string subjectName,
        string issuerName, DateTime validFrom,
        DateTime validTo, int keyStrength = 2048,
        string signatureAlgorithm = "SHA256WithRSA",
        AsymmetricKeyParameter? issuerPrivateKey = null)
    {
        // Generating Random Numbers
        var randomGenerator = new CryptoApiRandomGenerator();
        var secureRandom = new SecureRandom(randomGenerator);

        // The Certificate Generator
        var certificateGenerator = new X509V3CertificateGenerator();

        // Serial Number
        var serialNumber =
            BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), secureRandom);
        certificateGenerator.SetSerialNumber(serialNumber);

        // Issuer and Subject Name
        var subjectDn = new X509Name(subjectName);
        var issuerDn = new X509Name(issuerName);
        certificateGenerator.SetIssuerDN(issuerDn);
        certificateGenerator.SetSubjectDN(subjectDn);

        certificateGenerator.SetNotBefore(validFrom);
        certificateGenerator.SetNotAfter(validTo);

        if (hostName != null)
        {
            // add subject alternative names
            var nameType = GeneralName.DnsName;
            if (IPAddress.TryParse(hostName, out _)) nameType = GeneralName.IPAddress;

            var subjectAlternativeNames = new Asn1Encodable[] { new GeneralName(nameType, hostName) };

            var subjectAlternativeNamesExtension = new DerSequence(subjectAlternativeNames);
            certificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName.Id, false,
                subjectAlternativeNamesExtension);
        }

        // Subject Public Key
        var keyGenerationParameters = new KeyGenerationParameters(secureRandom, keyStrength);
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);
        var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

        certificateGenerator.SetPublicKey(subjectKeyPair.Public);

        // Set certificate intended purposes to only Server Authentication
        certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage.Id, false,
            new ExtendedKeyUsage(KeyPurposeID.id_kp_serverAuth));
        if (issuerPrivateKey == null)
            certificateGenerator.AddExtension(X509Extensions.BasicConstraints.Id, true, new BasicConstraints(true));

        var signatureFactory = new Asn1SignatureFactory(signatureAlgorithm,
            issuerPrivateKey ?? subjectKeyPair.Private, secureRandom);

        // Self-sign the certificate
        var certificate = certificateGenerator.Generate(signatureFactory);

        // Corresponding private key
        var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);

        var seq = (Asn1Sequence)Asn1Object.FromByteArray(privateKeyInfo.ParsePrivateKey().GetDerEncoded());

        if (seq.Count != 9) throw new PemException("Malformed sequence in RSA private key");

        var rsa = RsaPrivateKeyStructure.GetInstance(seq);
        var rsaparams = new RsaPrivateCrtKeyParameters(rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent,
            rsa.Prime1, rsa.Prime2, rsa.Exponent1,
            rsa.Exponent2, rsa.Coefficient);

        // Set private key onto certificate instance
        var x509Certificate = WithPrivateKey(certificate, rsaparams);

        if (!_doNotSetFriendlyName)
            try
            {
                x509Certificate.FriendlyName = CnRemoverRegex.Replace(subjectName, string.Empty);
            }
            catch (PlatformNotSupportedException)
            {
                _doNotSetFriendlyName = true;
            }

        return x509Certificate;
    }

    internal readonly Regex CnRemoverRegex =
        new(@"^CN\s*=\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private X509Certificate2 WithPrivateKey(X509Certificate certificate, AsymmetricKeyParameter privateKey)
    {
        const string password = "password";
        var builder = new Pkcs12StoreBuilder();
        var store = builder.Build();
        var entry = new X509CertificateEntry(certificate);
        store.SetCertificateEntry(certificate.SubjectDN.ToString(), entry);
        store.SetKeyEntry(certificate.SubjectDN.ToString(), new AsymmetricKeyEntry(privateKey), new[] { entry });
        using (var ms = new MemoryStream())
        {
            store.Save(ms, password.ToCharArray(), new SecureRandom(new CryptoApiRandomGenerator()));
            return new X509Certificate2(ms.ToArray(), password, X509KeyStorageFlags.Exportable);
        }
    }
    
    private X509Certificate2 MakeCertificateInternal(string host, string subjectName,
        DateTime validFrom, DateTime validTo, X509Certificate2? signingCertificate)
    {
        if (signingCertificate == null)
        {
            return GenerateCertificate(host, subjectName, subjectName, validFrom, validTo);
        }
        var kp = DotNetUtilities.GetKeyPair(signingCertificate.GetRSAPrivateKey());
        return GenerateCertificate(host, subjectName, signingCertificate.Subject, validFrom, validTo,
            issuerPrivateKey: kp.Private);
    }

    public X509Certificate2 MakeCertificate(string subject, string? host = null,
        X509Certificate2? signingCert = null)
    {
        string HostName = subject;
        if (host != null)
        {
            HostName = host;
        }
        return MakeCertificateInternal(HostName, $"CN={subject}",
            DateTime.UtcNow.AddDays(-CertificateGraceDays), DateTime.UtcNow.AddDays(certificateValidDays),
            signingCert);
    }
}