using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Node.Certificate.Template;

namespace Node.Certificate.Models
{
	public class CertificateBuilder: ICertificateTemplate
    {

		public CertificateTemplate certificateTemplate;

        public CertificateBuilder(CertificateTemplate certificateTemplate)
		{
			this.certificateTemplate = certificateTemplate;


        }

		public CertificateRequest GetRequest(ECDsa key, HashAlgorithmName signatureAlgorithm = default)
		{
			return this.certificateTemplate.GetCertificateRequest(key, signatureAlgorithm);
		}

        public byte[] GetSerialNumber()
        {
            return this.certificateTemplate.GetSerialNumber();
        }

        public X500DistinguishedName GetIssuerName()
        {
            return this.certificateTemplate.DistinguishedName;
        }

        public X509SignatureGenerator GetX509SignatureGenerator(ECDsa key)
        {
            return X509SignatureGenerator.CreateForECDsa(key);
        }
    }
}

