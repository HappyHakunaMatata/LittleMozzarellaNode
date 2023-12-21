using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Certificate;
using Node.Certificate.Math;

namespace TestNode
{
    [TestClass]
    public class TrailingZeros32Test
	{
        Bits builder;

        public TrailingZeros32Test()
        {
            builder = new Bits();
            Input = Bits.deBruijn64;
        }

        [TestMethod]
        public void TrailingZeros32ForZero()
		{
            var res = builder.TrailingZeros32(0);
            Assert.AreEqual(res, 32);
        }
        [TestMethod]
        public void TrailingZeros32ForOne()
        {
            var res = builder.TrailingZeros32(1);
            Assert.AreEqual(res, 0);
        }
        [TestMethod]
        public void TrailingZeros32For32()
        {
            var res = builder.TrailingZeros32(32);
            Assert.AreEqual(res, 5);
        }
        [TestMethod]
        public void TrailingZeros32For64()
        {
            var res = builder.TrailingZeros32(64);
            Console.WriteLine(res);
            Assert.AreEqual(res, 6);
        }
        [TestMethod]
        public void TrailingZeros32For1000()
        {
            var res = builder.TrailingZeros32(1000);
            Assert.AreEqual(res, 3);
        }

        [TestMethod]
        public void TrailingZeros16ForZero()
        {
            var res = builder.TrailingZeros16(0);
            Assert.AreEqual(res, 16);
        }
        [TestMethod]
        public void TrailingZeros16ForOne()
        {
            var res = builder.TrailingZeros16(1);
            Assert.AreEqual(res, 0);
        }
        [TestMethod]
        public void TrailingZeros16For32()
        {
            var res = builder.TrailingZeros16(32);
            Assert.AreEqual(res, 5);
        }
        [TestMethod]
        public void TrailingZeros16For64()
        {
            var res = builder.TrailingZeros16(64);
            Console.WriteLine(res);
            Assert.AreEqual(res, 6);
        }
        [TestMethod]
        public void TrailingZeros16For1000()
        {
            var res = builder.TrailingZeros32(1000);
            Assert.AreEqual(res, 3);
        }


        /// <summary>
        /// Benchmark test
        /// </summary>
        UInt64 Input;
        int Output;

        public void BenchmarkTrailingZeros32(int n)
        {
            int s = 0;
            for (int i = 0; i < n; i++)
            {
                var arg = (uint)Input << (i % 32);
                s += builder.TrailingZeros32(arg);
            }
            Output = s;
        }

        [TestMethod]
        public void RunBenchmark()
        {
            BenchmarkTrailingZeros32(110767);
            Console.WriteLine(Output);
            Assert.AreNotEqual(Output, 0);
        }
    }
}

