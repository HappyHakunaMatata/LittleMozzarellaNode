using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Node;
using Node.Session;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting.Server;
using Org.BouncyCastle.Bcpg;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System.Threading;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Utilities.Encoders;
using Microsoft.Extensions.Internal;

namespace TestNode
{

    [TestClass]
    public class TunnelingTest
    {
        
        
        [TestMethod]
        public void CreateSessionsTest()
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
            byte[] testData = { 1, 2, 3, 4, 5 };
            byte[] receivedData = new byte[testData.Length];
            FakeProxy.Send(testData);
            FakeClient.Receive(receivedData);
            CollectionAssert.AreEqual(testData, receivedData);
        }
        
        [TestMethod]
        public void DisposeSessionTest()
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
            byte[] testData = { 1, 2, 3, 4, 5 };
            byte[] receivedData = new byte[testData.Length];
            FakeProxy.Send(testData);
            FakeClient.Receive(receivedData);
            CollectionAssert.AreEqual(testData, receivedData);
            session.Dispose();
            try
            {
                FakeProxy.Send(testData);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is SocketException socketException);
            }
        }

        /// <summary>
        /// To test IsExpired you have to change access modifiers
        /// </summary>
        /*
        [TestMethod]
        public void IsExpiredTest()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Tunneling>();
            Tunneling session = new Tunneling(
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                logger);
            Assert.IsFalse(session.IsExpired());
            session.LastAccessed = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(30);
            Assert.IsTrue(session.IsExpired());
        }
        */

        /// <summary>
        /// To test Send you have to change access modifiers
        /// </summary>
        /*
        [TestMethod]
        public async Task SendTest()
        {
            /// <summary>
            /// Fake server for testing
            /// </summary>
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

            async Task<int> InnerTestFunc(TimeSpan timeSpan, Socket socket, byte[] buffer, int response, int delay)
            {
                CancellationToken cancellationToken = default;
                using (var timeout = new CancellationTokenSource(timeSpan))
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        await Task.Delay(delay, cts.Token);
                        return await session.SendAsyncExtend(
                        socket,
                        buffer,
                        response,
                        cts.Token);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            /// <summary>
            /// Null buffer testing
            /// </summary>
            byte[] buffer = null;
            Socket socket = null;
            int response = -1;
            int delay = 0;
            TimeSpan timeSpan = TimeSpan.FromMinutes(1);
            try
            {
                var task = await InnerTestFunc(timeSpan, socket, buffer, response, delay);

            }
            catch (Exception e)
            {
                Console.WriteLine($"Null buffer testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is ArgumentNullException ex);
            }

            /// <summary>
            /// Null socket testing
            /// </summary>
            buffer = new byte[0];
            try
            {
                var task = await InnerTestFunc(timeSpan, socket, buffer, response, delay);

            }
            catch (Exception e)
            {
                Console.WriteLine($"Null socket testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is ArgumentNullException ex);
            }


            /// <summary>
            /// Negative response testing
            /// </summary>
            try
            {
                var task = await InnerTestFunc(timeSpan, FakeClient, buffer, response, delay);

            }
            catch (Exception e)
            {
                Console.WriteLine($"Negative parametr testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is ArgumentOutOfRangeException ex);
            }


            /// <summary>
            /// Out of range response testing
            /// </summary>
            response = 1;
            try
            {
                var task = await InnerTestFunc(timeSpan, FakeClient, buffer, response, delay);

            }
            catch (Exception e)
            {
                Console.WriteLine($"Out of range testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is ArgumentOutOfRangeException ex);
            }
            /// <summary>
            /// Timeout testing
            /// </summary>
            buffer = new byte[1];
            delay = 2000;
            try
            {
                var task = await InnerTestFunc(timeSpan, FakeClient, buffer, response, delay);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Timeout testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is OperationCanceledException socketException);
            }
            /// <summary>
            /// Socket is not connected testing
            /// </summary>
            try
            {
                var task = await InnerTestFunc(timeSpan, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                    buffer, response, delay);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Socket testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is SocketException socketException);
            }
            /// <summary>
            /// Normal data testing
            /// </summary>
            delay = 0;
            try
            {
                var task = await InnerTestFunc(timeSpan, FakeClient, buffer, response, delay);
                Assert.IsTrue(task == response);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Testing result: {e.GetType()} catched!");
            }
        }
        */

        [TestMethod]
        public async Task CommitTest()
        {
            async Task<int?> FirstFunct(CancellationToken cancellationToken = default)
            {
                int sum = 0;
                cancellationToken.ThrowIfCancellationRequested();
                return await Task.Run(() =>
                {
                    sum += 1;
                    return sum;
                });
            }
            async Task<int?> SecondFunct(CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(61000, cancellationToken);
                    int sum = 0;
                    return await Task.Run(() =>
                    {
                        sum += 1;
                        return sum;
                    });
            }
            Func<CancellationToken, Task<int?>> FirstAction = async cts => await FirstFunct(cts);
            Func<CancellationToken, Task<int?>> SecondAction = async cts => await SecondFunct(cts);
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Tunneling>();
            Tunneling session = new Tunneling(
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                logger);

            var FirstResult = await session.Commit(FirstAction);
            Assert.IsTrue(FirstResult.Value == 1);
            try
            {
                var SecondResult = await session.Commit(SecondAction);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is OperationCanceledException socketException);
            }
            
        }

        [TestMethod]
        public void StartTunnellingTest()
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
            byte[] testData = { 1, 2, 3, 4, 5 };
            byte[] receivedData = new byte[testData.Length];
            FakeClient.Send(testData);
            FakeProxy.Receive(receivedData);
            CollectionAssert.AreEqual(testData, receivedData);
        }

        [TestMethod]
        public void IdleTimeoutTest()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Tunneling>();
            Tunneling session = new Tunneling(
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                logger);
            session.IdleTimeout = TimeSpan.FromHours(2);
            Assert.IsTrue(session.IdleTimeout == TimeSpan.FromHours(2));
            try
            {
                session.IdleTimeout = TimeSpan.FromHours(2).Negate();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentOutOfRangeException socketException);
            }
        }

        /// <summary>
        /// To test Recive you have to change access modifiers
        /// </summary>
        /*
        [TestMethod]
        public async Task ReciveTest()
        {
            /// <summary>
            /// Fake server for testing
            /// </summary>
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

            byte[] buffer_for_send = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100 };
            async Task<int> InnerTestFunc(TimeSpan timeSpan, Socket socket, byte[] buffer, int delay, bool have_to_send = false)
            {
                if (have_to_send == true)
                {
                    
                    await Task.Run(async () => await FakeProxy.SendAsync(new ArraySegment<byte>(buffer_for_send, 0, buffer_for_send.Length), SocketFlags.None));
                }
                CancellationToken cancellationToken = default;
                using (var timeout = new CancellationTokenSource(timeSpan))
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        await Task.Delay(delay, cts.Token);
                        return await session.ReceiveAsyncExtend(
                        socket,
                        buffer,
                        cts.Token);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            /// <summary>
            /// Null buffer testing
            /// </summary>
            byte[] buffer = null;
            Socket socket = null;
            int delay = 0;
            TimeSpan timeSpan = TimeSpan.FromSeconds(1);
            try
            {
                var task = await InnerTestFunc(timeSpan, socket, buffer, delay);

            }
            catch (Exception e)
            {
                Console.WriteLine($"Null buffer testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is ArgumentNullException ex);
            }

            /// <summary>
            /// Null socket testing
            /// </summary>
            buffer = new byte[100];
            try
            {
                var task = await InnerTestFunc(timeSpan, socket, buffer, delay);

            }
            catch (Exception e)
            {
                Console.WriteLine($"Null socket testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is ArgumentNullException ex);
            }

            
            /// <summary>
            /// Timeout testing
            /// </summary>
            delay = 2000;
            try
            {
                var task = await InnerTestFunc(timeSpan, FakeClient, buffer, delay);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Timeout testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is OperationCanceledException socketException);
            }

            /// <summary>
            /// Socket is not connected testing
            /// </summary>
            delay = 0;
            try
            {
                var task = await InnerTestFunc(timeSpan, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                    buffer, delay);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Socket testing result: {e.GetType()} catched!");
                Assert.IsTrue(e is SocketException socketException);
            }
            
            bool test = true;
            buffer = new byte[0];
            /// <summary>
            /// Normal data testing with small buffer
            /// </summary>
            delay = 0;
            try
            {
                var task = await InnerTestFunc(timeSpan, FakeClient, buffer, delay, test);
                Assert.IsTrue(task == 0 && buffer.Length == 0);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Testing result: {e.GetType()} catched!");
            }
            
            /// <summary>
            /// Normal data testing
            /// </summary>
            delay = 0;
            buffer = new byte[100];
            try
            {
                var task = await InnerTestFunc(timeSpan, FakeClient, buffer, delay, test);
                CollectionAssert.AreEqual(buffer, buffer_for_send);
                Console.WriteLine($"Task result is: {task}\nBuffer lenght is:{buffer.Length}\nFirst symbol is:{buffer[0]}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Testing result: {e.GetType()} catched!");
            }
        }
        */
    }
}
