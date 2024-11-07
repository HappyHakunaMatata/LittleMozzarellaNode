using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using Grpc.Net.Client;
using gRPCCertificateSign;

namespace Node.gRPC
{
    public class CertificateClient
    {

        public async Task<List<byte[]>> Sign(Options options)
	    {
            var IdentityChain = options.fullIdentity.Chain();
            var collection = TLSCert(IdentityChain.ToArray(), options.fullIdentity.Leaf, options.fullIdentity.PrivateKey);
            var handler = new HttpClientHandler();
            handler.ClientCertificates.AddRange(collection);
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-store");
            httpClient.DefaultRequestHeaders.Add("Client-Cert", $"{options.fullIdentity.ClientHeaderCertificate()}");
            httpClient.DefaultRequestHeaders.Add("Client-Cert-Chain", options.fullIdentity.ClientHeaderCertificateChain());
            


            GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:443", new GrpcChannelOptions
            {
                HttpClient = httpClient,
                Credentials = ChannelCredentials.SecureSsl,
            });
            CertificateSign.CertificateSignClient client = new CertificateSign.CertificateSignClient(channel);
            var request = new SigningRequest
            {
                Timestamp = 1,
                AuthToken = "1",
            };
            var response = client.Sign(request);
            var ResponseChain = response.Chain.ToList();
            var chain = new List<byte[]>();
            chain.AddRange(ResponseChain.Select(bytes => bytes.ToByteArray()));
            return chain;
        }


        public string CreateHeader(List<byte[]> chain)
        {
            string header = "";
            foreach (var i in chain)
            {
                header += $":{BitConverter.ToString(i).Replace("-", "")}: ,";
            }
            header = header.TrimEnd(new char[] { ' ', ',' });

            return header;
        }

        //TODO: ecds dispose
        public X509Certificate2Collection TLSCert(X509Certificate2[] chain, X509Certificate2 leaf, AsymmetricAlgorithm PrivateKey)
        {
            ArgumentNullException.ThrowIfNull(chain);
            ArgumentNullException.ThrowIfNull(PrivateKey);
            if (leaf == null)
            {
                leaf = chain[0];
                ArgumentNullException.ThrowIfNull(leaf);
            }
            var ecdsa = (ECDsa)PrivateKey;
            var collection = new X509Certificate2Collection(chain);
            var leafWithKey = leaf.CopyWithPrivateKey(ecdsa);
            collection.Add(leafWithKey);
            return collection;
        }
    }
}

