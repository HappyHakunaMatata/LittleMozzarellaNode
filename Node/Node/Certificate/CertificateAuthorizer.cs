using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Node.Certificate.Models;

namespace Node.Certificate
{
	public class CertificateAuthorizer
	{
		private readonly string CAcert;
        private readonly string CAkey;
        private readonly string Identcert;
        private readonly string IdentKey;


		public CertificateAuthorizer()
		{
			CAcert = "ca.cert";
			CAkey = "ca.key";
			Identcert = "identity.cert";
			IdentKey = "identity.key";
        }

        public CertificateAuthorizer(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            CAcert = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ca.cert");
            CAkey = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ca.key");
            Identcert = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "identity.cert");
            IdentKey = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "identity.key");
        }

        public CertificateAuthorizer(Environment.SpecialFolder folder = Environment.SpecialFolder.UserProfile)
        {
            CAcert = Path.Combine(Environment.GetFolderPath(folder), "ca.cert");
            CAkey = Path.Combine(Environment.GetFolderPath(folder), "ca.key");
            Identcert = Path.Combine(Environment.GetFolderPath(folder), "identity.cert");
            IdentKey = Path.Combine(Environment.GetFolderPath(folder), "identity.key");
        }

        //TODO: Dispose for AsyncAlgorithm
        public AsymmetricAlgorithm? PrivateKeyFromPEM(string kb, SignatureAlgorithmName name)
		{
            if (name == SignatureAlgorithmName.sha256ECDSA)
            {
                ECDsa? EcDsa = null;
                TryImportECPrivateKey(kb, out EcDsa);
                return EcDsa;
            }
            if (name == SignatureAlgorithmName.sha256ECDSA)
            {
                RSA? rsa = null;
                TryImportRSAPrivateKey(kb, out rsa);
                return rsa;
            }
            return null;
        }


        public bool TryImportECPrivateKey(string kb, out ECDsa? result)
        {
            try
            {
                ECDsa rsa = ECDsa.Create();
                rsa.ImportFromPem(kb);
                result = rsa;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public bool TryImportRSAPrivateKey(string kb, out RSA? result)
        {
            try
            {
                RSA rsa = RSA.Create();
                rsa.ImportFromPem(kb);
                result = rsa;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public FullCertificateAuthority LoadFullCAConfig()
        {
            var peerConfig = LoadPeerCAConfig();
            var keyPem = File.ReadAllText(CAkey);
            SignatureAlgorithm signatureAlgorithm = new();
            var name = signatureAlgorithm.GetSignatureAlgorithm(peerConfig.Cert.SignatureAlgorithm.FriendlyName);
            var k = PrivateKeyFromPEM(keyPem, name);
            if (k == null)
            {
                throw new ArgumentNullException();
            }
            return new FullCertificateAuthority(peerConfig.RestChain, peerConfig.Cert, peerConfig.NodeID, k);
        }

        public PeerCertificateAuthority LoadPeerCAConfig()
		{
            var certs = new X509Certificate2Collection();
            certs.Import(CAcert);
            //TODO: add param to setup file to CAIndex-1
            var nodeID = NodeIDFromCert(certs[0]);
            return new PeerCertificateAuthority(
                certs.Skip(0).ToArray(),
                certs[0],
                nodeID);
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


        public FullIdentity LoadIdentConfig()
		{
            var certs = new X509Certificate2Collection();
            certs.Import(Identcert);
            var keyBytes = File.ReadAllBytes(IdentKey);
            var peerIdentity = PeerIdentityFromChain(certs);
            return new FullIdentity(peerIdentity, keyBytes);
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

        public PeerIdentity PeerIdentityFromChain(X509Certificate2Collection collection)
        {
            var nodeID = NodeIDFromCert(collection[0]);
            return new PeerIdentity(collection.Skip(2).ToArray(), collection[1], nodeID, collection[0]);
        }
    }
}

