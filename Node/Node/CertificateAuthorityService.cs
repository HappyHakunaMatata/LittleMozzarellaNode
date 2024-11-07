using System;
using Common.Certificate;
using Common.Certificate.Models;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Common.Certificate.Interfaces;
using System.IO;
using Common.Extensions;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Node
{
	public class CertificateAuthorityService : IIdentityCreation
    {

        private CertificateDirector? director = null;

        private CertificateManager manager;

        private string? confDir;

        private string? caCertPath;

        private string? caKeyPath;

        private string? identCertPath;

        private string? identKeyPath;

        private CancellationTokenSource? cancellationTokenSource;

        private bool generating = false;


        public CertificateAuthorityService()
		{
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    director = new OSXCertificateDirector();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    director = new WinCertificateDirector(new CertificateSettings());
                }
                else
                {
                    throw new NotSupportedException();
                }
                manager = new CertificateManager();
            }
            catch
            {
                throw;
            }
        }


        public CertificateDirector Instance
        {
            get
            {
                if (director == null)
                {
                    throw new ArgumentException();
                }
                return director;
            }
            set
            {
                director = value;
            }
        }

        public async Task CreateFullCertificateAuthority(string path, CertificateSettings settings, AbstractCertificateBuilder builder)
        {
            try
            {
                if (generating)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Identity is already generating. Please cancel previous task");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                cancellationTokenSource = new CancellationTokenSource();
                generating = true;
                confDir = path;
                caCertPath = Path.Combine(confDir, "ca.cert");
                caKeyPath = Path.Combine(confDir, "ca.key");
                identCertPath = Path.Combine(confDir, "identity.cert");
                identKeyPath = Path.Combine(confDir, "identity.key");

                if (FilePath.IsExists(caCertPath, caKeyPath) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("CA certificate and/or key already exists, NOT overwriting!");
                    Console.WriteLine($"Files location : {confDir}");
                    return;
                }

                if (FilePath.IsExists(identCertPath, identKeyPath) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Identity certificate and/or key already exists, NOT overwriting!");
                    Console.WriteLine($"Files location : {confDir}");
                    return;
                }
                var authority = await Instance.GenerateFullCertificateAuthority(builder, settings, cancellationTokenSource.Token);
                if (authority == null)
                {
                    return;
                }
                var identity = Instance.CreateIdentity(builder, path, authority, settings);
                if (identity == null)
                {
                    return;
                }
                authority.SaveFullCertificateAuthority(path);
                identity.SaveFullIdentity(path);
                Console.Write("Unsigned identity is located in: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{confDir}\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Please *move* CA key to secure storage - it is only needed for identity management and isn't needed to run a storage node!");
                cancellationTokenSource.Cancel();
                generating = false;
            }
            catch
            {
                throw;
            }
        }

        public void LoadFullCertificateAuthority()
        {
            try
            {

                var CAisLoaded = manager.TryLoadFullCAConfig(out var certificateAuthority);
                var identityIsLoaded = manager.TryLoadIdentConfig(out var identity);
                if (!CAisLoaded || !identityIsLoaded)
                {
                    return;
                }
            }
            catch
            {
                throw;
            }
        }

        public bool? Cancel()
        {
            try
            {
                if (cancellationTokenSource == null)
                {
                    return null;
                }
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return true;
                }
                generating = false;
                cancellationTokenSource.Cancel();
                return true;
            }
            catch
            {
                throw;
            }
        }

        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                cancellationTokenSource?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


    }
}

