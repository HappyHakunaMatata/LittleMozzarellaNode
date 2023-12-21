using System.Runtime.InteropServices.Marshalling;
using Node.YggDrasil.cmd;
using Microsoft;

namespace Node.YggDrasil.models
{
    [NativeMarshalling(typeof(YggdrasilMarshaller))]
    public sealed class YggdrasilSafeReturnModel
    {
        public int err_code;
        public YggdrasilSafeHandle safeHandle;

        public YggdrasilSafeReturnModel(YggdrasilSafeHandle handle)
        {
            Requires.NotNull(handle, nameof(handle));

            safeHandle = handle;
        }

        public YggdrasilSafeReturnModel() : this(new YggdrasilSafeHandle()) { }
    };
}

