using System;
using System.IO;
using System.Net;

namespace Common.Certificate.Models
{
	public class CertificateSettings
	{
        private const string DefaultRootCertificateIssuer = "LittleMozzarella";
        private string? _issuerName;

        private string? _host;
        private const string DefaultRootRootCertificateHost = "127.0.0.1";

        public string PfxPassword = string.Empty;
        public string Pkcs12StorePassword = "password";

        public string PfxPath = "";

        public string? OrganizationName = null;
        public byte versionID = 0;


        public ushort minDifficulty = 8;
        public UInt16 Difficulty = 14;
        public ushort concurrency = 4;



        public IPAddress? IP { get; set; }

        public void SetIPAdress(string IP)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(IP);
            if (IPAddress.TryParse(IP, out var iPAddress))
            {
                this.IP = iPAddress;
            }
        }

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


        public string? RootCertificateName = null;

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

    }
}

