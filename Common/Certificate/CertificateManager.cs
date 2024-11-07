using System;
using System.Formats.Tar;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Grpc.Core;
using Microsoft.Extensions.Primitives;
using Common.Certificate.Enums;
using Common.Certificate.Models;


namespace Common.Certificate
{
	public class CertificateManager
	{
		private readonly string CAcert;
        private readonly string CAkey;
        private readonly string Identcert;
        private readonly string IdentKey;


		public CertificateManager()
		{
			CAcert = "ca.cert";
			CAkey = "ca.key";
			Identcert = "identity.cert";
			IdentKey = "identity.key";
        }


        public CertificateManager(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            try
            {
                CAcert = Path.Combine(path, "ca.cert");
                CAkey = Path.Combine(path, "ca.key");
                Identcert = Path.Combine(path, "identity.cert");
                IdentKey = Path.Combine(path, "identity.key");
            }
            catch
            {
                throw;
            }
        }


        public CertificateManager(Environment.SpecialFolder folder = Environment.SpecialFolder.UserProfile)
        {
            try
            {
                CAcert = Path.Combine(Environment.GetFolderPath(folder), "ca.cert");
                CAkey = Path.Combine(Environment.GetFolderPath(folder), "ca.key");
                Identcert = Path.Combine(Environment.GetFolderPath(folder), "identity.cert");
                IdentKey = Path.Combine(Environment.GetFolderPath(folder), "identity.key");
            }
            catch
            {
                throw;
            }
        }


        public AsymmetricAlgorithm? PrivateKeyFromPEM(string kb, SignatureAlgorithmName name)
		{
            ArgumentException.ThrowIfNullOrEmpty(kb);
            ArgumentNullException.ThrowIfNull(name);

            try
            {
                if (name == SignatureAlgorithmName.sha256ECDSA)
                {
                    TryImportECPrivateKey(kb, out var EcDsa);
                    return EcDsa;
                }
                if (name == SignatureAlgorithmName.sha256RSA)
                {
                    TryImportRSAPrivateKey(kb, out var rsa);
                    return rsa;
                }
                return null;
            }
            catch
            {
                throw;
            }
        }


        public bool TryImportECPrivateKey(string kb, out ECDsa? result)
        {
            ArgumentException.ThrowIfNullOrEmpty(kb);
            try
            {
                result = ECDsa.Create();
                result.ImportFromPem(kb);
                return true;
            }
            catch
            {
                throw;
            }
        }


        public bool TryImportRSAPrivateKey(string kb, out RSA? result)
        {
            ArgumentException.ThrowIfNullOrEmpty(kb);
            try
            {
                result = RSA.Create();
                result.ImportFromPem(kb);
                return true;
            }
            catch
            {
                throw;
            }
        }


        public bool TryLoadFullCAConfig(out FullCertificateAuthority<ECDsa>? fullCertificateAuthority)
        {
            try
            {
                if (File.Exists(CAcert) && File.Exists(CAkey))
                {
                    fullCertificateAuthority = LoadFullCAConfig();
                    return true;
                }
                fullCertificateAuthority = null;
                return false;
            }
            catch
            {
                throw;
            }
        }


        public bool TryLoadIdentConfig(out FullIdentity? fullIdentity)
        {
            try
            {
                if (File.Exists(Identcert) && File.Exists(IdentKey))
                {
                    fullIdentity = LoadIdentConfig();
                    return true;
                }
                fullIdentity = null;
                return false;
            }
            catch
            {
                throw;
            }
        }


        public FullCertificateAuthority LoadFullCAConfig()
        {
            try
            {
                var peerConfig = LoadPeerCAConfig();
                var keyPem = File.ReadAllText(CAkey);
                
                ArgumentNullException.ThrowIfNullOrEmpty(peerConfig.Cert.SignatureAlgorithm.FriendlyName);
                var name = GetSignatureAlgorithm(peerConfig.Cert.SignatureAlgorithm.FriendlyName);
                var k = PrivateKeyFromPEM(keyPem, SignatureAlgorithmName.sha256ECDSA);
                if (k == null)
                {
                    throw new ArgumentNullException();
                }
                return new FullCertificateAuthority(peerConfig.RestChain, peerConfig.Cert, peerConfig.NodeID, (ECDsa)k);
            }
            catch
            {
                throw;
            }
        }


        public PeerCertificateAuthority LoadPeerCAConfig()
		{
            try
            {
                var certs = new X509Certificate2Collection();
                certs.Import(CAcert);
                var nodeID = NodeIDFromCert(certs[(int)Indexes.CAIndex - 1]);
                return new PeerCertificateAuthority(
                    certs.Skip((int)Indexes.CAIndex).ToArray(),
                    certs[(int)Indexes.CAIndex - 1],
                    nodeID);
            }
            catch
            {
                throw;
            }
        }


		public byte[] NodeIDFromCert(X509Certificate2 certificate)
		{
            ArgumentNullException.ThrowIfNull(certificate);
            try
            {
                var version = IDVersionFromCert(certificate);
                return NodeIDFromKey(certificate.PublicKey.ExportSubjectPublicKeyInfo(), version);
            }
            catch
            {
                throw;
            }
		}


        public byte[] NodeIDFromKey(byte[] PublicKey, ushort version)
        {
            ArgumentNullException.ThrowIfNull(PublicKey);
            ArgumentNullException.ThrowIfNull(version);
            try
            {
                var idBytes = DoubleSHA256PublicKey(PublicKey);
                if (idBytes == null)
                {
                    throw new ArgumentNullException();
                }
                return NewVersionedID(idBytes, version);
            }
            catch
            {
                throw;
            }
        }


        public byte[]? DoubleSHA256PublicKey(byte[] publickey)
        {
            ArgumentNullException.ThrowIfNull(publickey);
            try
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    var mid = sha256Hash.ComputeHash(publickey);
                    return sha256Hash.ComputeHash(mid);
                }
            }
            catch
            {
                throw;
            }
        }


        public byte[] NewVersionedID(byte[] ID, ushort version)
        {
            ArgumentNullException.ThrowIfNull(ID);
            ArgumentNullException.ThrowIfNull(version);
            try
            {
                ID[ID.Length - 1] = Convert.ToByte(version);
                return ID;
            }
            catch
            {
                throw;
            }
        }


        public FullIdentity LoadIdentConfig()
		{
            try
            {
                var certs = new X509Certificate2Collection();
                certs.Import(Identcert);
                var certificates = certs.Reverse();
                var keyPem = File.ReadAllText(IdentKey);
                var keyBytes = PrivateKeyFromPEM(keyPem, SignatureAlgorithmName.sha256ECDSA);
                var peerIdentity = PeerIdentityFromChain(certificates);
                ArgumentNullException.ThrowIfNull(keyBytes);
                return new FullIdentity(peerIdentity, keyBytes);
            }
            catch
            {
                throw;
            }
        }


        public ushort IDVersionFromCert(X509Certificate2 certificate)
        {
            ArgumentNullException.ThrowIfNull(certificate);
            try
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
            catch
            {
                throw;
            }
        }


        public ushort GetIDVersion(string ID)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(ID);
            try
            {
                char id = ID[0];
                if (int.TryParse(id.ToString(), out var result))
                {
                    return (ushort)result;
                }
                return (ushort)result;
            }
            catch
            {
                throw;
            }
        }

        
        public PeerIdentity PeerIdentityFromChain(IEnumerable<X509Certificate2> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);
            try
            {
                var certificates = collection.ToArray();
                var nodeID = NodeIDFromCert(certificates[(int)Indexes.CAIndex]);
                return new PeerIdentity(collection.Skip((int)Indexes.CAIndex + 1).ToArray(), certificates[(int)Indexes.CAIndex], nodeID, certificates[(int)Indexes.LeafIndex]);
            }
            catch
            {
                throw;
            }
        }


        public Peer? FromContext(ServerCallContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            try
            {
                var httpContext = context.GetHttpContext();
                if (httpContext == null)
                {
                    return null;
                }
                var collection = new X509Certificate2Collection();
                X509Certificate2? x509Certificate2 = null;
                if (httpContext.Request.Headers.TryGetValue("Client-Cert", out var clientCertHeader))
                {
                    if (StringValues.IsNullOrEmpty(clientCertHeader))
                    {
                        return null;
                    }
                    x509Certificate2 = CertificateFromHeader(clientCertHeader.ToString());
                }

                if (httpContext.Request.Headers.TryGetValue("Client-Cert-Chain", out var clientChainHeader))
                {
                    if (StringValues.IsNullOrEmpty(clientChainHeader))
                    {
                        return null;
                    }
                    var certificates = CollectionFromHeader(clientChainHeader.ToString());
                    collection.AddRange(certificates);
                }

                var clientCertificate = httpContext.Connection.ClientCertificate;
                if (clientCertificate == null || x509Certificate2 == null)
                {
                    return null;
                }
                var equals = clientCertificate.RawData.SequenceEqual(x509Certificate2.RawData);
                if (!equals)
                {
                    return null;
                }
                var X509certificate2collection = new X509Certificate2Collection(clientCertificate);
                X509certificate2collection.AddRange(collection);
                var ip = context.GetHttpContext()?.Connection.RemoteIpAddress;
                if (ip == null)
                {
                    return null;
                }
                return new Peer(ip, X509certificate2collection);
            }
            catch
            {
                throw;
            }
        }
        

        private X509Certificate2Collection CollectionFromHeader(string Header)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(Header);
            try
            {
                var SplitedHeader = Header.Split(" ");
                string FindAllBetweenDots = "(?<=:)(.*?)(?=:)";
                string EverySixtyFour = "(.{64})";
                string replacement = "$1\n";
                X509Certificate2Collection collection = new X509Certificate2Collection();
                foreach (var content in SplitedHeader)
                {
                    var match = Regex.Match(content, FindAllBetweenDots).Value;
                    var Certificate = Regex.Replace(match, EverySixtyFour, replacement);
                    var CertificatePem = $"-----BEGIN CERTIFICATE-----\n{Certificate}\n-----END CERTIFICATE-----";
                    var cert = X509Certificate2.CreateFromPem(CertificatePem);
                    collection.Add(cert);
                }
                return collection;
            }
            catch
            {
                throw;
            }
        }


        private X509Certificate2 CertificateFromHeader(string Header)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(Header);
            try
            {
                string FindAllBetweenDots = "(?<=:)(.*?)(?=:)";
                string EverySixtyFour = "(.{64})";
                string replacement = "$1\n";
                var match = Regex.Match(Header, FindAllBetweenDots).Value;
                var Certificate = Regex.Replace(match, EverySixtyFour, replacement);
                var CertificatePem = $"-----BEGIN CERTIFICATE-----\n{Certificate}\n-----END CERTIFICATE-----";
                var cert = X509Certificate2.CreateFromPem(CertificatePem);
                return cert;
            }
            catch
            {
                throw;
            }
        }


        public PeerIdentity PeerIdentityFromPeer(Peer peer)
        {
            ArgumentNullException.ThrowIfNull(peer);
            try
            {
                var chain = peer.certificates;
                if (chain.Count - 1 < 1)
                {
                    throw new ArgumentException();
                }
                return PeerIdentityFromChain(chain);
            }
            catch
            {
                throw;
            }
        }


        public X509Certificate2 ImportPrivateKeyToCertificate(X509Certificate2 certificate, AsymmetricAlgorithm PrivateKey)
        {
            ArgumentNullException.ThrowIfNull(certificate);
            ArgumentNullException.ThrowIfNull(PrivateKey);
            try
            {
                return certificate.CopyWithPrivateKey((ECDsa)PrivateKey);
            }
            catch
            {
                throw;
            }
        }


        public SignatureAlgorithmName GetSignatureAlgorithm(string signatureAlgorithm)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(signatureAlgorithm);
            try
            {
                if (Enum.TryParse<SignatureAlgorithmName>(signatureAlgorithm, out var result))
                {
                    return result;
                }
                return SignatureAlgorithmName.None;
            }
            catch
            {
                throw;
            }
        }
    }
}

