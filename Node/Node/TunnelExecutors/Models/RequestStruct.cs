using System.Net;
using System.Text;

namespace Node.TunnelExecutors.Models
{
	public struct RequestStruct
	{
		public string? host;
		public RequestLine requestLine;
		

		public RequestStruct(byte[] request)
		{
			string encodedRequest = Encoding.UTF8.GetString(request);
			string[] SpletedRequest = encodedRequest.
				Split("\n", StringSplitOptions.RemoveEmptyEntries);
            ArgumentNullException.ThrowIfNull(SpletedRequest[0]);
            ArgumentNullException.ThrowIfNull(SpletedRequest[1]);
            ArgumentNullException.ThrowIfNull(SpletedRequest[2]);
            requestLine = new RequestLine(SpletedRequest[0]);
        }

		public struct Header
		{

		}

        public struct MessageBody
        {

        }
		 
        public struct RequestLine
        {
			public string Method;
			public string RequestURI;
            public string Host;
            public int? Port;
            public EndPoint endPoint;
            public string Version;


            public RequestLine(string requestLine)
			{
				string[] splitedRequestLine = requestLine.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                ArgumentNullException.ThrowIfNull(splitedRequestLine[0]);
                ArgumentNullException.ThrowIfNull(splitedRequestLine[1]);
                ArgumentNullException.ThrowIfNull(splitedRequestLine[2]);
				Method = splitedRequestLine[0];
                RequestURI = splitedRequestLine[1];
                Version = splitedRequestLine[2].Remove(splitedRequestLine[2].Length - 1);


                endPoint = GetEndPoint(RequestURI);
            }

            private EndPoint GetEndPoint(string uri)
            {
                var SplitedRequestURI = uri.Split(":");
                ArgumentNullException.ThrowIfNull(SplitedRequestURI[0]);
                ArgumentNullException.ThrowIfNull(SplitedRequestURI[1]);
                var host = Uri.CheckHostName(SplitedRequestURI[0]);
                if (host == UriHostNameType.Unknown)
                {
                    throw new ArgumentException("UriHostNameType is uknown");
                }
                int port;
                if (!int.TryParse(SplitedRequestURI[1], out port))
                {
                    throw new ArgumentException($"Method : {Method} is unsupported");
                }
                if (port < 0 || port > 65535)
                {
                    throw new ArgumentException("Port is out of range");
                }
                Port = port;
                Host = SplitedRequestURI[0];
                return new DnsEndPoint(SplitedRequestURI[0], port);
            }
        }
    }
}

