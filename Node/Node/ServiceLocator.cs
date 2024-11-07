using System;
using System.Runtime.InteropServices;
using Common.Certificate;
using Common.Certificate.Interfaces;
using Common.Certificate.Models;
using Node.Interfaces;
using Node.YggDrasil.cmd;

namespace Node
{
	public class ServiceLocator
	{
		private static ServiceLocator? locator = null;

		public static ServiceLocator Instance
		{
			get
			{
				if (locator == null)
				{
					locator = new ServiceLocator();
				}
				return locator;
			}
		}

		public ServiceLocator()
		{

		}


		private IIdentityCreation? identity = null;


        
        public IIdentityCreation GetIdentityCreation()
		{

			if (identity == null)
			{
				identity = new CertificateAuthorityService();
			}
            return identity;
        }


		private IYggdrasil? yggdrasil = null;

		public IYggdrasil GetYggdrasil()
		{
			if (yggdrasil == null)
			{
				yggdrasil = new YggdrasilService();
			}
			return yggdrasil;
		}
    }
}

