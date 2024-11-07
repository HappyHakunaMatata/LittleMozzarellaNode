using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace Common.Certificate.Models
{
	public class FullIdentity
	{
        public X509Certificate2[]? RestChain;

        public X509Certificate2? Cert;

        public byte[]? NodeID;

        public X509Certificate2? Leaf;

        public AsymmetricAlgorithm? PrivateKey;

        public FullIdentity(X509Certificate2[] RestChain, X509Certificate2 Cert, byte[] NodeID, X509Certificate2 Leaf, AsymmetricAlgorithm PrivateKey)
        {
            this.RestChain = RestChain;
            this.Cert = Cert;
            this.NodeID = NodeID;
            this.Leaf = Leaf;
            this.PrivateKey = PrivateKey;
        }

        public FullIdentity(PeerIdentity peerIdentity, AsymmetricAlgorithm PrivateKey)
        {
            this.RestChain = peerIdentity.RestChain;
            this.Cert = peerIdentity.Cert;
            this.NodeID = peerIdentity.NodeID;
            this.Leaf = peerIdentity.Leaf;
            this.PrivateKey = PrivateKey;
        }

        public FullIdentity()
        {

        }

        public X509Certificate2Collection Chain()
        {
            ArgumentNullException.ThrowIfNull(Leaf);
            ArgumentNullException.ThrowIfNull(Cert);
            try
            {
                var collection = new X509Certificate2Collection();
                //collection.Add(LoadLeathWithPrivateKey());
                collection.Add(this.Cert);
                if (this.RestChain != null)
                {
                    collection.AddRange(this.RestChain);
                }
                return collection;
            }
            catch
            {
                throw;
            }
        }

        public List<string> PemChain()
        {
            try
            {
                var collection = Chain();
                List<string> chain = new List<string>();
                chain.AddRange(collection.Select(value => value.ExportCertificatePem()));
                return chain;
            }
            catch
            {
                throw;
            }
        }

        public List<byte[]> RawChain()
        {
            try
            {
                var collection = Chain();
                List<byte[]> chain = new List<byte[]>();
                chain.AddRange(collection.Select(value => value.RawData));
                return chain;
            }
            catch
            {
                throw;
            }
        }

        public List<byte[]> RawRestChain()
        {
            ArgumentNullException.ThrowIfNull(RestChain);
            try
            {
                List<byte[]> chain = new List<byte[]>();
                chain.AddRange(RestChain.Select(value => value.RawData));
                return chain;
            }
            catch
            {
                throw;
            }
        }



        public string ClientHeaderCertificate()
        {
            ArgumentNullException.ThrowIfNull(Leaf);
            try
            {
                var certificatePem = Leaf.ExportCertificatePem();
                certificatePem = certificatePem.Replace("\n", "").Replace("-----BEGIN CERTIFICATE-----", ":").Replace("-----END CERTIFICATE-----", ":");
                return certificatePem;
            }
            catch
            {
                throw;
            }
        }

        public static string ClientHeaderCertificate(X509Certificate2 x509Certificate2)
        {
            ArgumentNullException.ThrowIfNull(x509Certificate2);
            try
            {
                var certificatePem = x509Certificate2.ExportCertificatePem();
                certificatePem = certificatePem.Replace("\n", "").Replace("-----BEGIN CERTIFICATE-----", ":").Replace("-----END CERTIFICATE-----", ":");
                return certificatePem;
            }
            catch
            {
                throw;
            }
        }

        public string ClientHeaderCertificateChain()
        {
            ArgumentNullException.ThrowIfNull(RestChain);
            ArgumentNullException.ThrowIfNull(Cert);
            try
            {
                string chain = $"{ClientHeaderCertificate(Cert)}, ";
                foreach (var cert in RestChain)
                {
                    var certificatePem = cert.ExportCertificatePem();
                    certificatePem = certificatePem.Replace("\n", "").Replace("-----BEGIN CERTIFICATE-----", ":").Replace("-----END CERTIFICATE-----", ":, ");
                    chain += certificatePem;
                }
                chain = chain.TrimEnd(new char[] { ' ', ',' });
                return chain;
            }
            catch
            {
                throw;
            }
        }




        
        public void SaveFullIdentity(string path)
        {
            try
            {
                var filename = $"identity.cert";
                Save(path, filename);
            }
            catch
            {
                throw;
            }
        }

        public void SaveBackUpFullIdentity(string path)
        {
            try
            {
                var filename = $"identity.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.cert";
                Save(path, filename);
            }
            catch
            {
                throw;
            }
        }

        private void Save(string path, string filename)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var task = Task.Run(async () => await SaveKey(path));
                string certificatePath = Path.Combine(path, filename);
                if (Leaf != null)
                {
                    File.WriteAllText(certificatePath, Leaf.ExportCertificatePem() + "\n");
                }
                if (Cert != null)
                {
                    File.AppendAllText(certificatePath, Cert.ExportCertificatePem() + "\n");
                }
                if (RestChain != null)
                {
                    foreach (var cert in RestChain)
                    {
                        File.AppendAllText(certificatePath, cert.ExportCertificatePem() + "\n");
                    }
                }
                task.Wait();
            }
            catch
            {
                throw;
            }
        }

        private async Task SaveKey(string path)
        {
            try
            {
                string certificatePath = Path.Combine(path, "identity.key");
                if (PrivateKey != null)
                {
                    await File.WriteAllTextAsync(certificatePath, PrivateKey.ExportPkcs8PrivateKeyPem()+"\n");
                }
            }
            catch
            {
                throw;
            }
        }

        public X509Certificate2 LoadLeathWithPrivateKey()
        {
            ArgumentNullException.ThrowIfNull(Leaf);
            ArgumentNullException.ThrowIfNull(PrivateKey);
            try
            {
                var leafWithKey = Leaf.CopyWithPrivateKey((ECDsa)PrivateKey);
                return leafWithKey;
            }
            catch
            {
                throw;
            }
        }

        public byte[] GetCACSR(AsymmetricAlgorithm key)
        {
            ArgumentNullException.ThrowIfNull(Cert);
            ArgumentNullException.ThrowIfNull(key);
            try
            {
                var csr = new CertificateRequest(Cert.SubjectName, (ECDsa)key, HashAlgorithmName.SHA256);
                return Encoding.UTF8.GetBytes(csr.CreateSigningRequestPem());
            }
            catch
            {
                throw;
            }
        }

        public bool AreEqual(FullIdentity identity)
        {

            if (!X509Certificate2.Equals(identity.Cert, this.Cert))
            {
                
                return false;
            }
            if (!X509Certificate2.Equals(identity.Leaf, this.Leaf))
            {
                
                return false;
            }
            
            if (!Array.Equals(identity.NodeID, this.NodeID))
            {
                
                return false;
            }

            if (!AsymmetricAlgorithm.Equals(identity.PrivateKey, this.PrivateKey))
            {
                
                return false;
            }


            if (!Array.Equals(identity.RestChain, this.RestChain))
            {
                
                return false;
            }
            
            return true;
        }
    }
}

