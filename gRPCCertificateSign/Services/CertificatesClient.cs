using Grpc.Core;
using gRPCCertificateSign;

namespace gRPCCertificateSign.Services;

public class CertificateSignService : CertificateSign.CertificateSignBase
{
    private readonly ILogger<CertificateSignService> _logger;
    public CertificateSignService(ILogger<CertificateSignService> logger)
    {
        _logger = logger;
    }

    public Task<SigningResponse> Sign(SigningRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SigningResponse
        {
            
        });
    }
}

