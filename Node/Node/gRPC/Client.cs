using System;
using Grpc.Net.Client;
using Certificatepb;

namespace Node.gRPC
{
    public class gRPCClient
    {

        public async Task Client()
	    {
            GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5001");
            Certificates.CertificatesClient client = new Certificates.CertificatesClient(channel);
            var request = new SigningRequest
            {
                
            };
            var response = await client.SignAsync(request);
        }
	}
}

