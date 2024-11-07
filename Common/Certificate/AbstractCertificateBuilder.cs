using System.Security.Cryptography.X509Certificates;
using Common.Certificate.Models;
using Common.Certificate.Math;
using System.Numerics;
using Common.Certificate.Enums;
using Org.BouncyCastle.Asn1.X9;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Common.Certificate
{
    abstract public class AbstractCertificateBuilder
    {


        #region Constructors


        public AbstractCertificateBuilder()
        {
            _mutex = new Object();
        }

        #endregion
        #region Parameters
        private readonly Object _mutex;
        private volatile byte[]? _selectedKey;
        private volatile byte[]? _selectedID;
        private volatile int highscore;
        private volatile int i = 0;
        public volatile byte[]? _pkey;
        #endregion
        #region Abstracts
        public abstract GeneratedKeys? GenerateKey(CancellationToken ctx, UInt16 minDifficulty = 8);
        public abstract byte[] DoubleSHA256PublicKey(byte[] publickey);
        public abstract bool VerifyECDSASignatureWithoutHashing(byte[] key, byte[] data, byte[] signature);
        public abstract X509Certificate2 CreateSelfSignedCertificate(byte[] PrivKey, byte[] PubKey, CertificateSettings settings);
        //public abstract void SaveKey(byte[] key, CertificateType type, string path);
        public abstract FullIdentity NewIdentity(X509Certificate2 Certificate, byte[] privateKey, CertificateTemplate template, out byte[]? privateEcdsaKey);
        public abstract CertificateTemplate CreateLeafTemplate(CertificateSettings settings);
        public abstract X509Certificate2 SignCertificate(X509Certificate2 issuer, byte[] privateKey, byte[] publicKey, X509Certificate2 template, out byte[] NewPrivateKey);
        #endregion
        #region Properties
        public byte[]? PublicKey
        {
            get
            {
                return _selectedKey;
            }
            set
            {
                _selectedKey = value;
            }
        }

        public byte[]? ID
        {
            get
            {
                return _selectedID;
            }
            set
            {
                _selectedID = value;
            }
        }

        public byte[]? PrivateKey
        {
            get
            {
                return _pkey;
            }
            set
            {
                _pkey = value;
            }
        }
        #endregion


        


        public async Task GenerateKeys(ushort minDifficulty = 8, ushort concurrency = 4, CancellationToken cancellationToken = default, UInt16 Difficulty = 36)
        {
            try
            {
                using (var ctx = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    var tasks = new List<Task>();
                    for (var i = 0; i < concurrency; i++)
                    {
                        var index = i + 1;
                        var task = Task.Run(() =>
                        {
                            while (true)
                            {
                                var k = GenerateKey(ctx.Token, minDifficulty: minDifficulty);
                                if (ctx.IsCancellationRequested)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine($"Task {index} has been canceled");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    break;
                                }
                                if (k == null)
                                {
                                    break;
                                }
                                var done = FoundCallBack(k.GetPublicKey(), k.GetId(), k.GetPrivateKey(), Difficulty);
                                if (done)
                                {
                                    break;
                                }
                            }
                        });
                        tasks.Add(task);
                    }
                    var firstCompletedTask = await Task.WhenAny(tasks);
                    ctx.Cancel();
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }
            catch
            {
                throw;
            }
        }


        public bool FoundCallBack(byte[] PublicKey, byte[] ID, byte[] PrivateKey, UInt16 Difficulty = 36)
        {
            ArgumentNullException.ThrowIfNull(PublicKey);
            ArgumentNullException.ThrowIfNull(ID);
            ArgumentNullException.ThrowIfNull(PrivateKey);
            try
            {
                if (Interlocked.Add(ref i, 1) % 100 == 0)
                {
                    UpdateStatus();
                }
                var difficulty = GetDifficulty(ID);
                if (difficulty >= Difficulty)
                {
                    lock (_mutex)
                    {
                        if (_selectedKey == null)
                        {
                            UpdateStatus();
                            _selectedKey = PublicKey;
                            _selectedID = ID;
                            _pkey = PrivateKey;
                        }

                    }
                    Interlocked.Exchange(ref highscore, difficulty);
                    UpdateStatus();
                    Console.Write($"\r\nFound a key with difficulty {difficulty}!\n");
                    return true;

                }
                while (true)
                {
                    int hs = Interlocked.CompareExchange(ref highscore, 0, 0);

                    if (difficulty <= hs)
                    {
                        return false;
                    }

                    if (Interlocked.CompareExchange(ref highscore, difficulty, hs) == hs)
                    {
                        UpdateStatus();
                        return false;
                    }
                }
            }
            catch
            {
                throw;
            }
        }


        public void UpdateStatus()
        {
            try
            {
                int count = Interlocked.CompareExchange(ref i, 0, 0);
                int hs = Interlocked.CompareExchange(ref highscore, 0, 0);
                Console.Write($"\rGenerated {count} keys; best difficulty so far: {hs}");
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UInt16 GetDifficulty(byte[] id)
        {
            ArgumentNullException.ThrowIfNull(id);
            var ArrayLenght = id.Length;
            byte? b;
            int? zeroBits;
            try
            {
                for (var i = 2; i <= ArrayLenght; i++)
                {
                    b = id[ArrayLenght - i];
                    if (b != 0)
                    {
                        Bits bits = new();
                        zeroBits = bits.TrailingZeros16((UInt16)b);
                        if (zeroBits == 16)
                        {
                            return 0;

                        }
                        return (UInt16)((i - 1) * 8 + zeroBits);
                    }
                }
                return 0;
            }
            catch
            {
                throw;
            }
        }


        public byte[] NewId(byte[] key, byte versionID = 0)
        {
            ArgumentNullException.ThrowIfNull(key);
            try
            {
                key[key.Length - 1] = versionID;
                return key;
            }
            catch
            {
                throw;
            }
        }


        public BigInteger NewSerialNumber()
        {
            try
            {
                BigInteger serialNumberLimit = new BigInteger(1) << 128;
                RandomBigInteger rnd = new RandomBigInteger();
                return rnd.NextBigInteger(0, serialNumberLimit);
            }
            catch
            {
                throw;
            }
        }


        public byte[] NodeIDFromKey(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);
            try
            {
                var idBytes = DoubleSHA256PublicKey(key);
                return NewId(idBytes);
            }
            catch
            {
                throw;
            }
        }


        public bool VerifySignatureWithoutHashing(byte[] key, byte[] data, byte[] signature)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(signature);
            try
            {
                //TODO: Make switch for RSA
                return VerifyECDSASignatureWithoutHashing(key, data, signature);
            }
            catch
            {
                throw;
            }
        }


        /*public void SaveCertificate(X509Certificate2 cert, string path)
        {
            ArgumentNullException.ThrowIfNull(cert);
            try
            {
                File.WriteAllText(path, cert.ExportCertificatePem());
            }
            catch
            {
                throw;
            }
        }

        public void SaveIdentity(X509Certificate2Collection collection, string path)
        {
            ArgumentNullException.ThrowIfNull(collection);
            try
            {
                File.WriteAllText(path, collection.ExportCertificatePems());
            }
            catch
            {
                throw;
            }
        }*/

        public ushort GetIDVersion(string ID)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(ID);
            try
            {
                char id = ID[0];
                if (int.TryParse(id.ToString(), out var result))
                {
                    return (ushort)result;
                }
                return (ushort)result;
            }
            catch
            {
                throw;
            }
        }

        public ushort IDVersionFromCert(X509Certificate2 certificate)
        {
            ArgumentNullException.ThrowIfNull(certificate);
            try
            {
                foreach (X509Extension ext in certificate.Extensions)
                {
                    if (ext.Oid != null && ext.Oid.Value == "2.999.2.1")
                    {
                        return GetIDVersion(ext.Oid.Value);
                    }
                }
                return 0;
            }
            catch
            {
                throw;
            }
        }
    }
}

