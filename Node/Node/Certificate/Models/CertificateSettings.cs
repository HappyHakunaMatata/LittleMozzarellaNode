using System;
namespace Node.Certificate.Models
{
	public class CertificateSettings
	{
        private const string DefaultRootCertificateIssuer = "MozzarellaNetwork";
        private string? _issuerName;

        private const string DefaultRootRootCertificateName = "CN=MozzarellaNetwork Root Certificate Authority";
        private string? _certificateName;

        private string? _host;

        internal string PfxPassword = "pass";

        public string PfxPath = "";


        public string IssuerName
        {
            get
            {
                if (_issuerName != null)
                {
                    return _issuerName;
                }
                else
                {
                    return DefaultRootCertificateIssuer;
                }
            }
            set
            {
                _issuerName = value;
            }
        }

        public string Host
        {
            get
            {
                if (_host != null)
                {
                    return _host;
                }
                else
                {
                    return DefaultRootRootCertificateName;
                }
            }
            set
            {
                _host = value;
            }
        }


        public string RootCertificateName
        {
            get
            {
                if (_certificateName != null)
                {
                    return _certificateName;
                }
                else
                {
                    return DefaultRootRootCertificateName;
                }
            }
            set
            {
                _certificateName = value;
            }
        }
    }
}

