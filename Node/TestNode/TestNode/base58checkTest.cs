using System;
using TokenCreation.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TokenCreation.Model;

namespace TestNode
{
    [TestClass]
    public class base58checkTest
	{
        [TestMethod]
        public void TestBase58()
        {
            var encoder = new Base58();
            var result = encoder.Encode(new byte[] { });
            Assert.AreEqual("", result);
            result = encoder.Encode(new byte[] { 32 });
            Assert.AreEqual("Z", result);
            result = encoder.Encode(new byte[] { 45 });
            Assert.AreEqual("n", result);
            result = encoder.Encode(new byte[] { 48 });
            Assert.AreEqual("q", result);
            result = encoder.Encode(new byte[] { 49 });
            Assert.AreEqual("r", result);
            result = encoder.Encode(new byte[] { 45, 49 });
            Assert.AreEqual("4SU", result);
            result = encoder.Encode(new byte[] { 49, 49 });
            Assert.AreEqual("4k8", result);
            result = encoder.Encode(new byte[] { 97, 98, 99 });
            Assert.AreEqual("ZiCa", result);
            result = encoder.Encode(new byte[] { 49, 50, 51, 52, 53, 57, 56, 55, 54, 48 });
            Assert.AreEqual("3mJr7AoUXx2Wqd", result);
            result = encoder.Encode(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122 });
            Assert.AreEqual("3yxU3u1igY8WkgtjK92fbJQCd4BZiiT1v25f", result);
            result = encoder.Encode(new byte[] { 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48 });
            Assert.AreEqual("3sN2THZeE9Eh9eYrwkvZqNstbHGvrxSAM7gXUXvyFQP8XvQLUqNCS27icwUeDT7ckHm4FUHM2mTVh1vbLmk7y", result);
        }

        [TestMethod]
        public void TestBase58Check()
        {
            var encoder = new Base58();
            var result = encoder.CheckEncode(new byte[] { },20);
            Assert.AreEqual("3MNQE1X", result);
            result = encoder.CheckEncode(new byte[] { 32 }, 20);
            Assert.AreEqual("B2Kr6dBE", result);
            result = encoder.CheckEncode(new byte[] { 45 }, 20);
            Assert.AreEqual("B3jv1Aft", result);
            result = encoder.CheckEncode(new byte[] { 48 }, 20);
            Assert.AreEqual("B482yuaX", result);
            result = encoder.CheckEncode(new byte[] { 49 }, 20);
            Assert.AreEqual("B4CmeGAC", result);
            result = encoder.CheckEncode(new byte[] { 45, 49 }, 20);
            Assert.AreEqual("mM7eUf6kB", result);
            result = encoder.CheckEncode(new byte[] { 49, 49 }, 20);
            Assert.AreEqual("mP7BMTDVH", result);
            result = encoder.CheckEncode(new byte[] { 97, 98, 99 }, 20);
            Assert.AreEqual("4QiVtDjUdeq", result);
            result = encoder.CheckEncode(new byte[] { 49, 50, 51, 52, 53, 57, 56, 55, 54, 48 }, 20);
            Assert.AreEqual("ZmNb8uQn5zvnUohNCEPP", result);
            result = encoder.CheckEncode(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122 }, 20);
            Assert.AreEqual("K2RYDcKfupxwXdWhSAxQPCeiULntKm63UXyx5MvEH2", result);
            result = encoder.CheckEncode(new byte[] { 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48 }, 20);
            Assert.AreEqual("bi1EWXwJay2udZVxLJozuTb8Meg4W9c6xnmJaRDjg6pri5MBAxb9XwrpQXbtnqEoRV5U2pixnFfwyXC8tRAVC8XxnjK", result);
        }

        [TestMethod]
        public void TestBase58Decode()
        {
            var encoder = new Base58();
            var result = encoder.Decode("");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { }, result));
            result = encoder.Decode("Z");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 32 }, result));
            result = encoder.Decode("n");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 45 }, result));
            result = encoder.Decode("q");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 48 }, result));
            result = encoder.Decode("r");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 49 }, result));
            result = encoder.Decode("4SU");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 45, 49 }, result));
            result = encoder.Decode("4k8");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 49, 49 }, result));
            result = encoder.Decode("ZiCa");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 97, 98, 99 }, result));
            result = encoder.Decode("3mJr7AoUXx2Wqd");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 49, 50, 51, 52, 53, 57, 56, 55, 54, 48 }, result));
            result = encoder.Decode("3yxU3u1igY8WkgtjK92fbJQCd4BZiiT1v25f");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122 }, result));
            result = encoder.Decode("3sN2THZeE9Eh9eYrwkvZqNstbHGvrxSAM7gXUXvyFQP8XvQLUqNCS27icwUeDT7ckHm4FUHM2mTVh1vbLmk7y");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48 }, result));
            }
        
        [TestMethod]
        public void TestBase58DecodeCheck()
        {
            var encoder = new Base58();
            var result = encoder.CheckDecode("3MNQE1X");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { }, result));
            result = encoder.CheckDecode("B2Kr6dBE");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 32 }, result));
            result = encoder.CheckDecode("B3jv1Aft");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 45 }, result));
            result = encoder.CheckDecode("B482yuaX");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 48 }, result));
            result = encoder.CheckDecode("B4CmeGAC");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 49 }, result));
            result = encoder.CheckDecode("mM7eUf6kB");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 45, 49 }, result));
            result = encoder.CheckDecode("mP7BMTDVH");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 49, 49 }, result));
            result = encoder.CheckDecode("4QiVtDjUdeq");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 97, 98, 99 }, result));
            result = encoder.CheckDecode("ZmNb8uQn5zvnUohNCEPP");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 49, 50, 51, 52, 53, 57, 56, 55, 54, 48 }, result));
            result = encoder.CheckDecode("K2RYDcKfupxwXdWhSAxQPCeiULntKm63UXyx5MvEH2");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122 }, result));
            result = encoder.CheckDecode("bi1EWXwJay2udZVxLJozuTb8Meg4W9c6xnmJaRDjg6pri5MBAxb9XwrpQXbtnqEoRV5U2pixnFfwyXC8tRAVC8XxnjK");
            Assert.IsTrue(result != null && Enumerable.SequenceEqual(new byte[] { 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48 }, result));
        }
        

    }
}

