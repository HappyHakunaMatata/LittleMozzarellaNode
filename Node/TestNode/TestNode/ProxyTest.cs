using System;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node;
using System.Text;
using System.Security.Policy;
using Node.TunnelManagers;
using Node.TunnelExecutors.Models;

namespace TestNode
{
    [TestClass]
    public class ProxyTest
	{
        TunnelManager proxyServer = new SocketTunnelManager();

        /// <summary>
        /// To use this test you have to change access modifiers
        /// </summary>
        /*
        [TestMethod]
        public async Task CloseSocket()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Tunneling>();
            var FakeWebServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeWebServer.Bind(new IPEndPoint(IPAddress.Loopback, 54321));
            FakeWebServer.Listen(1);
            var FakeProxyServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeProxyServer.Bind(new IPEndPoint(IPAddress.Loopback, 12345));
            FakeProxyServer.Listen(1);
            Socket FakeProxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeProxy.Connect(new IPEndPoint(IPAddress.Loopback, 54321));
            Socket FakeAcceptedProxy = FakeWebServer.Accept();
            Socket FakeClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeClient.Connect(new IPEndPoint(IPAddress.Loopback, 12345));
            Socket FakeAcceptedClient = FakeProxyServer.Accept();
            Tunneling session = new Tunneling(
                FakeAcceptedProxy,
                FakeAcceptedClient,
                logger);
            session.StartTunneling();
            session._client.Close();
            try
            {
                await session.CloseSocketAsync(session._client);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Close socket exception catched: {e.Message}");
                Assert.IsTrue(e is ObjectDisposedException socketException);
            }
            
            session = new Tunneling(
                FakeAcceptedProxy,
                FakeAcceptedClient,
                logger);
            session.StartTunneling();
            session._client.Dispose();
            try
            {
                await session.CloseSocketAsync(session._client);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Dispose socket exception catched: {e.Message}");
                Assert.IsTrue(e is ObjectDisposedException socketException);
            }
        }*/

        [TestMethod]
        public async Task TryCreateTunnelParseTest()
        {
            List<string> check_list_true = new List<string>(){
                "CONNECT www.example.com:443 HTTP/1.1",
                "CONNECT 123.www.site.my.ry:20 HTTP/1.1",
                "CONNECT 1-2-3.www.site.my.ry:20 HTTP/1.1",
                "CONNECT www.example.com:443 HTTP/2",
                "CONNECT www.example.com:443 HTTP/3",
            };
            List<string> check_list_false = new List<string>(){
                "CONNECT www.example.com HTTP/1.1",
                "CONNECT 123.www.site.my.ry:200000 HTTP/1.1",
                "CONNECT //www.example.com:443// HTTP/1.1",
                "CONNECT www.example.:443 HTTP/1.1",
                "CONNECT www.example$:443 HTTP/1.1",
                "connect www.example:443 HTTP/1.1",
                "CONECT www.example:443 HTTP/1.1",
                "CONNECT www.example:443 HTP/1.1",
                "www.example:443",
                "CONNECT www.example.com:443 HTTP/0.1.1",
                "CONNECT www.example.com:443 HTTP/1.1.3",
                "CONNECT www.example.com:443 HTTP/2.1",
                "CONNECT www.example.com:443 HTTP/1.2",
                "CONNECT www.example.com:443 HTTP/3.1",
                "CONNECT www.example.com:443 HTTPS/1.1",
                "CONNECT www.example.com:443 HTTP1.1",
            };
            var FakeWebServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeWebServer.Bind(new IPEndPoint(IPAddress.Loopback, 54321));
            FakeWebServer.Listen(1);
            var FakeProxyServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeProxyServer.Bind(new IPEndPoint(IPAddress.Loopback, 12345));
            FakeProxyServer.Listen(1);
            Socket FakeProxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeProxy.Connect(new IPEndPoint(IPAddress.Loopback, 54321));
            Socket FakeAcceptedProxy = FakeWebServer.Accept();
            Socket FakeClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeClient.Connect(new IPEndPoint(IPAddress.Loopback, 12345));
            Socket FakeAcceptedClient = FakeProxyServer.Accept();
            RequestStruct? requestStruct = new RequestStruct();
            foreach (var i in check_list_true)
            {
                var requestParts = Encoding.UTF8.GetBytes(i);
                var result = proxyServer.TryCheckHttpMethod(requestParts, out requestStruct);
                Assert.IsTrue(result);
            }
            foreach (var i in check_list_false)
            {
                var requestParts = Encoding.UTF8.GetBytes(i);
                var result = proxyServer.TryCheckHttpMethod(requestParts, out requestStruct);
                Assert.IsFalse(result);
            }
        }
        /// <summary>
        /// To use this test you have to change _ioTimeout access modifiers
        /// </summary>
        /*
        [TestMethod]
        public async Task TryCreateTunnelCommitTest()
        {
            proxyServer._ioTimeout = TimeSpan.FromMilliseconds(0.1);
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Tunneling>();
            var FakeWebServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeWebServer.Bind(new IPEndPoint(IPAddress.Loopback, 54321));
            FakeWebServer.Listen(1);
            var FakeProxyServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeProxyServer.Bind(new IPEndPoint(IPAddress.Loopback, 12345));
            FakeProxyServer.Listen(1);
            Socket FakeProxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeProxy.Connect(new IPEndPoint(IPAddress.Loopback, 54321));
            Socket FakeAcceptedProxy = FakeWebServer.Accept();
            Socket FakeClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FakeClient.Connect(new IPEndPoint(IPAddress.Loopback, 12345));
            Socket FakeAcceptedClient = FakeProxyServer.Accept();
            string request = "CONNECT www.example.com:443 HTTP/1.1";
            string[] requestParts = request.
                Split("\n", StringSplitOptions.RemoveEmptyEntries)[0].
                Split(" ", StringSplitOptions.RemoveEmptyEntries);
            try
            {
                var result = await proxyServer.TryCreateTunnel(requestParts, FakeAcceptedProxy);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is OperationCanceledException socketException);
            }
        }*/

       
    }
}

