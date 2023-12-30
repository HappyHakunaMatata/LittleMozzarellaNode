using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using Node.Certificate.Models;
using gRPCCertificateSign.Models;
using System.Security.Cryptography;
using Node.Certificate;
using Google.Protobuf;

namespace gRPCCertificateSign.Services;

public class CertificateSignService : CertificateSign.CertificateSignBase
{
    private readonly ILogger<CertificateSignService> _logger;
    private string CAcert = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ca.cert");
    private string CAkey = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ca.key");

    public CertificateSignService(ILogger<CertificateSignService> logger)
    {
        _logger = logger;
    }

    public override Task<SigningResponse> Sign(SigningRequest request, ServerCallContext context)
    {
        var peer = FromContext(context);
        if (context.UserState.TryGetValue("CustomKey", out var customValue))
        {
            // Использование customValue
            _logger.LogError($"CustomValue: {customValue}");
        }
        if (peer == null)
        {
            
            return Task.FromResult(new SigningResponse
            {

            });
        }
        var peerIdent = PeerIdentityFromPeer(peer);
        var signedChainBytes = Sign(peerIdent.Cert);
        //TODO: add difficulty check add token
        

        var response = new SigningResponse();
        response.Chain.AddRange(signedChainBytes.Select(bytes => ByteString.CopyFrom(bytes)).ToList());
        return Task.FromResult(response);
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


    public List<byte[]> Sign(X509Certificate2 ca)
    {
        var signer = LoadFullCAConfig();
        var builder = new ConcreteSystemSecurityBuilder();

        var signedPeerCA = builder.SignCertificate(signer.Cert, signer.PrivateKey, ca.PublicKey.ExportSubjectPublicKeyInfo(), ca);

        var signedChainBytes = new List<byte[]> { signedPeerCA.RawData, signer.Cert.RawData };
        signedChainBytes.AddRange(signer.RawRestChain());
        
        return signedChainBytes;
    }

    public Peer? FromContext(ServerCallContext context)
    {
        
        return context.UserState as Peer;
    }

    public PeerIdentity PeerIdentityFromPeer(Peer peer)
    {
        var certificate = peer.State.RemoteCertificate;
        if (certificate == null)
        {
            throw new ArgumentNullException();
        }
        var chain = new X509Certificate2Collection(new X509Certificate2(certificate));
        if (chain.Count - 1 < 1)
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
}

