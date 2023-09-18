using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace TestFunc
{
	public class TestLib
	{
        public static void TestLibrary()
        {
            var result = Timeit(Test1);
            
            Console.WriteLine($"First func: {result.Item1}\t Status: {result.Item2.Item1} Ver: {result.Item2.Item2}");
            result = Timeit(Test2);
            Console.WriteLine($"Second func: {result.Item1}\t Status: {result.Item2} Ver: {result.Item2.Item2}");
        }

        private static Tuple<double, Tuple<int, string>> Timeit(Func<Tuple<int, string>> func)
        {
            var watch = Stopwatch.StartNew();
            var res = func();
            watch.Stop();
            return Tuple.Create(watch.Elapsed.TotalMilliseconds, res);
        }

        private static Tuple<int, string> Test1()
        {
            var version = InnerDllImport.Ver();
            string? text = Marshal.PtrToStringUTF8(version.safeHandle);
            Marshal.FreeHGlobal(version.safeHandle);
            if (text != null)
            {
                return new Tuple<int, string>(version.errCode, text);
            }
            return new Tuple<int, string>(version.errCode, "");
        }

        private static Tuple<int, string> Test2()
        {
            var version = InnerLibraryImport.Ver();
            string? text = Marshal.PtrToStringUTF8(version.safeHandle);
            Marshal.FreeHGlobal(version.safeHandle);
            if (text != null)
            {
                return new Tuple<int, string>(version.errCode, text);
            }
            return new Tuple<int, string>(version.errCode, "");
        }
        

    }
    public static partial class InnerLibraryImport
    {
        [LibraryImport("yggdrasil.dll")]
        private static partial YggdrasilUnsafeReturnStruct setup(
        int genconf = 2,
        int normaliseconf = 2,
        int confjson = 2,
        int ver = 2,
        int getaddr = 2,
        int getsnet = 2,
        int buildname = 2,
        IntPtr useconffile = default(IntPtr),
        IntPtr loglevel = default(IntPtr),
        IntPtr logto = default(IntPtr));

        public static YggdrasilUnsafeReturnStruct Ver()
        {
            return setup(ver: 1,
                getsnet: 0, genconf: 0, normaliseconf: 0, confjson: 0, getaddr: 0,
                buildname: 0, loglevel: IntPtr.Zero, logto: IntPtr.Zero, useconffile: IntPtr.Zero);
        }
    }

    public class InnerDllImport
    {
        [DllImport("yggdrasil.dll", SetLastError = true)]
        public static extern YggdrasilUnsafeReturnStruct setup(
            int genconf = 2,
            int normaliseconf = 2,
            int confjson = 2,
            int ver = 2,
            int getaddr = 2,
            int getsnet = 2,
            int buildname = 2,
            IntPtr useconffile = default(IntPtr),
            IntPtr loglevel = default(IntPtr),
            IntPtr logto = default(IntPtr));

        public static YggdrasilUnsafeReturnStruct Ver()
        {
            return setup(ver: 1,
                getsnet: 0, genconf: 0, normaliseconf: 0, confjson: 0, getaddr: 0,
                buildname: 0, loglevel: IntPtr.Zero, logto: IntPtr.Zero, useconffile: IntPtr.Zero);
        }
    }


    public struct YggdrasilUnsafeReturnStruct
    {
        public int errCode;
        public IntPtr safeHandle;
    }
}

