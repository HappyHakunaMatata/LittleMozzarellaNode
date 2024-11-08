﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


namespace Common.Certificate

{
	public class CertificateTemplate
	{
		public CertificateTemplate(byte[] SerialNumber, X500DistinguishedName DistinguishedName, List<X509Extension> X509Extensions)
		{
			this.SerialNumber = SerialNumber;
			this.DistinguishedName = DistinguishedName;
			this.X509Extensions = X509Extensions;
        }



        internal byte[] SerialNumber
		{
			get;
		}

		public X500DistinguishedName DistinguishedName
		{
			get;
		}


		public List<X509Extension> X509Extensions
        {
			get;
		}

		public CertificateRequest GetCertificateRequest(ECDsa key, HashAlgorithmName signatureAlgorithm = default)
		{
            ArgumentNullException.ThrowIfNull(key);
            try
			{
				var request = new CertificateRequest(DistinguishedName, key, signatureAlgorithm);
				foreach (var i in X509Extensions)
				{
					request.CertificateExtensions.Add(i);
				}
				return request;
			}
			catch
			{
				throw;
			}
        }

		public byte[] GetSerialNumber()
		{
			return SerialNumber;
		}
    }
}

