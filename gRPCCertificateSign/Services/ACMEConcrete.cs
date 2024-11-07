using System;
using System.Runtime.ConstrainedExecution;
using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
using Org.BouncyCastle.Asn1.X509;

namespace gRPCCertificateSign.Services
{
	public class ACMEConcrete 
    {
		public ACMEConcrete()
		{
            
        }

        

		public async Task<string> RegisterAccount(string email)
		{
            try
            {
                var acme = new AcmeContext(WellKnownServers.LetsEncryptStagingV2);
                await acme.NewAccount(email, true);
                var pemKey = acme.AccountKey.ToPem();
                SaveKey(pemKey, "AccountKey.pem");
                return pemKey;
            }
            catch
            {
                throw;
            }
        }

        private void SaveKey(string pem, string filename)
        {
            try
            {
                if (!string.IsNullOrEmpty(pem))
                {
                    string path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), filename);
                    File.WriteAllText(path, pem);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<AcmeContext> LoginAccount(string pemKey)
        {
            var accountKey = KeyFactory.FromPem(pemKey);
            var acme = new AcmeContext(WellKnownServers.LetsEncryptStagingV2, accountKey);
            //var account = await acme.Account();
            return acme;
        }

        public async Task<IOrderContext> CreateOrder(AcmeContext acme, string domain)
        {
            var order = await acme.NewOrder(new[] { domain });
            var authz = (await order.Authorizations()).First();
            var dnsChallenge = await authz.Dns();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Your DNS Token: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(acme.AccountKey.DnsTxt(dnsChallenge.Token));
            return order;
        }

        public async Task Authorization(IOrderContext order)
        {
            var authz = (await order.Authorizations()).First();
            var httpChallenge = await authz.Http();
            var keyAuthz = httpChallenge.KeyAuthz;
            SaveKey(keyAuthz, "AuthKey.key");
        }


    }
}

