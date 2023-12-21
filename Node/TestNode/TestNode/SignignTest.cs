using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Certificate;
using Node.Certificate.Models;

namespace TestNode
{
    [TestClass]
    public class SignignTest
    {

        public SignignTest()
        {
            
            byte[] byteArray = new byte[2000];
            string result = Encoding.Default.GetString(byteArray);
            var test = new Test() { name = "longnulls", data = result };
            tests.Add(test);
        }

        List<Test> tests = new List<Test>() {
            new Test() {name="empty", data="0"},
            new Test() {name="single byte", data="C"},
        };

        [TestMethod]
        public void TestSigningAndVerifyingECDSA()
        {
            ConcreteSystemSecurityBuilder builder = new ConcreteSystemSecurityBuilder();
            foreach (var i in tests)
            {
                var privKey = builder.GeneratePrivateECDSAKey();
                Assert.IsNotNull(privKey);
                var pubKey = builder.GetPublicKeyFromPrivate(ref privKey, out _);
                Assert.IsNotNull(pubKey);

                var sig = builder.HashAndSign(privKey, Encoding.UTF8.GetBytes(i.data));
                var res = builder.HashAndVerifySignature(pubKey, Encoding.UTF8.GetBytes(i.data), sig);
                Assert.IsTrue(res);

                var sig2 = builder.HashAndSign(privKey, Encoding.UTF8.GetBytes(i.data));
                Assert.AreNotEqual(sig, sig2);

                sig = builder.SignWithoutHashing(privKey, Encoding.UTF8.GetBytes(i.data));
                res = builder.VerifySignatureWithoutHashing(pubKey, Encoding.UTF8.GetBytes(i.data), sig);
                Assert.IsTrue(res);

                sig2 = builder.SignWithoutHashing(privKey, Encoding.UTF8.GetBytes(i.data));
                Assert.AreNotEqual(sig, sig2);
            }
        }

        ConcreteSystemSecurityBuilder builder;
        [TestMethod]
        public void TestGenerateKey()
        {
            builder = new ConcreteSystemSecurityBuilder();
            List<byte[]> keys = new List<byte[]>() {
                new byte[] {48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 181, 106, 130, 159, 26, 55, 206, 215, 78, 24, 218, 158, 145, 0, 176, 171, 53, 172, 17, 21, 185, 151, 40, 164, 133, 76, 235, 3, 66, 77, 41, 235, 110, 96, 213, 88, 46, 82, 219, 208, 85, 108, 116, 158, 115, 242, 86, 181, 48, 112, 158, 122, 46, 204, 48, 205, 24, 9, 27, 111, 201, 54, 215, 244},
                new byte[] {48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 253, 80, 41, 4, 96, 187, 179, 241, 207, 62, 117, 124, 253, 145, 75, 149, 148, 206, 110, 207, 226, 246, 178, 68, 242, 71, 118, 82, 208, 126, 24, 58, 20, 104, 34, 64, 246, 232, 178, 71, 203, 51, 193, 223, 48, 229, 203, 241, 90, 99, 167, 51, 94, 254, 60, 63, 166, 90, 207, 120, 114, 128, 218, 39},
                new byte[] {48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 21, 240, 52, 204, 161, 138, 193, 189, 97, 214, 95, 244, 136, 156, 199, 66, 77, 93, 178, 96, 136, 94, 133, 126, 183, 20, 74, 251, 0, 220, 160, 227, 143, 252, 188, 193, 6, 67, 150, 115, 48, 87, 229, 125, 76, 227, 193, 5, 94, 147, 175, 239, 110, 70, 65, 12, 192, 3, 208, 143, 59, 14, 244, 25},
                new byte[] {48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 77, 189, 239, 164, 167, 119, 156, 93, 192, 219, 220, 212, 87, 182, 165, 19, 223, 183, 140, 105, 163, 40, 6, 11, 25, 38, 190, 67, 64, 142, 149, 153, 150, 95, 177, 135, 60, 97, 44, 118, 188, 29, 24, 62, 102, 62, 98, 2, 161, 243, 161, 131, 222, 235, 86, 40, 204, 174, 78, 9, 186, 77, 221, 201},
                new byte[] {48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 152, 222, 55, 80, 3, 230, 206, 188, 32, 85, 13, 72, 108, 143, 72, 138, 182, 170, 76, 216, 17, 108, 134, 127, 187, 117, 130, 170, 127, 215, 70, 224, 193, 121, 252, 136, 233, 214, 60, 56, 187, 21, 162, 62, 151, 112, 72, 18, 167, 241, 225, 39, 13, 32, 13, 210, 242, 98, 160, 236, 10, 115, 83, 189}
            };
            List<byte[]?> found_keys = new List<byte[]?>()
            {
                new byte[] {209, 44, 34, 200, 156, 35, 241, 46, 125, 189, 89, 118, 173, 63, 203, 212, 107, 68, 191, 41, 225, 191, 13, 164, 66, 196, 108, 47, 242, 97, 30, 0 },
                new byte[] {136, 185, 51, 80, 244, 80, 206, 7, 76, 46, 165, 170, 62, 134, 80, 41, 78, 175, 21, 216, 36, 209, 19, 232, 9, 168, 234, 193, 117, 163, 228, 0},
                new byte[] {226, 97, 208, 194, 59, 197, 144, 4, 87, 238, 183, 131, 138, 162, 56, 76, 140, 128, 59, 152, 247, 247, 137, 209, 233, 176, 188, 61, 27, 69, 158, 0},
                new byte[] {199, 174, 149, 94, 20, 187, 176, 239, 152, 177, 74, 227, 112, 54, 210, 179, 73, 125, 45, 66, 187, 178, 253, 91, 19, 211, 192, 30, 203, 107, 2, 0},
            };
            List<byte[]?> generated = new();
            foreach (var i in keys)
            {
                var res = GenerateKey(generatedkey: i);
                generated.Add(res);
            }
            Assert.IsTrue(generated.Count == keys.Count);
            for (var i = 0; i<found_keys.Count; i++)
            {
                var result = found_keys[i].SequenceEqual(generated[i]);
                Assert.IsTrue(result);
            }
        }


        public byte[]? GenerateKey(CancellationToken ctx = default, UInt16 minDifficulty = 8, byte[]? generatedkey = null)
        {
            UInt16 d;
            byte[]? id = null;
            while (true)
            {
                var pubKey = generatedkey;
                try
                {
                    if (ctx.IsCancellationRequested)
                    {
                        break;
                    }
                    if (pubKey == null)
                    {
                        break;
                    }
                    id = builder.NodeIDFromKey(pubKey);
                    d = builder.GetDifficulty(id);
                    if (d >= minDifficulty)
                    {

                        return id;
                    }
                }
                catch
                {
                    break;
                }
            }
            return id;
        }

        [TestMethod]
        public void TestFoundCallBack()
        {
            var _builder = new ConcreteSystemSecurityBuilder(new CertificateAuthorityConfig() {Difficulty=4});
            List<Tuple<byte[], byte[], bool>> test_values = new() {
                new Tuple<byte[], byte[], bool> (
                new byte[] { 48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 7, 227, 164, 99, 124, 30, 128, 190, 241, 196, 155, 24, 158, 239, 128, 239, 90, 40, 246, 121, 188, 194, 208, 6, 204, 72, 121, 191, 248, 248, 245, 204, 121, 23, 91, 93, 146, 60, 31, 231, 107, 92, 101, 207, 141, 43, 226, 176, 17, 49, 199, 2, 156, 146, 153, 27, 96, 219, 181, 30, 105, 229, 206, 140 },
                new byte[] { 78, 251, 17, 113, 157, 223, 21, 64, 248, 231, 16, 43, 8, 48, 214, 100, 166, 139, 55, 198, 94, 21, 22, 5, 50, 237, 3, 95, 76, 152, 223, 0 },
                true),
                new Tuple<byte[], byte[], bool> (
                new byte[] { 48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 165, 244, 193, 89, 84, 249, 188, 207, 184, 174, 170, 53, 119, 156, 70, 73, 134, 83, 112, 4, 159, 60, 146, 145, 157, 221, 140, 153, 84, 152, 227, 72, 122, 26, 49, 25, 246, 15, 78, 230, 200, 63, 97, 129, 103, 255, 30, 158, 94, 42, 213, 225, 102, 172, 214, 11, 72, 42, 93, 16, 255, 155, 133, 148},
                new byte[] { 40, 52, 99, 48, 98, 87, 161, 66, 68, 110, 243, 253, 178, 33, 246, 173, 83, 108, 105, 227, 56, 251, 85, 126, 54, 153, 103, 190, 135, 238, 196, 0},
                true),
                new Tuple<byte[], byte[], bool> (
                new byte[] { 48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 253, 249, 241, 18, 67, 149, 181, 84, 70, 76, 245, 28, 6, 43, 27, 188, 23, 141, 144, 8, 138, 167, 246, 34, 226, 184, 130, 164, 103, 107, 193, 16, 226, 107, 154, 236, 167, 151, 138, 15, 191, 23, 176, 191, 30, 93, 2, 235, 246, 134, 129, 46, 133, 79, 130, 40, 41, 201, 111, 187, 197, 124, 201, 54},
                new byte[] { 211, 156, 13, 69, 146, 117, 197, 104, 148, 7, 39, 192, 169, 173, 224, 168, 20, 244, 141, 83, 66, 114, 23, 169, 106, 206, 19, 59, 218, 75, 222, 0},
                true),
                new Tuple<byte[], byte[], bool> (
                new byte[] { 48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 180, 147, 237, 118, 107, 130, 181, 110, 113, 254, 240, 30, 96, 101, 85, 183, 162, 202, 244, 218, 164, 34, 27, 214, 239, 177, 24, 21, 234, 130, 26, 178, 7, 178, 22, 104, 64, 183, 40, 86, 65, 210, 233, 128, 11, 170, 236, 176, 231, 39, 176, 112, 162, 0, 85, 98, 7, 130, 68, 147, 195, 249, 22, 196},
                new byte[] { 210, 240, 151, 73, 238, 77, 89, 9, 240, 154, 25, 252, 140, 65, 30, 85, 140, 25, 87, 139, 31, 246, 155, 87, 37, 82, 207, 85, 167, 17, 184, 0},
                true),
                new Tuple<byte[], byte[], bool> (
                new byte[] { 48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 97, 211, 249, 165, 186, 49, 170, 89, 158, 104, 174, 242, 216, 70, 102, 50, 61, 250, 98, 175, 158, 19, 106, 225, 31, 14, 235, 22, 40, 225, 155, 218, 18, 62, 5, 121, 137, 119, 224, 207, 49, 218, 125, 160, 239, 138, 44, 144, 156, 176, 166, 166, 128, 32, 5, 208, 249, 255, 52, 176, 50, 67, 133, 59},
                new byte[] { 217, 246, 15, 111, 117, 20, 186, 50, 153, 166, 61, 4, 197, 113, 146, 50, 186, 100, 254, 129, 156, 126, 82, 150, 115, 218, 123, 127, 76, 151, 204, 0 },
                true)
            };
            foreach (var i in test_values)
            {
                var result = _builder.FoundCallBack(i.Item1, i.Item2, null);
                Assert.AreEqual(result, i.Item3);
            }

            _builder = new ConcreteSystemSecurityBuilder(new CertificateAuthorityConfig() { Difficulty = 14 });
            test_values = new() {
                new Tuple<byte[], byte[], bool> (
                new byte[] { 48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 217, 19, 192, 64, 70, 231, 210, 172, 94, 25, 155, 132, 179, 30, 108, 56, 84, 121, 129, 167, 169, 226, 237, 223, 105, 123, 145, 190, 84, 12, 153, 86, 110, 39, 132, 107, 53, 105, 54, 136, 135, 8, 24, 42, 159, 93, 234, 132, 50, 47, 74, 102, 250, 151, 43, 167, 220, 55, 136, 146, 127, 27, 74, 171},
                new byte[] { 94, 118, 131, 206, 44, 79, 36, 48, 248, 46, 224, 19, 138, 228, 102, 15, 102, 165, 254, 141, 254, 205, 35, 96, 65, 42, 184, 243, 139, 56, 42, 0},
                false),
                new Tuple<byte[], byte[], bool> (
                new byte[] { 48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 129, 246, 103, 206, 61, 92, 62, 82, 233, 117, 206, 119, 116, 129, 112, 112, 117, 90, 159, 15, 248, 137, 218, 46, 202, 6, 166, 168, 98, 16, 28, 1, 176, 255, 44, 56, 35, 82, 202, 203, 78, 254, 166, 42, 223, 166, 25, 32, 111, 131, 251, 187, 47, 76, 121, 55, 232, 72, 19, 135, 173, 120, 241, 171},
                new byte[] { 215, 91, 199, 48, 157, 143, 100, 175, 112, 226, 55, 95, 4, 130, 184, 158, 225, 174, 185, 244, 219, 146, 155, 199, 88, 108, 189, 230, 243, 210, 246, 0},
                false),
                new Tuple<byte[], byte[], bool> (
                new byte[] { 48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 154, 116, 87, 97, 108, 209, 178, 211, 198, 24, 164, 218, 90, 171, 112, 227, 229, 91, 125, 111, 208, 6, 74, 76, 248, 76, 128, 14, 56, 45, 103, 156, 247, 222, 118, 180, 51, 250, 141, 246, 116, 132, 177, 164, 90, 234, 249, 207, 210, 58, 90, 169, 66, 106, 144, 86, 0, 196, 226, 75, 31, 138, 206, 37},
                new byte[] { 191, 210, 160, 79, 59, 229, 172, 3, 182, 173, 55, 231, 167, 89, 198, 169, 155, 56, 20, 140, 34, 230, 240, 95, 78, 44, 91, 19, 122, 161, 200, 0},
                false),
                new Tuple<byte[], byte[], bool> (
                new byte[] {48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 29, 159, 209, 17, 126, 16, 80, 239, 228, 138, 100, 240, 77, 92, 137, 206, 223, 197, 226, 237, 69, 95, 21, 234, 112, 66, 109, 174, 106, 76, 219, 45, 199, 112, 198, 208, 157, 121, 13, 229, 130, 27, 179, 40, 73, 203, 204, 217, 145, 85, 158, 34, 230, 129, 246, 50, 159, 56, 215, 219, 79, 71, 54, 85},
                new byte[] { 49, 12, 196, 56, 110, 57, 245, 64, 58, 110, 144, 129, 162, 180, 237, 120, 84, 213, 8, 199, 204, 112, 221, 149, 250, 48, 115, 81, 242, 202, 238, 0},
                false),
                new Tuple<byte[], byte[], bool> (
                new byte[] {48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 197, 218, 167, 129, 115, 160, 114, 23, 55, 63, 27, 157, 239, 0, 129, 30, 174, 233, 205, 144, 66, 202, 200, 123, 94, 249, 79, 24, 234, 105, 109, 4, 64, 105, 232, 4, 22, 117, 124, 184, 164, 206, 184, 140, 4, 48, 194, 52, 156, 157, 133, 74, 90, 170, 38, 108, 20, 118, 92, 145, 158, 140, 94, 110},
                new byte[] {17, 186, 193, 189, 76, 137, 158, 217, 221, 139, 17, 155, 142, 53, 66, 46, 78, 86, 137, 195, 46, 254, 176, 171, 244, 172, 89, 46, 32, 147, 124, 0},
                false),
                new Tuple<byte[], byte[], bool> (
                new byte[] {48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 140, 153, 5, 58, 134, 66, 98, 221, 8, 130, 127, 247, 99, 5, 134, 109, 158, 203, 123, 234, 28, 95, 180, 206, 169, 158, 142, 178, 43, 104, 31, 149, 28, 138, 109, 95, 28, 99, 40, 227, 156, 238, 216, 234, 46, 74, 240, 221, 43, 168, 125, 51, 242, 124, 15, 236, 126, 53, 203, 159, 245, 57, 140, 100},
                new byte[] {242, 62, 252, 23, 215, 96, 183, 125, 227, 17, 60, 29, 167, 35, 53, 212, 87, 164, 13, 38, 80, 62, 196, 146, 29, 52, 43, 189, 43, 97, 64, 0},
                true),
                new Tuple<byte[], byte[], bool> (
                new byte[] {48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 115, 68, 198, 129, 61, 244, 144, 68, 239, 218, 92, 127, 244, 127, 62, 47, 215, 108, 145, 8, 53, 30, 90, 1, 213, 22, 21, 180, 134, 150, 56, 252, 6, 229, 246, 146, 246, 7, 109, 45, 7, 60, 203, 179, 146, 46, 135, 194, 237, 231, 201, 7, 228, 190, 189, 70, 109, 252, 95, 17, 211, 1, 237, 65},
                new byte[] {189, 79, 242, 167, 222, 136, 78, 22, 15, 99, 68, 35, 240, 189, 91, 32, 162, 69, 41, 2, 40, 151, 101, 130, 205, 229, 145, 106, 88, 44, 200, 0},
                false)
            };
            foreach (var i in test_values)
            {
                var result = _builder.FoundCallBack(i.Item1, i.Item2, null);
                Assert.AreEqual(result, i.Item3);
            }
        }


        [TestMethod]
        public async Task TestGenerateKeys()
        {
            ConcreteSystemSecurityBuilder _builder = new ConcreteSystemSecurityBuilder(new CertificateAuthorityConfig() { Difficulty = 18});
            await _builder.GenerateKeys();
            var ID = _builder.GetID();
            var KEY = _builder.GetPublicKey;

            Assert.IsTrue(KEY != null);
            Assert.IsTrue(ID != null);
            Assert.IsTrue(ID.Length == 32);
            Assert.IsTrue(ID[ID.Length - 1] == 0);
            Assert.IsTrue(_builder.GetDifficulty(ID) >= 18);
        }

        [TestMethod]
        public void TestNewSerialNumber()
        {
            ConcreteSystemSecurityBuilder _builder = new ConcreteSystemSecurityBuilder();
            var serial = _builder.newSerialNumber();
            Assert.IsTrue(serial.ToByteArray().Length <= 17);
        }

        [TestMethod]
        public void TestCreateSelfSignedCertificate()
        {
            ConcreteSystemSecurityBuilder _builder = new ConcreteSystemSecurityBuilder();
            byte[] pkey = new byte[] { 48, 119, 2, 1, 1, 4, 32, 100, 90, 59, 79, 2, 20, 210, 105, 223, 201, 209, 128, 75, 147, 60, 214, 255, 221, 21, 236, 66, 228, 107, 72, 146, 135, 135, 124, 0, 250, 228, 95, 160, 10, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 161, 68, 3, 66, 0, 4, 189, 248, 18, 33, 6, 33, 54, 163, 44, 196, 104, 160, 254, 238, 106, 39, 123, 254, 151, 91, 150, 224, 24, 95, 6, 249, 146, 174, 243, 130, 194, 111, 167, 22, 77, 109, 45, 26, 212, 235, 160, 86, 46, 7, 121, 55, 0, 150, 114, 190, 223, 188, 11, 135, 96, 214, 236, 13, 194, 80, 91, 125, 53, 15 };
            byte[] key = new byte[] { 48, 89, 48, 19, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 8, 42, 134, 72, 206, 61, 3, 1, 7, 3, 66, 0, 4, 189, 248, 18, 33, 6, 33, 54, 163, 44, 196, 104, 160, 254, 238, 106, 39, 123, 254, 151, 91, 150, 224, 24, 95, 6, 249, 146, 174, 243, 130, 194, 111, 167, 22, 77, 109, 45, 26, 212, 235, 160, 86, 46, 7, 121, 55, 0, 150, 114, 190, 223, 188, 11, 135, 96, 214, 236, 13, 194, 80, 91, 125, 53, 15 };
            var result = _builder.CreateSelfSignedCertificate(pkey, key);
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.GetKeyAlgorithm() == "1.2.840.10045.2.1");
            var AreEquals = key.SequenceEqual(result.PublicKey.ExportSubjectPublicKeyInfo());
            Assert.IsTrue(AreEquals);
            DateTime date = new DateTime(0001, 01, 01, 01, 00, 00);
            Assert.IsTrue(result.NotAfter == date);
            Assert.IsTrue(result.NotBefore == date);
            Assert.IsTrue(Regex.IsMatch(result.SubjectName.Name, "O=LittleMozzarella"));
        }

    }


    public class Test
	{
		public string? name { get; set; }
		public string? data { get; set; }
	}
}

