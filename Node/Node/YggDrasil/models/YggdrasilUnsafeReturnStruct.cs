using System.Runtime.InteropServices;

namespace Node.YggDrasil.models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct YggdrasilUnsafeReturnStruct
	{
        public int errCode;
        public IntPtr safeHandle;
    }
}

