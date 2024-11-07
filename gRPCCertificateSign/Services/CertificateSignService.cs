using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using gRPCCertificateSign.Models;
using System.Security.Cryptography;
using Google.Protobuf;
using System.Globalization;
using Node.Certificate.Models;
using Node.Certificate;
using Org.BouncyCastle.Utilities;

namespace gRPCCertificateSign.Services;

public class CertificateSignService : CertificateSign.CertificateSignBase
{
    private readonly ILogger<CertificateSignService> _logger;
    private string CAcert = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ca.cert");
    private string CAkey = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ca.key");
    private CertificateAuthorizer loader;
    private X509Certificate2 Signer;
    

    public CertificateSignService(ILogger<CertificateSignService> logger)
    {
        _logger = logger;
        this.loader = new CertificateAuthorizer();
    }


    public override Task<SigningResponse> Sign(SigningRequest request, ServerCallContext context)
    {
        var peer = loader.FromContext(context);        
        if (peer == null)
        {
            var nullResponse = new SigningResponse();
            nullResponse.Chain.Add(ByteString.CopyFrom(0));
            return Task.FromResult(nullResponse);
        }
        var peerIdent = loader.PeerIdentityFromPeer(peer);
        var signedChainBytes = Sign(peerIdent.Cert);

        //TODO: add difficulty check add token
        var response = new SigningResponse();
        response.Chain.AddRange(signedChainBytes.Select(bytes => ByteString.CopyFrom(bytes)).ToList());
        return Task.FromResult(response);
    }


    public void LoadSignCertificate(X509Certificate2 signer)
    {
        ArgumentNullException.ThrowIfNull(signer);
        this.Signer = signer;
    }


    public List<byte[]> Sign(X509Certificate2 ca)
    {
        ArgumentNullException.ThrowIfNull(ca);
        ArgumentNullException.ThrowIfNull(Signer);
        var builder = new ConcreteSystemSecurityBuilder();



        var signedPeerCA = builder.SignCertificate(Signer, Signer.GetRSAPrivateKey().ExportRSAPrivateKey(), ca.PublicKey.ExportSubjectPublicKeyInfo(), ca, out _);
        var signedChainBytes = new List<byte[]> { signedPeerCA.RawData, Signer.RawData };
        //signedChainBytes.AddRange(ca..RawRestChain());
        return signedChainBytes;
    }



}

