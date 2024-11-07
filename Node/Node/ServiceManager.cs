using System;
using Common.Certificate;
using Common.Certificate.Interfaces;
using Common.Certificate.Models;
using Microsoft.Extensions.Logging;
using Node.Interfaces;

namespace Node
{
	public class ServiceManager
	{
        protected readonly ILogger _logger;
        public ServiceManager()
        {


            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });
            _logger = loggerFactory.CreateLogger<ServiceManager>();
        }



        private IYggdrasil? yggdrasil = null;

        public IYggdrasil Yggdrasil
        {
            get
            {
                if (yggdrasil == null)
                {
                    throw new ArgumentNullException();
                }
                return yggdrasil;
            }
            set
            {
                yggdrasil = value;
            }
        }


        private IIdentityCreation? identity = null;

        public IIdentityCreation Identity
        {
            get
            {
                if (identity == null)
                {
                    throw new ArgumentNullException();
                }
                return identity;
            }
            set
            {
                identity = value;
            }
        }

        public async Task CreateFullCertificateAuthority(string path, CertificateSettings settings)
        {
            try
            {
                var builder = new ConcreteSystemSecurityBuilder();
                await Identity.CreateFullCertificateAuthority(path, settings, builder);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message, e.Data);
            }
        }

        public void CancelCreation()
        {
            try
            {
                var result = Identity.Cancel();
                if (result == null)
                {
                    Console.WriteLine("Creation has not been started");
                }
                if (result == true)
                {
                    Console.WriteLine("Task has been canceled");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message, e.Data);
            }

        }



        //TODO :Dispose
    }
}

