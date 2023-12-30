using System;
using System.Net;
using System.Net.Security;

namespace gRPCCertificateSign.Models
{
    public class Peer
    {
        public IPEndPoint IP { get; set; }
        public SslStream State { get; set; }

        public Peer(IPEndPoint Addr, SslStream ConnectionState)
        {
            IP = Addr;
            State = ConnectionState;
        }
    }
}

