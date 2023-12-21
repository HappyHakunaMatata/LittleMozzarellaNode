using System;
using Node.YggDrasil.models;
using System.Runtime.InteropServices;

namespace Node.YggDrasil.cmd
{

    public static partial class YggdrasilLibraryImporter
	{
        [LibraryImport("yggdrasil.dll")]
        private static partial YggdrasilSafeReturnModel setup(
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


        [LibraryImport("yggdrasil.dll")]
        private static partial YggdrasilSafeReturnModel start(
            byte autoconf = 0);

        public static YggdrasilSafeReturnModel GetAddr(IntPtr ptr)
        {
            return setup(useconffile: ptr, getaddr: 1,
                genconf: 0, normaliseconf: 0, confjson: 0, ver: 0, getsnet: 0,
                buildname: 0, loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilSafeReturnModel GetSnet(IntPtr ptr)
        {
            return setup(useconffile: ptr, getsnet: 1,
                genconf: 0, normaliseconf: 0, confjson: 0, ver: 0, getaddr: 0,
                buildname: 0, loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilSafeReturnModel Ver()
        {
            return setup(ver: 1,
                getsnet: 0, genconf: 0, normaliseconf: 0, confjson: 0, getaddr: 0,
                buildname: 0, loglevel: IntPtr.Zero, logto: IntPtr.Zero, useconffile: IntPtr.Zero);
        }

        public static YggdrasilSafeReturnModel BuildName()
        {
            return setup(buildname: 1,
                genconf: 0, normaliseconf: 0, confjson: 0, ver: 0, getaddr: 0,
            getsnet: 0, useconffile: IntPtr.Zero, loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }


        public static YggdrasilSafeReturnModel Autoconf()
        {
            return start(autoconf: 1);
        }

        public static YggdrasilSafeReturnModel Genconf(byte json = 1)
        {
            return setup(genconf: 1, confjson: json,
                buildname: 0, normaliseconf: 0, ver: 0, getaddr: 0,
                getsnet: 0, useconffile: IntPtr.Zero,
                loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilSafeReturnModel Useconffile(IntPtr ptr, byte normaliseconf = 1, byte json = 1)
        {
            return setup(useconffile: ptr, normaliseconf: normaliseconf, confjson: json,
                buildname: 0, genconf: 0, ver: 0, getaddr: 0, getsnet: 0,
                loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilSafeReturnModel LogLevels(IntPtr ptr)
        {
            return setup(loglevel: ptr,
                buildname: 0, genconf: 0, normaliseconf: 0, confjson: 0,
                ver: 0, getaddr: 0, getsnet: 0, useconffile: IntPtr.Zero,
                logto: IntPtr.Zero);
        }

        public static YggdrasilSafeReturnModel Logto(IntPtr ptr)
        {
            return setup(logto: ptr, buildname: 0, genconf: 0, normaliseconf: 0, confjson: 0,
                ver: 0, getaddr: 0, getsnet: 0, useconffile: IntPtr.Zero,
                loglevel: IntPtr.Zero);
        }

        public static YggdrasilSafeReturnModel Help()
        {
            return setup(buildname: 0, genconf: 0, normaliseconf: 0, confjson: 0,
                ver: 0, getaddr: 0, getsnet: 0, useconffile: IntPtr.Zero,
                loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilSafeReturnModel Start()
        {
            return start(autoconf: 0);
        }

        [LibraryImport("yggdrasil.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool Exit();
    }
}

