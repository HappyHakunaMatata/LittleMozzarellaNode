using System;
using System.Security.Cryptography.X509Certificates;

namespace gRPCCertificateSign.Models
{
	public class AmazonCertificates
	{

		public readonly X509Certificate2 ACMIssuedCertificate;
		public readonly X509Certificate2Collection CertificateChain;

        public AmazonCertificates(X509Certificate2 ACMIssuedCertificate, X509Certificate2Collection CertificateChain)
		{
			this.ACMIssuedCertificate = ACMIssuedCertificate;
			this.CertificateChain = CertificateChain;
        }
	}
}

