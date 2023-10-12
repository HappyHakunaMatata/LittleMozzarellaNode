using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Amazon.Runtime.Internal;
using Microsoft.Extensions.Hosting;
using Node.Certificate.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;

namespace Node.Certificate
{
    public class ConcreteSystemSecurityBuilder : AbstractCertificateBuilder
    {
        private readonly CertificateSettings _certificateSettings;
        private X509Certificate2? _certificate;


        public ConcreteSystemSecurityBuilder(CertificateSettings? certificateSettings = null)
        {
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


        private void GenerateCertificate(string subjectName, string host, DateTime validFrom, DateTime validTo)
        {
            var request = CreateCertificateRequest(subjectName, HashAlgorithmName.SHA256);
            SetSubject(request, host);
            CreateSelfSigned(request, validFrom, validTo);
        }


        public override void CreateCertificate()
        {
            var cn = $"CN={_certificateSettings.RootCertificateName}";
            GenerateCertificate(cn, _certificateSettings.Host, DateTime.UtcNow.AddDays(-_certificateSettings.CertificateGraceDays), DateTime.UtcNow.AddDays(_certificateSettings.CertificateValidDays));
        }


        public CertificateRequest CreateCertificateRequest(string subjectName, HashAlgorithmName signatureAlgorithm = default)
        {
            RSA rsaKey = RSA.Create();
            RSASignaturePadding padding = RSASignaturePadding.Pkcs1;
            return new CertificateRequest(
                subjectName,
                rsaKey,
                signatureAlgorithm,
                padding);
        }

        public void SetSubject(CertificateRequest request, string host)
        {
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName(host);
            request.CertificateExtensions.Add(sanBuilder.Build());
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        }

        public X509Certificate CreateSelfSigned(CertificateRequest request, DateTime validFrom, DateTime validTo)
        {
            return request.CreateSelfSigned(validFrom, validTo);
        }

    }
}

