using System;
using System.Security.Cryptography.X509Certificates;
using Node.Certificate.Models;
using Org.BouncyCastle.X509;

namespace Node.Certificate
{
	abstract public class AbstractCertificateBuilder
    {
        public virtual void CreateCertificate() { }
        public abstract X509Certificate2 GetResult();

    }
}

