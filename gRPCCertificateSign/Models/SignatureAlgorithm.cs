using System;
namespace Node.Certificate.Models
{
	public class SignatureAlgorithm
	{
		
		public SignatureAlgorithmName GetSignatureAlgorithm(string signatureAlgorithm)
		{
            if (Enum.TryParse<SignatureAlgorithmName>(signatureAlgorithm, out var result))
            {
                return result;
            }
            return SignatureAlgorithmName.None;
        }
    }
}

