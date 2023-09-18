using System;
using System.Diagnostics;

namespace Test
{
	public class TestFuncSum
	{
        public static void TestFuncMain()
        {
            List<int> data = Enumerable.Range(1, 10).ToList();
            Console.WriteLine($"Array lenght: {data.Count}");
            var result = Timeit(Test1, data);
            double timeElapsed = result.Item1;
            Console.WriteLine($"First func: {timeElapsed}\t Counted: {result.Item2}");
            result = Timeit(Test2, data);
            timeElapsed = result.Item1;
            Console.WriteLine($"Second func: {timeElapsed}\t Counted: {result.Item2}");
            result = Timeit(Test3, data);
            timeElapsed = result.Item1;
            Console.WriteLine($"Third func: {timeElapsed}\t Counted: {result.Item2}");
            result = Timeit(Test4, data);
            timeElapsed = result.Item1;
            Console.WriteLine($"Fourth func: {timeElapsed}\t Counted: {result.Item2}");
            result = Timeit(Test5, data);
            timeElapsed = result.Item1;
            Console.WriteLine($"Fifth func: {timeElapsed}\t Counted: {result.Item2}");
        }

        public static Tuple<double, long> Timeit(Func<List<int>, long> func, List<int> args)
        {
            var watch = Stopwatch.StartNew();
            var res = func(args);
            watch.Stop();

            return Tuple.Create(watch.Elapsed.TotalMilliseconds, res);
        }

        public static long Test1(List<int> val)
        {
            long total = 0;
            foreach (int i in val)
            {
                total = DoSomeIndependentTimeconsumingTask();
            };
            return total;
        }

        public static long Test2(List<int> val)
        {
            long total = 0;
            Task.Run(() => {
                foreach (int i in val)
                {
                    total = DoSomeIndependentTimeconsumingTask();
                };
            }).Wait();
            return total;
        }

        public static long Test3(List<int> val)
        {
            long total = 0;
            Parallel.ForEach(val, i =>
            {
                total = DoSomeIndependentTimeconsumingTask();
            });
            return total;
        }

        public static long Test4(List<int> data)
        {
            long total = 0;
            for (int i = 0; i < data.Count; i++)
            {
                total = DoSomeIndependentTimeconsumingTask();
            }
            return total;
        }

        public static long Test5(List<int> data)
        {
            long total = 0;
            int mid = data.Count / 2;
            Task.Run(() => {
                for (int i = 0; i < mid; i++)
                {
                    total = DoSomeIndependentTimeconsumingTask();
                }
            }).Wait();
            Task.Run(() => {
                for (int i = mid; i < data.Count; i++)
                {
                    total = DoSomeIndependentTimeconsumingTask();
                }
            }).Wait();
            return total;
        }

        static long DoSomeIndependentTimeconsumingTask()
        {
            //Do Some Time Consuming Task here
            long total = 0;
            for (int i = 1; i < 1000000000; i++)
            {
                total += i;
            }
            return total;
        }
    }
}

