using System;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Common.Certificate.Interfaces;

namespace Common.Certificate.Models
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
            ArgumentNullException.ThrowIfNull(key);
            try
            {
                return this.certificateTemplate.GetCertificateRequest(key, signatureAlgorithm);
            }
            catch
            {
                throw;
            }
		}

        public byte[] GetSerialNumber()
        {
            try
            {
                return this.certificateTemplate.GetSerialNumber();
            }
            catch
            {
                throw;
            }
        }

        public X500DistinguishedName GetIssuerName()
        {
            try
            {
                return this.certificateTemplate.DistinguishedName;
            }
            catch
            {
                throw;
            }
        }

        public X509SignatureGenerator GetX509SignatureGenerator(ECDsa key)
        {
            ArgumentNullException.ThrowIfNull(key);
            try
            {
                return X509SignatureGenerator.CreateForECDsa(key);
            }
            catch
            {
                throw;
            }
        }
    }
}

