using System;
using Common.Certificate.Models;

namespace Common.Certificate.Interfaces
{
	public interface IIdentityCreation
	{
        public Task CreateFullCertificateAuthority(string path, CertificateSettings settings, AbstractCertificateBuilder builder);
        public bool? Cancel();
        public void Dispose();
    }
}

