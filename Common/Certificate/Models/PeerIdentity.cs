using System;
using System.Security.Cryptography.X509Certificates;

namespace Common.Certificate.Models
{
	public class PeerIdentity
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


        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach(var cert in RestChain)
                {
                    cert.Dispose();
                }
                Cert.Dispose();
                Leaf.Dispose();
            }

            _disposed = true;
        }

        public void Dipose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

