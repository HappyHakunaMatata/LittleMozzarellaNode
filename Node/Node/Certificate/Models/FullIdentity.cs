using System;
using System.Security.Cryptography.X509Certificates;

namespace Node.Certificate.Models
{
	public struct FullIdentity
	{
        public readonly X509Certificate2[] RestChain;

        public readonly X509Certificate2 Cert;

        public readonly byte[] NodeID;

        public readonly X509Certificate2 Leaf;

        public readonly byte[] PrivateKey;

        public FullIdentity(X509Certificate2[] RestChain, X509Certificate2 Cert, byte[] NodeID, X509Certificate2 Leaf, byte[] PrivateKey)
        {
            this.RestChain = RestChain;
            this.Cert = Cert;
            this.NodeID = NodeID;
            this.Leaf = Leaf;
            this.PrivateKey = PrivateKey;
        }

        public FullIdentity(PeerIdentity peerIdentity, byte[] PrivateKey)
        {
            this.RestChain = peerIdentity.RestChain;
            this.Cert = peerIdentity.Cert;
            this.NodeID = peerIdentity.NodeID;
            this.Leaf = peerIdentity.Leaf;
            this.PrivateKey = PrivateKey;
        }
    }
}

