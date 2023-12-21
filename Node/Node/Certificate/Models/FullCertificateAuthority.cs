using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Node.Certificate.Models
{
	public class FullCertificateAuthority
	{
        public X509Certificate2[]? RestChain;

        public X509Certificate2? Cert;

        public byte[]? NodeID;

        public AsymmetricAlgorithm? algorithm;

        public byte[]? PrivateKey;

        public FullCertificateAuthority()
        {

        }

        public FullCertificateAuthority(X509Certificate2 Cert, byte[] NodeID, byte[] PrivateKey)
        {
            this.Cert = Cert;
            this.NodeID = NodeID;
            this.PrivateKey = PrivateKey;
        }

        public FullCertificateAuthority(X509Certificate2 Cert, byte[] NodeID, AsymmetricAlgorithm algorithm)
        {
            this.Cert = Cert;
            this.NodeID = NodeID;
            this.algorithm = algorithm;
        }

        public FullCertificateAuthority(X509Certificate2[] RestChain, X509Certificate2 Cert, byte[] NodeID, AsymmetricAlgorithm algorithm)
        {
            this.RestChain = RestChain;
            this.Cert = Cert;
            this.NodeID = NodeID;
            this.algorithm = algorithm;
        }

        public List<byte[]> RawRestChain()
        {
            ArgumentNullException.ThrowIfNull(RestChain);
            List<byte[]> chain = new List<byte[]>();
            foreach(var i in RestChain)
            {
                chain.Add(i.RawData);
            }
            return chain;
        }
    }
}

