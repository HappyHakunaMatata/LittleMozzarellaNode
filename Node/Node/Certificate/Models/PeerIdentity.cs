using System;
using System.Security.Cryptography.X509Certificates;

namespace Node.Certificate.Models
{
	public struct PeerIdentity
	{

        public readonly X509Certificate2[] RestChain;

        public readonly X509Certificate2 Cert;

        public readonly byte[] NodeID;

        public readonly X509Certificate2 Leaf;

        public PeerIdentity(X509Certificate2[] RestChain, X509Certificate2 Cert, byte[] NodeID, X509Certificate2 Leaf)
		{
            this.RestChain = RestChain;
            this.Cert = Cert;
            this.NodeID = NodeID;
            this.Leaf = Leaf;
        }
	}
}

