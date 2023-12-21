using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Node.TunnelManagers;


namespace TestNode
{
    [TestClass]
    public class AddressTest
    {
        TunnelManager proxyServer = new SocketTunnelManager();
        [TestMethod]
        public void TestAddress_IsValid()
        {
            IPAddress address = IPAddress.Parse("200:1111:2222:3333:4444:5555:6666:7777");
            bool result = proxyServer.IsValid(address, out proxyServer.ListeningAddress);
            Assert.IsTrue(result);

            address = IPAddress.Parse("201:00AA:40FE:3333:4444:5555:6666:7777");
            result = proxyServer.IsValid(address, out proxyServer.ListeningAddress);
            Assert.IsTrue(result);

            address = IPAddress.Parse("211:00AA:40FE:3333:4444:5555:6666:7777");
            result = proxyServer.IsValid(address, out proxyServer.ListeningAddress);
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void TestCheckMask_InvalidAddress()
        {
            IPAddress address = IPAddress.Parse("2000:1111:2222:3333:4444:5555:6666:7777");
            bool result = proxyServer.IsValid(address, out proxyServer.ListeningAddress);
            Assert.IsFalse(result);

            address = IPAddress.Parse("301:00AA:40FE:3333:4444:5555:6666:7777");
            result = proxyServer.IsValid(address, out proxyServer.ListeningAddress);
            Assert.IsFalse(result);

            address = IPAddress.Parse("2200:00AA:40FE:3333:4444:5555:6666:7777");
            result = proxyServer.IsValid(address, out proxyServer.ListeningAddress);
            Assert.IsFalse(result);
            address = IPAddress.Parse("120:00AA:40FE:3333:4444:5555:6666:7777");
            result = proxyServer.IsValid(address, out proxyServer.ListeningAddress);
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void TestCheckMask_NullAddress()
        {
            IPAddress address = null;
            bool result = proxyServer.IsValid(address, out proxyServer.ListeningAddress);
            Assert.IsFalse(result);
        }
    }
}

