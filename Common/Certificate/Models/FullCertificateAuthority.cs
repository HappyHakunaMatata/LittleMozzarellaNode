using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Common.Certificate.Models
{
	public class FullCertificateAuthority<GenericAsymmetricAlgorithm> where GenericAsymmetricAlgorithm : AsymmetricAlgorithm
    {
        private X509Certificate2[]? _restchain;
        private X509Certificate2? _cert;
        public byte[]? _NodeID;
        private GenericAsymmetricAlgorithm? _privateKey;
        
        

        public X509Certificate2[]? RestChain
        {
            get
            {
                return _restchain;
            }
            set
            {
                _restchain = value;
            }
        }

        public X509Certificate2 Cert
        {
            get
            {
                if (_cert != null)
                {
                    return _cert;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _cert = value;
            }
        }

        public GenericAsymmetricAlgorithm PrivateKey
        {
            get
            {
                if (_privateKey != null)
                {
                    return _privateKey;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _privateKey = value;
            }
        }


        public byte[] NodeID
        {
            get
            {
                if (_NodeID != null)
                {
                    return _NodeID;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _NodeID = value;
            }
        }


        public FullCertificateAuthority()
        {
        }

        public FullCertificateAuthority(X509Certificate2 Cert, byte[] NodeID, GenericAsymmetricAlgorithm privatekey)
        {
            this.Cert = Cert;
            this.NodeID = NodeID;
            this.PrivateKey = privatekey;
        }

        public FullCertificateAuthority(X509Certificate2[] RestChain, X509Certificate2 Cert, byte[] NodeID, GenericAsymmetricAlgorithm privatekey)
        {
            this.RestChain = RestChain;
            this.Cert = Cert;
            this.NodeID = NodeID;
            this.PrivateKey = privatekey;
        }


        public List<byte[]> RawRestChain()
        {
            try
            {
                List<byte[]> chain = new List<byte[]>();
                if (RestChain != null)
                {
                    foreach (var i in RestChain)
                    {
                        chain.Add(i.RawData);
                    }
                }
                return chain;
            }
            catch
            {
                throw;
            }
        }

        public void SaveFullCertificateAuthority(string path)
        {
            try
            {
                var filename = $"ca.cert";
                Save(path, filename);
            }
            catch
            {
                throw;
            }
        }

        public void SaveBackUpFullCertificateAuthority(string path)
        {
            try
            {
                var filename = $"ca.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.cert";
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

                if (Cert != null)
                {
                    File.WriteAllText(certificatePath, Cert.ExportCertificatePem() + "\n");
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
                string certificatePath = Path.Combine(path, "ca.key");
                if (PrivateKey != null)
                {
                    await File.WriteAllTextAsync(certificatePath, PrivateKey.ExportPkcs8PrivateKeyPem() + "\n");
                }
            }
            catch
            {
                throw;
            }
        }

        public void UpdateFullCertificateAuthority(FullIdentity identity)
        {
            ArgumentNullException.ThrowIfNull(identity);
            if (identity.Cert != null)
            {
                this.Cert = identity.Cert;
            }
            if (identity.RestChain != null)
            {
                this.RestChain = identity.RestChain;
            }
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
                if (RestChain != null)
                {
                    foreach (var cert in RestChain)
                    {
                        cert.Dispose();
                    }
                }
                Cert.Dispose();
                PrivateKey.Dispose();
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

