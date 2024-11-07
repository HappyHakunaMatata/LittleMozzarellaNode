using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Common.Certificate.Models
{
	public class FullCertificateAuthority: FullCertificateAuthority<ECDsa>
    {

        public FullCertificateAuthority(X509Certificate2[] RestChain, X509Certificate2 Cert, byte[] NodeID, ECDsa privatekey)
        {
            this.RestChain = RestChain;
            this.Cert = Cert;
            this.NodeID = NodeID;
            this.PrivateKey = privatekey;
        }

        public FullCertificateAuthority(X509Certificate2 Cert, byte[] NodeID, byte[] privatekey)
        {
            this.RestChain = RestChain;
            this.Cert = Cert;
            this.NodeID = NodeID;
            SetPrivateKey(privatekey);
        }


        public void SetPrivateKey(byte[] privatekey)
		{
			try
			{
				this.PrivateKey = ECDsa.Create();
				PrivateKey.ImportPkcs8PrivateKey(privatekey, out _);
			}
			catch
			{
				throw;
			}
		}
	}
}

