using System.Net;
using System.Security.Cryptography.X509Certificates;
using Node.Certificate.Models;
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

internal class ConcreteBouncyCastleCertificateBuilder : AbstractCertificateBuilder
{
    private readonly CertificateSettings _certificateSettings;
    private readonly X509V3CertificateGenerator certificateGenerator;
    private readonly CryptoApiRandomGenerator randomGenerator;
    private readonly SecureRandom secureRandom;

    private X509Certificate2? _certificate;



    public ConcreteBouncyCastleCertificateBuilder(CertificateSettings? certificateSettings = null)
    {
        // The Certificate Generator
        certificateGenerator = new X509V3CertificateGenerator();

        // Generating Random Numbers
        randomGenerator = new CryptoApiRandomGenerator();
        secureRandom = new SecureRandom(randomGenerator);

        if (certificateSettings == null)
        {
            _certificateSettings = new CertificateSettings();
        }
        else
        {
            _certificateSettings = certificateSettings;
        }
    }

    public X509Certificate2 Certificate2
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

    public override X509Certificate2 GetResult()
    {
        try
        {
            return Certificate2;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    private void GenerateCertificate(string subjectName, string issuerName, string host, DateTime validFrom, DateTime validTo)
    {
        CreateSubjectAlternativeName(host);
        SetSerialNumber();
        SetSubject(subjectName, issuerName, validFrom, validTo);
        var subjectKeyPair = SetSubjectPublicKey();
        var rparams = CreatePrivateKeyInfo(subjectKeyPair);
        var signatureFactory = SetCertificateIntendedPurposes(subjectKeyPair);
        var cert = SignCertificate(subjectKeyPair, signatureFactory);
        Certificate2 = SetPrivateKey(cert, rparams);
    }


    public override void CreateCertificate()
    {
        var cn = $"CN={_certificateSettings.RootCertificateName}";
        GenerateCertificate(cn, cn, _certificateSettings.Host, DateTime.UtcNow.AddDays(-_certificateSettings.CertificateGraceDays), DateTime.UtcNow.AddDays(_certificateSettings.CertificateValidDays));
    }

    /// <summary>
    /// Set serial number
    /// </summary>
    public void SetSerialNumber()
    {
        var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), secureRandom);
        certificateGenerator.SetSerialNumber(serialNumber);
    }

    /// <summary>
    /// Set Issuer and Subject Name
    /// </summary>
    public void SetSubject(string subjectName, string issuerName, DateTime validFrom, DateTime validTo)
    {
        var subjectDn = new X509Name(subjectName);
        var issuerDn = new X509Name(issuerName);
        certificateGenerator.SetIssuerDN(issuerDn);
        certificateGenerator.SetSubjectDN(subjectDn);
        certificateGenerator.SetNotBefore(validFrom);
        certificateGenerator.SetNotAfter(validTo);
    }

    /// <summary>
    /// Add subject alternative name
    /// </summary>
    public void CreateSubjectAlternativeName(string hostName)
    {
        var nameType = GeneralName.DnsName;
        if (IPAddress.TryParse(hostName, out _))
        {
            nameType = GeneralName.IPAddress;
        }
        var subjectAlternativeNames = new Asn1Encodable[] { new GeneralName(nameType, hostName) };
        var subjectAlternativeNamesExtension = new DerSequence(subjectAlternativeNames);
        certificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName.Id, false,
            subjectAlternativeNamesExtension);
    }

    /// <summary>
    /// Generate Key Pair
    /// </summary>
    public AsymmetricCipherKeyPair SetSubjectPublicKey()
    {
        var keyGenerationParameters = new KeyGenerationParameters(secureRandom, _certificateSettings.KeyStrength);
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);
        return keyPairGenerator.GenerateKeyPair();
    }


    /// <summary>
    /// Set certificate intended purposes to only Server Authentication
    /// Self-sign the certificate
    /// </summary>
    public Asn1SignatureFactory SetCertificateIntendedPurposes(AsymmetricCipherKeyPair subjectKeyPair, string signatureAlgorithm = "SHA256WithRSA")
    {
        certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage.Id, false, new ExtendedKeyUsage(KeyPurposeID.id_kp_serverAuth));
        return new Asn1SignatureFactory(signatureAlgorithm, subjectKeyPair.Private, secureRandom);
    }


    /// <summary>
    /// Set Public Key and Self-sign the certificate 
    /// </summary>
    /// <param name="subjectKeyPair"></param>
    /// <param name="signatureFactory"></param>
    public X509Certificate SignCertificate(AsymmetricCipherKeyPair subjectKeyPair, Asn1SignatureFactory signatureFactory)
    {
        certificateGenerator.SetPublicKey(subjectKeyPair.Public);
        return certificateGenerator.Generate(signatureFactory);
    }

    /// <summary>
    /// Corresponding private key
    /// </summary>
    public RsaPrivateCrtKeyParameters CreatePrivateKeyInfo(AsymmetricCipherKeyPair subjectKeyPair)
    {
        var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);
        var seq = (Asn1Sequence)Asn1Object.FromByteArray(privateKeyInfo.ParsePrivateKey().GetDerEncoded());
        if (seq.Count != 9)
        {
            throw new PemException("Malformed sequence in RSA private key");
        }
        var rsa = RsaPrivateKeyStructure.GetInstance(seq);
        return  new RsaPrivateCrtKeyParameters(rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent,
            rsa.Prime1, rsa.Prime2, rsa.Exponent1,
            rsa.Exponent2, rsa.Coefficient);
    }

    /// <summary>
    ///  Set private key onto certificate instance
    /// </summary>
    private X509Certificate2 SetPrivateKey(X509Certificate certificate, AsymmetricKeyParameter privateKey)
    {
        
        var builder = new Pkcs12StoreBuilder();
        var store = builder.Build();
        var entry = new X509CertificateEntry(certificate);
        store.SetCertificateEntry(certificate.SubjectDN.ToString(), entry);
        store.SetKeyEntry(certificate.SubjectDN.ToString(), new AsymmetricKeyEntry(privateKey), new[] { entry });
        using (var ms = new MemoryStream())
        {
            store.Save(ms, _certificateSettings.Pkcs12StorePassword.ToCharArray(), new SecureRandom(new CryptoApiRandomGenerator()));
            return new X509Certificate2(ms.ToArray(), _certificateSettings.Pkcs12StorePassword, X509KeyStorageFlags.Exportable);
        }
    }
}