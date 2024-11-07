using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Common.Certificate.Models
{
    public class Peer
    {
        public IPAddress IP { get; set; }
        public X509Certificate2Collection certificates { get; set; }

        public Peer(IPAddress Addr, X509Certificate2Collection certificates)
        {
            IP = Addr;
            this.certificates = certificates;
        }
    }
}

