using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TokenCreation.DLL
{
    public static partial class Base58Import
    {

        private static readonly string path = Path.Combine(Directory.GetCurrentDirectory(), @"DLL");
        [LibraryImport("base58", StringMarshalling = StringMarshalling.Utf8)]
        public static partial string Encode(IntPtr b, int size);


        [LibraryImport("base58", StringMarshalling = StringMarshalling.Utf8)]
        public static partial string Decode(IntPtr input);


        
        [LibraryImport("base58", StringMarshalling = StringMarshalling.Utf8)]
        public static partial string CheckDecode(IntPtr input);
        


        [LibraryImport("base58", StringMarshalling = StringMarshalling.Utf8)]
        public static partial string CheckEncode(IntPtr b, int size, byte version);
    }
}

