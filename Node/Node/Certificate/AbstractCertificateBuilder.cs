using System.Security.Cryptography.X509Certificates;
using Node.Certificate.Models;
using Node.Certificate.Math;
using System.Numerics;
using Node.Certificate.Template;

namespace Node.Certificate
{
    abstract public class AbstractCertificateBuilder
    {
        protected readonly CertificateAuthorityConfig caConfig;
        private readonly Object _mutex;
        protected readonly CertificateSettings _certificateSettings;


        public AbstractCertificateBuilder(CertificateAuthorityConfig certificateAuthority, CertificateSettings certificateSettings)
        {
            _mutex = new Object();
            _certificateSettings = certificateSettings;
            caConfig = certificateAuthority;
        }


        public AbstractCertificateBuilder(CertificateAuthorityConfig certificateAuthority)
        {
            _mutex = new Object();
            _certificateSettings = new CertificateSettings(Models.CertificateType.CA);
            caConfig = certificateAuthority;
        }


        public AbstractCertificateBuilder(CertificateSettings certificateSettings)
        {
            _mutex = new Object();
            caConfig = new CertificateAuthorityConfig();
            _certificateSettings = certificateSettings;
        }


        public AbstractCertificateBuilder()
        {
            _mutex = new Object();
            _certificateSettings = new CertificateSettings(Models.CertificateType.CA);
            caConfig = new CertificateAuthorityConfig();
        }


        public abstract X509Certificate2 GetCertificate();
        public abstract GeneratedKeys GenerateKey(CancellationToken ctx, UInt16 minDifficulty = 8);
        public abstract byte[]? DoubleSHA256PublicKey(byte[] publickey);
        public abstract bool VerifyECDSASignatureWithoutHashing(byte[] key, byte[] data, byte[] signature);
        public abstract X509Certificate2 CreateSelfSignedCertificate(byte[] PrivKey, byte[] PubKey);
        public abstract void SaveKey(byte[] key);
        public abstract X509Certificate2Collection NewIdentity(X509Certificate2 Certificate, byte[] privateKey, CertificateTemplate template, out byte[]? privateEcdsaKey);
        public abstract CertificateTemplate CreateLeafTemplate();

        public async Task GenerateKeys(ushort minDifficulty = 8, ushort concurrency = 4, GenerateTypes generateTypes = GenerateTypes.Threads, CancellationToken cancellationToken = default)
        {
            switch (generateTypes)
            {
                case GenerateTypes.Tasks:
                    await GenerateKeys(minDifficulty, concurrency, cancellationToken);
                    break;
                case GenerateTypes.ParallelFor:
                    GenerateKeysParallelFor(minDifficulty, concurrency, cancellationToken);
                    break;
                case GenerateTypes.Threads:
                    GenerateKeysInThreads(minDifficulty, concurrency, cancellationToken);
                    break;
                case GenerateTypes.Single:
                    Console.WriteLine("Method is not suitable");
                    break;
            }
        }

        public void GenerateKeysParallelFor(ushort minDifficulty = 8, ushort concurrency = 4, CancellationToken cancellationToken = default)
        {
            using (var ctx = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var tasks = new List<Task>();
                Parallel.ForEach(Enumerable.Range(0, concurrency), _ =>
                {
                    while (true)
                    {
                        var k = GenerateKey(ctx.Token, minDifficulty: minDifficulty);
                        if (ctx.IsCancellationRequested)
                        {
                            break;
                        }
                        if (k.GetPublicKey() == null || k.GetId() == null || k.GetPrivateKey() == null)
                        {
                            ctx.Cancel();
                            break;
                        }
                        var done = FoundCallBack(k.GetPublicKey(), k.GetId(), k.GetPrivateKey());
                        if (done)
                        {
                            ctx.Cancel();
                            break;
                        }
                    }
                });
            }
        }

        public void GenerateKeysInThreads(ushort minDifficulty = 8, ushort concurrency = 4, CancellationToken cancellationToken = default)
        {
            using (var ctx = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                Thread[] threads = new Thread[concurrency];
                for (var i = 0; i < concurrency; i++)
                {
                    threads[i] = new Thread(() =>
                    {
                        while (true)
                        {
                            var k = GenerateKey(ctx.Token, minDifficulty: minDifficulty);
                            if (ctx.IsCancellationRequested)
                            {
                                break;
                            }
                            if (k.GetPublicKey() == null || k.GetId() == null || k.GetPrivateKey() == null)
                            {
                                ctx.Cancel();
                                break;
                            }
                            var done = FoundCallBack(k.GetPublicKey(), k.GetId(), k.GetPrivateKey());
                            if (done)
                            {
                                ctx.Cancel();
                                break;
                            }
                        }
                    });
                    threads[i].Start();
                }
                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }
        }

        //Foreach vs for + task ?
        private async Task GenerateKeys(ushort minDifficulty = 8, ushort concurrency = 4, CancellationToken cancellationToken = default)
        {
            using (var ctx = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var tasks = new List<Task>();
                for (var i = 0; i < concurrency; i++)
                {
                    var task = Task.Run(() =>
                    {
                        while (true)
                        {
                            var k = GenerateKey(ctx.Token, minDifficulty: minDifficulty);
                            if (ctx.IsCancellationRequested)
                            {
                                break;
                            }
                            if (k.GetPublicKey() == null || k.GetId() == null || k.GetPrivateKey() == null)
                            {
                                break;
                            }
                            var done = FoundCallBack(k.GetPublicKey(), k.GetId(), k.GetPrivateKey());
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
                try
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private volatile byte[]? _selectedKey;
        private volatile byte[]? _selectedID;
        private volatile int highscore;
        private int i = 0;
        public volatile byte[]? _pkey;


        private byte[] PublicKey
        {
            get
            {
                if (_selectedKey != null)
                {
                    return _selectedKey;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _selectedKey = value;
            }
        }


        public byte[] GetPublicKey()
        {
            try
            {
                return PublicKey;
            }
            catch
            {
                return new byte[] { };
            }
        }


        private byte[] ID
        {
            get
            {
                if (_selectedID != null)
                {
                    return _selectedID;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _selectedID = value;
            }
        }


        public byte[] GetID()
        {
            try
            {
                return ID;
            }
            catch
            {
                return new byte[] { };
            }
        }


        private byte[] PrivateKey
        {
            get
            {
                if (_pkey != null)
                {
                    return _pkey;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _pkey = value;
            }
        }

        public byte[] GetPrivate()
        {
            try
            {
                return PrivateKey;
            }
            catch
            {
                return new byte[] { };
            }
        }


        public FullCertificateAuthority GetCertificateAuthority()
        {
            return new FullCertificateAuthority()
            {
                Cert = GetCertificate(),
                NodeID = GetID(),
                PrivateKey = GetPrivate(),
            };
        }

        public bool FoundCallBack(byte[] PublicKey, byte[] ID, byte[] PrivateKey)
        {
            if (Interlocked.Add(ref i, 1) % 100 == 0)
            {
                updateStatus();
            }
            var difficulty = GetDifficulty(ID);

            if (difficulty >= caConfig.Difficulty)
            {
                lock (_mutex)
                {
                    if (_selectedKey == null)
                    {
                        updateStatus();
                        _selectedKey = PublicKey;
                        _selectedID = ID;
                        _pkey = PrivateKey;
                    }

                }
                Interlocked.Exchange(ref highscore, difficulty);
                updateStatus();
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
                    updateStatus();
                    return false;
                }
            }
        }


        public void updateStatus()
        {
            int count = Interlocked.CompareExchange(ref i, 0, 0);
            int hs = Interlocked.CompareExchange(ref highscore, 0, 0);
            Console.Write($"\rGenerated {count} keys; best difficulty so far: {hs}");
        }


        /// <summary>
        /// TODO: TEST
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UInt16 GetDifficulty(byte[] id)
        {
            var ArrayLenght = id.Length;
            byte? b;
            int? zeroBits;

            for (var i = 2; i <= ArrayLenght; i++)
            {
                b = id[ArrayLenght - i];
                if (b != 0)
                {
                    Bits bits = new();
                    zeroBits = bits.TrailingZeros16((UInt16)b);
                    if (zeroBits == 16)
                    {
                        // we already checked that b != 0.
                        return 0;

                    }
                    return (UInt16)((i - 1) * 8 + zeroBits);
                }
            }
            return 0;
        }


        public byte[] NewId(byte[] key)
        {
            //byte[] copy = new byte[caConfig.idArraySize];
            //Array.Copy(key, copy, caConfig.idArraySize);

            key[caConfig.idArraySize - 1] = caConfig.versionID;
            return key;
        }


        public BigInteger newSerialNumber()
        {
            BigInteger serialNumberLimit = new BigInteger(1) << 128;
            RandomBigInteger rnd = new RandomBigInteger();
            return rnd.NextBigInteger(0, serialNumberLimit);
        }


        public byte[] NodeIDFromKey(byte[] key)
        {
            var idBytes = DoubleSHA256PublicKey(key);
            idBytes = idBytes ?? new byte[caConfig.idArraySize];
            return NewId(idBytes);
        }


        public bool VerifySignatureWithoutHashing(byte[] key, byte[] data, byte[] signature)
        {
            //TODO: Make switch for RSA
            return VerifyECDSASignatureWithoutHashing(key, data, signature);
        }


        public void SaveCertificate(X509Certificate2 cert)
        {
            try
            {
                File.WriteAllText(_certificateSettings.CertPath, cert.ExportCertificatePem());
            }
            catch
            {

            }
        }

        public void SaveIdentity(X509Certificate2Collection collection)
        {
            try
            {
                File.WriteAllText(_certificateSettings.CertPath, collection.ExportCertificatePems());
            }
            catch
            {

            }
        }

        public ushort GetIDVersion(string ID)
        {
            int result = 0;
            if (string.IsNullOrEmpty(ID))
            {
                throw new ArgumentException("Input string is null or empty.");
            }

            char id = ID[0];

            if (int.TryParse(id.ToString(), out result))
            {
                return (ushort)result;
            }
            return (ushort)result;
        }

        public ushort IDVersionFromCert(X509Certificate2 certificate)
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
    }
}

