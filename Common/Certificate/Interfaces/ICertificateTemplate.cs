using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Common.Certificate.Interfaces
{
	public interface ICertificateTemplate
    {

        public CertificateRequest GetRequest(ECDsa key, HashAlgorithmName signatureAlgorithm  = default);

        public byte[] GetSerialNumber();

        public X509SignatureGenerator GetX509SignatureGenerator(ECDsa key);

        public X500DistinguishedName GetIssuerName();
    }
}

