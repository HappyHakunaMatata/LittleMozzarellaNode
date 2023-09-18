using System;
using System.Diagnostics;

namespace Test
{
    public class TestFuncList
    {
        public static void TestFuncMain()
        {
            Random random = new Random();
            List<int> data = new List<int>();
            while (data.Count < 100000000)
            {
                data.Add(random.Next(-10, 10));
            }
            Console.WriteLine($"Array lenght: {data.Count}");
            var result = Timeit(Test1, data);
            double timeElapsed = result.Item1;
            Console.WriteLine($"First func: {timeElapsed}\t Zero found: {result.Item2}");
            result = Timeit(Test2, data);
            timeElapsed = result.Item1;
            Console.WriteLine($"Second func: {timeElapsed}\t Zero found: {result.Item2}");

            result = Timeit(Test3, data);
            timeElapsed = result.Item1;
            Console.WriteLine($"Third func: {timeElapsed}\t Zero found: {result.Item2}");
            result = Timeit(Test4, data);
            timeElapsed = result.Item1;
            Console.WriteLine($"Fourth func: {timeElapsed}\t Zero found: {result.Item2}");
            result = Timeit(Test5, data);
            timeElapsed = result.Item1;
            Console.WriteLine($"Fifth func: {timeElapsed}\t Zero found: {result.Item2}");
            result = Timeit(Test6, data);
            timeElapsed = result.Item1;
            Console.WriteLine($"Sixth func: {timeElapsed}\t Zero found: {result.Item2}");
        }

        public static Tuple<double, int> Timeit(Func<List<int>, int> func, List<int> args)
        {
            var watch = Stopwatch.StartNew();
            var res = func(args);
            watch.Stop();

            return Tuple.Create(watch.Elapsed.TotalMilliseconds, res);
        }

        public static int Test1(List<int> data)
        {
            int k = 0;
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] == 0)
                {
                    k += 1;
                }
            }
            return k;
        }

        public static int Test2(List<int> data)
        {
            int k = 0;
            Task.Run(() => {
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i] == 0)
                    {
                        k += 1;
                    }
                }
            }).Wait();
            return k;
        }

        public static int Test3(List<int> data)
        {
            int k = 0;
            Parallel.ForEach(data, data =>
            {
                if (data == 0)
                {
                    Interlocked.Increment(ref k);
                }
            });
            return k;
        }

        public static int Test4(List<int> data)
        {
            data.Sort();
            int k = 0;
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] == 0)
                {
                    k += 1;
                }
                else if(data[i] > 0)
                {
                    break;
                }
            }
            return k;
        }

        public static int Test5(List<int> data)
        {
            return data.Where(m => m == 0).Count();
        }

        public static int Test6(List<int> data)
        {
            int k = 0;
            int mid = data.Count / 2;
            Task.Run(() => {
                for (int i = 0; i < mid; i++)
                {
                    if (data[i] == 0)
                    {
                        k += 1;
                    }
                }
            }).Wait();
            Task.Run(() => {
                for (int i = mid; i < data.Count; i++)
                {
                    if (data[i] == 0)
                    {
                        k += 1;
                    }
                }
            }).Wait();
            return k;
        }
    }
}

