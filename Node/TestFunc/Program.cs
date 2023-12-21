

using TestFunc;

namespace Test
{
    class Program
    {
        static async Task Main()
        {
            /*TestFindAdress obj = new TestFindAdress();
            obj.TestFuncMain();
            Console.WriteLine($"Adresses: {obj.ListeningAddress1}, {obj.ListeningAddress2}, {obj.ListeningAddress1?.ToString() == obj.ListeningAddress2?.ToString()}");
            */
            //TestFuncList.TestFuncMain();
            //TestFuncSum.TestFuncMain();
            //TestFunClass testFunClass = new();
            //testFunClass.TestFuncMain();
            //TestClassInit testFunClass = new();
            //testFunClass.TestFuncMain();
            /*TestCycleAcceptUsage testCycleAcceptUsage = new TestCycleAcceptUsage();
            var newSocket = testCycleAcceptUsage.Start(8080);
            var newResult = testCycleAcceptUsage.EstimateFunctionMemoryUsage(newSocket);
            Console.WriteLine($"{testCycleAcceptUsage.OK()}, {testCycleAcceptUsage.sockets.Count}");

            TestCallBackAcceptUsage testCallBackAcceptUsage = new TestCallBackAcceptUsage();
            var socket = testCallBackAcceptUsage.Start(8081);
            var result = testCallBackAcceptUsage.EstimateFunctionMemoryUsage(socket);
            Console.WriteLine($"{testCallBackAcceptUsage.OK()}, {testCallBackAcceptUsage.sockets.Count}");

            Console.WriteLine($"Result: {result}, {newResult}");*/

            //TestLib.TestLibrary();
            Console.ReadLine();
        }
    }
}