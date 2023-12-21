using System;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using Node.Certificate.Models;
using Node.gRPC.Models;

namespace Node.gRPC
{
	public class Server
	{
        private readonly FullCertificateAuthority CA;

		public Server(ref FullCertificateAuthority ca)
		{
            this.CA = ca;
            /*const int Port = 50051;
            Server server = new Server
            {
                Services = { YourService.BindService(new YourServiceImplementation()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
            */
        }

        public void Sign()
        {
            var signedPeerCA = Sign(new X509Certificate2());
            List<byte[]> signedChainBytes = new List<byte[]> { signedPeerCA.RawData, CA.Cert.RawData };
            signedChainBytes.AddRange(CA.RawRestChain());
        }


        public X509Certificate2 Sign(X509Certificate2 cert)
        {
            var signedCert = CreateCertificate(cert.GetPublicKey(), CA.Cert.GetECDsaPrivateKey().ExportECPrivateKey(), cert, CA.Cert);
            return signedCert;
        }

        public X509Certificate2 CreateCertificate(byte[] signee, byte[] signer, X509Certificate2 template, X509Certificate2 issuer)
        {
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportSubjectPublicKeyInfo(signee, out _);
                ecdsa.ImportECPrivateKey(signer, out _);
                CertificateRequest certificateRequest = new CertificateRequest(template.SubjectName, ecdsa, HashAlgorithmName.SHA256);
                return certificateRequest.Create(issuer, DateTimeOffset.Now, DateTimeOffset.Now, template.GetSerialNumber());
            }
        }

        public PeerIdentity PeerIdentityFromPeer(Peer peer)
        {
            var certificate = peer.State.RemoteCertificate;
            if (certificate == null)
            {
                throw new ArgumentNullException();
            }
            var chain = new X509Certificate2Collection(new X509Certificate2(certificate));
            if (chain.Count-1 < 1)
            {
                throw new ArgumentException();
            }
            var pi = PeerIdentityFromChain(chain);
            return pi;
        }

        public PeerIdentity PeerIdentityFromChain(X509Certificate2Collection collection)
        {
            var nodeID = NodeIDFromCert(collection[0]);
            return new PeerIdentity(collection.Skip(2).ToArray(), collection[1], nodeID, collection[0]);
        }

        public byte[] NodeIDFromCert(X509Certificate2 certificate)
        {
            var version = IDVersionFromCert(certificate);
            return NodeIDFromKey(certificate.PublicKey.ExportSubjectPublicKeyInfo(), version);
        }


        public byte[] NodeIDFromKey(byte[] PublicKey, ushort version)
        {
            var idBytes = DoubleSHA256PublicKey(PublicKey);
            if (idBytes == null)
            {
                throw new ArgumentNullException();
            }
            return NewVersionedID(idBytes, version);
        }

        public byte[]? DoubleSHA256PublicKey(byte[] publickey)
        {
            byte[]? end;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var mid = sha256Hash.ComputeHash(publickey);
                end = sha256Hash.ComputeHash(mid);
            }
            return end;
        }


        public byte[] NewVersionedID(byte[] ID, ushort version)
        {
            ID[ID.Length - 1] = Convert.ToByte(version);
            return ID;
        }


        public ushort IDVersionFromCert(X509Certificate2 certificate)
        {
            foreach (X509Extension ext in certificate.Extensions)
            {
                if (ext.Oid != null && ext.Oid.Value == "2.999.2.1")
                {
                    return GetIDVersion(ext.Oid.Value);
                }
            }
            return 0;
        }


        public ushort GetIDVersion(string ID)
        {
            int result = 0;
            if (string.IsNullOrEmpty(ID))
            {
                throw new ArgumentException("Input string is null or empty.");
            }

            char id = ID[0];

            if (int.TryParse(id.ToString(), out result))
            {
                return (ushort)result;
            }
            return (ushort)result;
        }
    }
}

