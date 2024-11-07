using System;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace TokenCreation.Model
{
	public class TokenManager
	{
		public TokenManager()
		{
		}

		public Authorization NewAuthorization(string ID)
		{
            Token token = new Token { UserID = ID };
            Random rnd = new Random();
            try
            {
                token.Data = new byte[64];
                rnd.NextBytes(token.Data);
            }
            catch
            {
                throw;
            }
            Authorization authorization = new Authorization(token.UserID);


            List<Claim> claim = new()
            {
                new Claim("IPv4", ""),
                new Claim("Timestamp", ""),
                new Claim("IPv6", ""),
                new Claim("YggdrasilAddress", ""),
                new Claim("Identity", ""),
                new Claim("SignedChainBytes", ""),
            };



            return authorization;
        }

        public void Create(string ID)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(ID);
            Authorization authorization = NewAuthorization(ID);
            Console.WriteLine($"Completed: {authorization.Complete}");
        }

	}
}

