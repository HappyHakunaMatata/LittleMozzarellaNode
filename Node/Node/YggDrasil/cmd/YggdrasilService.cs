using System;
using Node.YggDrasil.models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection.Metadata;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using Node.Interfaces;

namespace Node.YggDrasil.cmd
{
    public class YggdrasilService : SafeHandle, IYggdrasil
    {

        public YggdrasilService(IntPtr handle) : base(handle, true)
        {
            SetHandle(handle);
        }

        public YggdrasilService() : this(IntPtr.Zero)
        {

        }

        override protected bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }

        public override bool IsInvalid
        {
            get
            { 
                return handle == new IntPtr(-1);
            }
        }

        

        public Tuple<int, string> NormaliseConfing(string path = "yggdrasil.conf", bool json = true)
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            byte convertedByte = json ? (byte)1 : (byte)0;
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.NormaliseConfing(handle, convertedByte);
                Marshal.FreeHGlobal(handle);
                string text = textptr.safeHandle.GetString();
                textptr.safeHandle.Dispose();
                return new Tuple<int, string>(textptr.err_code, text);
            }
            catch
            {
                throw;
            }
        }

        
        public Tuple<int, string> GetIPAddress(string path = "yggdrasil.conf")
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path); 
            }
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.GetAddress(handle);
                Marshal.FreeHGlobal(handle);
                string text = textptr.safeHandle.GetString();
                textptr.safeHandle.Dispose();
                return new Tuple<int, string>(textptr.err_code, text);
            }
            catch
            {
                throw;
            }
        }

        public Tuple<int, string> GetPemKey(string path = "yggdrasil.conf")
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.GetPemKey(handle);
                Marshal.FreeHGlobal(handle);
                string text = textptr.safeHandle.GetString();
                textptr.safeHandle.Dispose();
                return new Tuple<int, string>(textptr.err_code, text);
            }
            catch
            {
                throw;
            }
        }

        public Tuple<int, string> GetPrivateKey(string path = "yggdrasil.conf")
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.GetPkey(handle);
                Marshal.FreeHGlobal(handle);
                string text = textptr.safeHandle.GetString();
                textptr.safeHandle.Dispose();
                return new Tuple<int, string>(textptr.err_code, text);
            }
            catch
            {
                throw;
            }
        }


        public Tuple<int, string> GetSnet(string path = "yggdrasil.conf")
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.GetSnet(handle);
                Marshal.FreeHGlobal(handle);
                string text = textptr.safeHandle.GetString();
                textptr.safeHandle.Dispose();
                return new Tuple<int, string>(textptr.err_code, text);
            }
            catch
            {
                throw;
            }
        }
        
        public string Version()
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            try
            {
                string IPv6 = YggdrasilLibraryImporter.GetVersion();
                return IPv6;
            }
            catch
            {
                throw;
            }
        }
        
        public string BuildName()
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            try
            {
                string name = YggdrasilLibraryImporter.GetBuildName(); 
                return name;
            }
            catch
            {
                throw;
            }
        }
        
        

        public Tuple<int, string> Genconf(bool json = true)
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            byte convertedByte = json ? (byte)1 : (byte)0;
            try
            {
                var model = YggdrasilLibraryImporter.GenConfigFile(convertedByte);
                var text = model.safeHandle.GetString();
                model.safeHandle.Dispose();
                return new Tuple<int, string>(model.err_code, text);
            }
            catch
            {
                throw;
            }
        }
        
        public void Logto(Logs log = Logs.custom, string path = "")
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            byte[] stringBytes;
            if (log == Logs.custom)
            {
                stringBytes = Encoding.ASCII.GetBytes(path + '\0');
            }
            else
            {
                stringBytes = Encoding.ASCII.GetBytes(log.ToString() + '\0');
            }
            try
            {
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                YggdrasilLibraryImporter.Logto(handle);
                Marshal.FreeHGlobal(handle);
            }
            catch
            {
                throw;
            }
        }

        
        private CancellationTokenSource? cancellationTokenSource;
        
        public Tuple<int, string> RunYggdrasilAsync(string? path)
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (path != null || !File.Exists(path))
                {
                    byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                    handle = Marshal.AllocHGlobal(stringBytes.Length);
                    Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                }
                else
                {
                    handle = nint.Zero;
                }
                var task = YggdrasilLibraryImporter.Start(handle);
                Marshal.FreeHGlobal(handle);
                string text = task.safeHandle.GetString();
                task.safeHandle.Dispose();
                return new Tuple<int, string>(task.err_code, text);
            }
            catch
            {
                throw;
            }
        }

        public Tuple<int, string> SetLogLevel(LogLevels logLevel = LogLevels.error)
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(logLevel.ToString() + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.SetLogLevel(handle);
                Marshal.FreeHGlobal(handle);
                string text = textptr.safeHandle.GetString();
                textptr.safeHandle.Dispose();
                return new Tuple<int, string>(textptr.err_code, text);
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }

        public bool ExitYggdrasil()
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
            try
            {
                return YggdrasilLibraryImporter.Exit();
            }
            catch
            {
                throw;
            }
        }

        private bool _disposed = false;
        protected new void Dispose(bool disposing)
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

        ~YggdrasilService()
        {
            Dispose(disposing: false);
        }
    }
}

