using System;
namespace Common.Extensions
{
	public static class FilePath
	{


		public static ushort IsExists(string certPath, string keyPath)
		{
			ArgumentException.ThrowIfNullOrEmpty(certPath);
			ArgumentException.ThrowIfNullOrEmpty(keyPath);
			var hasKey = true;
			var hasCert = true;


			try
			{
				FileInfo fileInfo = new FileInfo(certPath);
				if (!fileInfo.Exists)
				{
					hasKey = false;
				}
				fileInfo = new FileInfo(keyPath);
				if (!fileInfo.Exists)
				{
					hasCert = false;
				}

				if (hasCert == true && hasKey == true)
				{
					return 3;
				}
                if (hasCert == false && hasKey == true)
                {
                    return 2;
                }
				if (hasCert == true && hasKey == false)
				{
					return 1;
				}
				return 0;
			}
			catch
			{
				throw;
			}
        }
	}
}

