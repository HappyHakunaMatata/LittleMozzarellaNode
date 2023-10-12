using System;
namespace Node.Certificate.Models
{
	public class CertificateSettings
	{
        private const string DefaultRootCertificateIssuer = "localhost";
        private string? _issuerName;

        private const string DefaultRootRootCertificateName = "localhost";
        private string? _certificateName;

        private string? _host;
        private const string DefaultRootRootCertificateHost = "127.0.0.1";

        public string PfxPassword = string.Empty;
        public string Pkcs12StorePassword = "password";

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
                    return DefaultRootRootCertificateHost;
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

        /// <summary>
        ///     Number of Days generated HTTPS certificates are valid for.
        ///     Maximum allowed on iOS 13 is 825 days and it is the default.
        /// </summary>
        private const int DefaultCertificateValidDays = 825;
        private int? _certificateValidDays;
        public int CertificateValidDays
        {
            get
            {
                if (_certificateValidDays != null)
                {
                    return _certificateValidDays.Value;
                }
                else
                {
                    return DefaultCertificateValidDays;
                }
            }
            set
            {
                _certificateValidDays = value;
            }
        }

        private const int DefaultCertificateGraceDays = 365;
        private int? _certificateGraceDays;
        public int CertificateGraceDays
        {
            get
            {
                if (_certificateGraceDays != null)
                {
                    return _certificateGraceDays.Value;
                }
                else
                {
                    return DefaultCertificateGraceDays;
                }
            }
            set
            {
                _certificateGraceDays = value;
            }
        }

        private const int DefaultKeyStrength = 2048;
        private int? _keyStrength;
        public int KeyStrength
        {
            get
            {
                if (_keyStrength != null)
                {
                    return _keyStrength.Value;
                }
                else
                {
                    return DefaultKeyStrength;
                }
            }
            set
            {
                _keyStrength = value;
            }
        }


    }
}

