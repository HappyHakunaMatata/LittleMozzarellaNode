using System;
namespace Node.Certificate.Math
{
	public class Bits
	{
		public Bits()
		{
		}


        /// <summary>
        /// TODO: TEST
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int TrailingZeros16(UInt32 x)
        {
            if (x == 0)
            {
                return 16;
            }
            return (int)(deBruijn32tab[((uint)((UInt32)(x & -x) * deBruijn32) >> (32 - 5))]);
        }


        public byte[] deBruijn32tab = new byte[32]{
    0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
    31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9};

        public const UInt32 deBruijn32 = 125613361;
        public const UInt64 deBruijn64 = 0x03f79d71b4ca8b09;


        public int TrailingZeros32(UInt32 x)
        {
            if (x == 0)
            {
                return 32;
            }
            return (int)(deBruijn32tab[((uint)((x & -x) * deBruijn32) >> (32 - 5))]);
        }
    }
}

