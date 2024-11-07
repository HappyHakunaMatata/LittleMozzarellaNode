using System;
using System.Security.Cryptography.X509Certificates;

namespace Node.Certificate.Models
{
	public ref struct PeerCertificateAuthority
	{
		public readonly X509Certificate2[] RestChain;

        public readonly X509Certificate2 Cert;

		public readonly byte[] NodeID;

        public PeerCertificateAuthority(X509Certificate2[] RestChain, X509Certificate2 Cert, byte[] NodeID)
		{
			this.RestChain = RestChain;
			this.Cert = Cert;
			this.NodeID = NodeID;
		}
	}
}

