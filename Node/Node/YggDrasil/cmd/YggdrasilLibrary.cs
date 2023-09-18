using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Node.YggDrasil.models;

namespace Node.YggDrasil.cmd
{
    public static partial class YggdrasilLibrary
    {


        [LibraryImport("yggdrasil.dll")]
        private static partial YggdrasilReturnModel start(
            byte autoconf = 0);

        [LibraryImport("yggdrasil.dll")]
        private static partial YggdrasilReturnModel setup(
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

        public static YggdrasilReturnModel GetAddr(IntPtr ptr)
        {
            return setup(useconffile: ptr, getaddr: 1,
                genconf: 0, normaliseconf: 0, confjson: 0, ver: 0, getsnet: 0,
                buildname: 0, loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilReturnModel GetSnet(IntPtr ptr)
        {
            return setup(useconffile: ptr, getsnet: 1,
                genconf: 0, normaliseconf: 0, confjson: 0, ver:0, getaddr: 0,
                buildname: 0, loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilReturnModel Ver()
        {
            return setup(ver: 1,
                getsnet: 0, genconf: 0, normaliseconf: 0, confjson: 0, getaddr: 0,
                buildname: 0, loglevel: IntPtr.Zero, logto: IntPtr.Zero, useconffile: IntPtr.Zero);
        }

        public static YggdrasilReturnModel BuildName()
        {
            return setup(buildname: 1,
                genconf: 0, normaliseconf: 0, confjson: 0 , ver: 0, getaddr: 0,
            getsnet: 0, useconffile:IntPtr.Zero, loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }


        public static YggdrasilReturnModel Autoconf()
        {
            return start(autoconf: 1);
        }

        public static YggdrasilReturnModel Genconf(byte json = 1)
        {
            return setup(genconf: 1, confjson: json,
                buildname: 0, normaliseconf: 0, ver: 0, getaddr: 0,
                getsnet: 0, useconffile: IntPtr.Zero,
                loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilReturnModel Useconffile(IntPtr ptr, byte normaliseconf = 1, byte json = 1)
        {
            return setup(useconffile: ptr, normaliseconf: normaliseconf, confjson: json,
                buildname: 0, genconf: 0, ver: 0, getaddr: 0, getsnet: 0,
                loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilReturnModel LogLevels(IntPtr ptr)
        {
            return setup(loglevel: ptr,
                buildname: 0, genconf: 0, normaliseconf: 0, confjson: 0,
                ver: 0, getaddr: 0, getsnet: 0, useconffile: IntPtr.Zero,
                logto: IntPtr.Zero);
        }

        public static YggdrasilReturnModel Logto(IntPtr ptr)
        {
            return setup(logto: ptr, buildname: 0, genconf: 0, normaliseconf: 0, confjson: 0,
                ver: 0, getaddr: 0, getsnet: 0, useconffile: IntPtr.Zero,
                loglevel: IntPtr.Zero);
        }

        public static YggdrasilReturnModel Help()
        {
            return setup(buildname: 0, genconf: 0, normaliseconf: 0, confjson: 0,
                ver: 0, getaddr: 0, getsnet: 0, useconffile: IntPtr.Zero,
                loglevel: IntPtr.Zero, logto: IntPtr.Zero);
        }

        public static YggdrasilReturnModel Start()
        {
            return start(autoconf: 0);
        }

        [LibraryImport("yggdrasil.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool Exit();
    }
}

