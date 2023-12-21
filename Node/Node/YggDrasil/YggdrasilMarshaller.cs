using System.Runtime.InteropServices.Marshalling;
using Node.YggDrasil.cmd;
using Node.YggDrasil.models;
using Microsoft;

namespace Node.YggDrasil
{
    [CustomMarshaller(typeof(YggdrasilSafeReturnModel), MarshalMode.ManagedToUnmanagedOut, typeof(ManagedToUnmanagedOut))]
    public static unsafe class YggdrasilMarshaller
	{
        public ref struct ManagedToUnmanagedOut
        {
            private YggdrasilUnsafeReturnStruct _unmanaged;
            private YggdrasilSafeHandle _safeHandle;
            private IntPtr _originalYggdrasilPtr;

            private bool _safeHandleAddRefd;


            public void FromManaged(YggdrasilSafeReturnModel data)
            {
                Requires.NotNull(data, nameof(data));

                _safeHandleAddRefd = false;

                _safeHandle = data.safeHandle;

                _safeHandle.DangerousAddRef(ref _safeHandleAddRefd);
                _unmanaged.safeHandle = _originalYggdrasilPtr = _safeHandle.DangerousGetHandle();
                _unmanaged.errCode = data.err_code;
            }

            public YggdrasilUnsafeReturnStruct ToUnmanaged()
            {
                return _unmanaged;
            }


            public void FromUnmanaged(YggdrasilUnsafeReturnStruct unmanaged)
            {
                _originalYggdrasilPtr = unmanaged.safeHandle;

                _unmanaged = unmanaged;
            }

            public YggdrasilSafeReturnModel ToManaged()
            {

                if (_unmanaged.safeHandle != _originalYggdrasilPtr)
                    throw new NotSupportedException();


                YggdrasilSafeHandle yggdrasilSafeHandle = new YggdrasilSafeHandle(_unmanaged.safeHandle, true);
                return new YggdrasilSafeReturnModel()
                {
                    err_code = _unmanaged.errCode,
                    safeHandle = yggdrasilSafeHandle,
                };
            }



            public void Free()
            {
                if (_safeHandleAddRefd)
                {
                    _safeHandle.DangerousRelease();
                }
            }
        }
    }
}

