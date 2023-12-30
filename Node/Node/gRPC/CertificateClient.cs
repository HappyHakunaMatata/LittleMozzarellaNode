using System;
using Grpc.Net.Client;
using gRPCCertificateSign;

namespace Node.gRPC
{
    public class CertificateClient
    {

        public async Task<List<Google.Protobuf.ByteString>> Sign()
	    {
            GrpcChannel channel = GrpcChannel.ForAddress("http://localhost:80");
            CertificateSign.CertificateSignClient client = new CertificateSign.CertificateSignClient(channel);
            var request = new SigningRequest
            {
                Timestamp = 1,
                AuthToken = "1",
            };
            var response = client.Sign(request);
            var chain = response.Chain;
            return chain.ToList();
        }
	}
}

