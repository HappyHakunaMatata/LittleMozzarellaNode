using System;
using Common.Certificate.Models;

///TODO: what options do we need
namespace Node.gRPC
{
	public class Options
	{
		public readonly FullIdentity fullIdentity;

        public Options(FullIdentity fullIdentity)
		{
			this.fullIdentity = fullIdentity;
		}

	}
}

