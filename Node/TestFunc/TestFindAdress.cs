using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace Test
{
	public class TestFindAdress
	{
        public void TestFuncMain()
        {
            var result1 = Timeit(Test1);
            double timeElapsed = result1.Item1;
            Console.WriteLine($"First func: {timeElapsed}");

            var result2 = Timeit(Test2);
            timeElapsed = result2.Item1;
            Console.WriteLine($"Second func: {timeElapsed}");
            Console.WriteLine($"Result is equal: {result1.Item2 == result2.Item2}");
        }

        public Tuple<double, bool> Timeit(Func<bool> func)
        {
            var watch = Stopwatch.StartNew();
            var res = func();
            watch.Stop();
            return Tuple.Create(watch.Elapsed.TotalMilliseconds, res);
        }

        public IPAddress? ListeningAddress1;
        public IPAddress? ListeningAddress2;

        public bool Test1()
        {
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (IPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            if (addr.Address.GetAddressBytes()[0] == 2)
                            {
                                ListeningAddress1 = addr.Address;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool Test2()
        {
            var result = NetworkInterface.GetAllNetworkInterfaces().
                Where(m => m.OperationalStatus == OperationalStatus.Up).
                Select(m => m.GetIPProperties()).
                SelectMany(m => m.UnicastAddresses.Where(m => m.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).
                Select(m => m.Address).Where(m => m.GetAddressBytes()[0] == 2)).FirstOrDefault();
            if (result != null)
            {
                ListeningAddress2 = result;
                return true;
            }
            return false;
        }
    }
}

