using System;
namespace Node.Certificate.Models
{
	public class CertificateAuthorityConfig
	{
		public CertificateAuthorityConfig()
		{
		}

		public uint minimumLoggableDifficulty = 8;


        public UInt16 Difficulty = 36;

		public uint Concurrency = 4;

        public byte versionID = 0;

        public int idArraySize = 32;
    }
}

