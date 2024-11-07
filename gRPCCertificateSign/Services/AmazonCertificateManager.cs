using System.Security.Cryptography.X509Certificates;
using Amazon;
using Amazon.ACMPCA;
using Node.Certificate;
using Node.Certificate.Models;
using Amazon.ACMPCA.Model;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace gRPCCertificateSign.Services
{
	public class AmazonCertificateManager
	{
		private readonly AmazonACMPCAClient _client;
        private readonly string _arn;


        public AmazonCertificateManager(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region, string Arn)
		{
            _arn = Arn;
            _client = new AmazonACMPCAClient(awsAccessKeyId, awsSecretAccessKey, region);
        }


        public async Task<FullIdentity> SignIdentity(FullIdentity identity, FullCertificateAuthority ca)
        {
            var publicCACertificate = await LoadCAPublicKey();
            var signedCertificate = await RequestSign(identity.GetCACSR(ca.algorithm));
            if (publicCACertificate == null || signedCertificate == null)
            {
                return identity;
            }
            identity.Cert = signedCertificate;
            identity.RestChain = new X509Certificate2Collection(publicCACertificate).ToArray();
            return identity;
        }


		private async Task<X509Certificate2?> LoadCAPublicKey()
		{
            var request = new GetCertificateAuthorityCertificateRequest
            {
                CertificateAuthorityArn = _arn
            };
            var response = await _client.GetCertificateAuthorityCertificateAsync(request);
            
            if (response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }
            return X509Certificate2.CreateFromPem(response.Certificate);
        }

        

        private async Task<X509Certificate2?> RequestSign(byte[] csr)
        {
            var issueCertificateRequest = new IssueCertificateRequest
            {
                CertificateAuthorityArn = _arn,
                Csr = new MemoryStream(csr),
                SigningAlgorithm = SigningAlgorithm.SHA256WITHECDSA,
                Validity = new Validity
                {
                    Type = ValidityPeriodType.DAYS,
                    Value = 365
                }
            };
            var issueCertificateResponse = await _client.IssueCertificateAsync(issueCertificateRequest);
            if (issueCertificateResponse == null || issueCertificateResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }
            var request = new GetCertificateRequest()
            {
                CertificateArn = issueCertificateResponse.CertificateArn,
                CertificateAuthorityArn = _arn,
            };
            //TODO: FIX THIS SLEEP
            Thread.Sleep(20000);
            var response = await _client.GetCertificateAsync(request);
            if (response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }
            
            return X509Certificate2.CreateFromPem(response.Certificate);
        }

        public async Task<FullIdentity> Test(FullIdentity identity)
        {
            var publicCACertificate = await LoadCAPublicKey();
            var request = new GetCertificateRequest()
            {
                CertificateArn = "arn:aws:acm-pca:eu-north-1:134802823426:certificate-authority/07204130-a09e-42b4-8f26-c64a95dfd5e1/certificate/894a751dd67bbae17364869311a04d27",
                CertificateAuthorityArn = _arn,
            };
            
            var response = await _client.GetCertificateAsync(request);

            Console.WriteLine("Certificates");
            Console.WriteLine(response.Certificate);
            Console.WriteLine(response.CertificateChain);
            Console.WriteLine("Certificates");
            if (publicCACertificate == null || response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return identity;
            }

            identity.Cert = X509Certificate2.CreateFromPem(response.Certificate);
            identity.RestChain = new X509Certificate2Collection(publicCACertificate).ToArray();
            return identity;
        }

        public async Task ImportCertificate(X509Certificate2 chain, X509Certificate2 x509Certificate)
        {
            Console.WriteLine($"Chain: {chain.ExportCertificatePem()}");
            Console.WriteLine($"Cert: {x509Certificate.ExportCertificatePem()}");
            var request = new ImportCertificateAuthorityCertificateRequest()
            {
                CertificateAuthorityArn = _arn,
                CertificateChain = new MemoryStream(Encoding.UTF8.GetBytes(chain.ExportCertificatePem())),
                Certificate = new MemoryStream(Encoding.UTF8.GetBytes(x509Certificate.ExportCertificatePem()))
            };
            var response = await _client.ImportCertificateAuthorityCertificateAsync(request);
            Console.WriteLine($"Lenght: {response.ContentLength}");
            Console.WriteLine($"Import: {response.HttpStatusCode}");
        }
	}
}

