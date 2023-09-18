using System;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using static Test.TestClassInit;

namespace Test
{
	public class TestClassInit
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

            result1 = Timeit(TestCycl1);
            timeElapsed = result1.Item1;
            Console.WriteLine($"First func: {timeElapsed}");

            result2 = Timeit(TestCycl1);
            timeElapsed = result2.Item1;
            Console.WriteLine($"Second func: {timeElapsed}");
            Console.WriteLine($"Result is equal: {result1.Item2 == result2.Item2}");
        }

        public Tuple<double, int> Timeit(Func<int?, int> func)
        {
            var val = 10000;
            var watch = Stopwatch.StartNew();
            var res = func(val);
            watch.Stop();
            return Tuple.Create(watch.Elapsed.TotalMilliseconds, res);
        }

        public int Test1(int? val)
        {
            var res = new TestClass()
            {
                field1 = 1,
                field2 = 2,
                field3 = "3",
                field4 = 4,
                field5 = '5'
            };
            return res.field1 + res.field2;
        }

        public int Test2(int? val)
        {
            var res = new TestStruct()
            {
                field1 = 1,
                field2 = 2,
                field3 = "3",
                field4 = 4,
                field5 = '5'
            };
            return res.field1 + res.field2;
        }

        public int TestCycl1(int? val)
        {
            int res = 0;
            for (int i = 0; i < val; i++)
            {
                var ob = new TestClass();
                res = ob.field2 + ob.field1;
            }
            return res;
        }

        public int TestCycl2(int? val)
        {
            int res = 0;
            for (int i = 0; i < val; i++)
            {
                var ob = new TestStruct();
                res = ob.field2 + ob.field1;
            }
            return res;
        }

        public class TestClass
        {
            public int field1;
            public int field2;
            public string field3;
            public byte field4;
            public char field5;
        }
        public struct TestStruct
        {
            public int field1;
            public int field2;
            public string field3;
            public byte field4;
            public char field5;
        }
    }
}

