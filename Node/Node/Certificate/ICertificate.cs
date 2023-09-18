using System;
using System.Security.Cryptography.X509Certificates;

namespace Node.Certificate
{
    public interface ICertificate
    {
        X509Certificate2 MakeCertificate(string host, string subject, X509Certificate2? signingCert);
    }
}

