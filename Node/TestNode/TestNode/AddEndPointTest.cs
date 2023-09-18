using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node;

namespace TestNode
{
    [TestClass]
    public class AddEndPointTest
    {
        ProxyServer proxyServer = new();
        [TestMethod]
        public void TestAddEndPoint_IsValid()
        {
            string address = "200:1111:2222:3333:4444:5555:6666:7777";
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, IPAddress.Parse(address));

            address = "201:00AA:40FE:3333:4444:5555:6666:7777";
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, IPAddress.Parse(address));

            address = "211:00AA:40FE:3333:4444:5555:6666:7777";
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, IPAddress.Parse(address));
        }
        [TestMethod]
        public void TestAddEndPoint_InvalidAddress()
        {
            

            string address = "2000:1111:2222:3333:4444:5555:6666:7777";
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, null);

            address = "301:00AA:40FE:3333:4444:5555:6666:7777";
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, null);

            address = "2200:00AA:40FE:3333:4444:5555:6666:7777";
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, null);

            address = "120:00AA:40FE:3333:4444:5555:6666:7777";
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, null);

            address = "1.1.1.1";
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, null);

            address = "Adress";
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, null);
        }
        [TestMethod]
        public void TestAddEndPoint_NullAddress()
        {
            
            string address = null;
            proxyServer.AddEndPoint(address);
            Assert.AreEqual(proxyServer.ListeningAddress, null);
        }
    }
}

