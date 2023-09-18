using System;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

namespace Test
{
    public class TestFunClass
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

        public Tuple<double, int> Timeit(Func<int, int> func)
        {
            var val = 100000;
            var watch = Stopwatch.StartNew();
            var res = func(val);
            watch.Stop();
            return Tuple.Create(watch.Elapsed.TotalMilliseconds, res);
        }

        public int Test1(int val)
        {
            var res = Test1Class.sum(val);
            return res;
        }

        public int Test2(int val)
        {
            Test2Class test2Class = new Test2Class();
            return test2Class.sum(val);
        }

        public int TestCycl1(int val)
        {
            int res = 0;
            for (int i = 0; i < val; i++)
            {
                res = res + Test1Class.sum(val);
            }
            return res;
        }

        public int TestCycl2(int val)
        {
            int res = 0;
            Test2Class test2Class = new Test2Class();
            for (int i = 0; i < val; i++)
            {
                res = res + test2Class.sum(val);
            }
            return res;
        }

        private class Test1Class
        {
            public static int sum(int val)
            {
                int i = 0;
                while (i < val)
                {
                    i += 1;
                };
                return i;
            }
        }

        private class Test2Class
        {
            public int sum(int val)
            {
                int i = 0;
                while (i < val)
                {
                    i += 1;
                };
                return i;
            }
        }
    }
}

