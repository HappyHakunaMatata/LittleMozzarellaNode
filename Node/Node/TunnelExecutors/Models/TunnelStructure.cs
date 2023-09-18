using System.Net.Security;
using System.Net.Sockets;

namespace Node.TunnelExecutors.Models
{
	public class TunnelStructureStore: Dictionary<Guid, TunnelExecutor> {}

    public struct TunnelStructure
    {

        public TunnelStructure()
        {
            key = new Guid();
        }
        public Guid key;
		public Socket remote;
        public SslStream sslRemoteStream;
        public Socket client;
        public SslStream sslClientStream;
    }
}

