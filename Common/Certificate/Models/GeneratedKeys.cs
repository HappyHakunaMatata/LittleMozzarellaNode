using System;
using System.Buffers;
using System.Security.Cryptography.X509Certificates;

namespace Common.Certificate.Models
{
	public class GeneratedKeys
	{
		private byte[]? _id;
        private byte[]? _privateKey;
        private byte[]? _publicKey;

        public GeneratedKeys(byte[] id, byte[] privatekey, byte[] publickey)
		{
            /*_id = ArrayPool<byte>.Shared.Rent(id.Length);
            Buffer.BlockCopy(id, 0, _id, 0, id.Length);

            _privateKey = ArrayPool<byte>.Shared.Rent(privatekey.Length);
            Buffer.BlockCopy(privatekey, 0, _privateKey, 0, privatekey.Length);

            _publicKey = ArrayPool<byte>.Shared.Rent(publickey.Length);
            Buffer.BlockCopy(publickey, 0, _publicKey, 0, publickey.Length);
            */
            _id = id;
            _privateKey = privatekey;
            _publicKey = publickey;
		}

        public byte[] GetId()
        {
            ArgumentNullException.ThrowIfNull(_id);
            return _id;
        }

        public byte[] GetPrivateKey()
        {
            ArgumentNullException.ThrowIfNull(_privateKey);
            return _privateKey;
        }

        public byte[] GetPublicKey()
        {
            ArgumentNullException.ThrowIfNull(_publicKey);
            return _publicKey;
        }

        /*
        public void Dispose()
		{
            ArgumentNullException.ThrowIfNull(_id);
            ArgumentNullException.ThrowIfNull(_privateKey);
            ArgumentNullException.ThrowIfNull(_publicKey);
            byte[] toReturnId = _id;
			if (toReturnId != null)
			{
				_id = null;
				ArrayPool<byte>.Shared.Return(toReturnId);
			}
            byte[] toReturnPrivKey = _privateKey;
            if (toReturnPrivKey != null)
            {
                _privateKey = null;
                ArrayPool<byte>.Shared.Return(toReturnPrivKey);
            }
			byte[] toReturnPubkey = _publicKey;
            if (toReturnPubkey != null)
            {
                _publicKey = null;
                ArrayPool<byte>.Shared.Return(toReturnPubkey);
            }
        }*/
	}
}

