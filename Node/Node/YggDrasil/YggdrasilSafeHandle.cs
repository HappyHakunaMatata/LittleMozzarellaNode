using System;
using Microsoft.Win32.SafeHandles;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;
using Node.YggDrasil.cmd;
using System.Runtime.InteropServices;
using Node.YggDrasil.models;
using System.Text;

namespace Node.YggDrasil.cmd
{
    public class YggdrasilSafeHandle : SafeHandle
    {


        public YggdrasilSafeHandle(IntPtr handle, bool ownsHandle) : this(ownsHandle)
        {
            SetHandle(handle);
        }

        public YggdrasilSafeHandle() : this(true)
        { }


        private YggdrasilSafeHandle(bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
        { }

        override protected bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }

        public static YggdrasilSafeHandle InvalidHandle => new(false);

        public override bool IsInvalid => handle == IntPtr.Zero;

        public string GetString()
        {
            if (IsInvalid)
            {
                throw new InvalidOperationException("Invalid handle");
            }
            try
            {
                
                string? result = Marshal.PtrToStringUTF8(handle);
                return result ?? "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private bool _disposed = false;
        protected new virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (!IsInvalid)
                {
                    SetHandle(IntPtr.Zero);
                }
            }
            base.Dispose();
            _disposed = true;
        }

        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~YggdrasilSafeHandle()
        {
            Dispose(disposing: false);
        }
    }
}

