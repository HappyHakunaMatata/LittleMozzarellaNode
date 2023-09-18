using System;
using Node.YggDrasil.models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection.Metadata;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;

namespace Node.YggDrasil.cmd
{
    public class YggdrasilTest : SafeHandle
    {

        public YggdrasilTest(IntPtr handle) : base(handle, true)
        {
            SetHandle(handle);
        }

        public YggdrasilTest() : this(IntPtr.Zero)
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

        public bool CheckFileExistence(string filename = "yggdrasil.conf")
        {
            return File.Exists(filename);
        }

        public Tuple<int, string> Useconffile(string? path = "yggdrasil.conf", bool normaliseconf = true, bool json = true)
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            byte normaliseconfByte = normaliseconf ? (byte)1 : (byte)0;
            byte jsonByte = json ? (byte)1 : (byte)0;
            if (CheckFileExistence() == false)
            {
                Genconf();
            }
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.Useconffile(handle, normaliseconfByte, jsonByte);
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

        
        public Tuple<int, string> GetIPAddress(string path = "yggdrasil.conf")
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            if (CheckFileExistence() == false)
            {
                Genconf();
            }
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.GetAddr(handle);
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
        
        public Tuple<int, string> GetSnet(string path = "yggdrasil.conf")
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            if (CheckFileExistence() == false)
            {
                Genconf();
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
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }
        
        public Tuple<int, string> Version()
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            try
            {
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.Ver();
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
        
        public Tuple<int, string> BuildName()
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            try
            {
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.BuildName();
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
        
        internal async Task<Tuple<int, string>> Autoconf()
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var task = await Task.Run(() =>
                {
                    return YggdrasilLibraryImporter.Autoconf();
                }, cancellationTokenSource.Token);
                var model = task;
                var text = model.safeHandle.GetString();
                model.safeHandle.Dispose();
                return new Tuple<int, string>(model.err_code, text);
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
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
                var model = YggdrasilLibraryImporter.Genconf(convertedByte);
                var text = model.safeHandle.GetString();
                model.safeHandle.Dispose();
                return new Tuple<int, string>(model.err_code, text);
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
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
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.LogLevels(handle);
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
        
        public Tuple<int, string> SetLogPath(Logs log = Logs.custom, string path = "")
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

                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.Logto(handle);
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
        
        public Tuple<int, string> Help()
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            try
            {
                YggdrasilSafeReturnModel textptr = YggdrasilLibraryImporter.Help();
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

        
        private CancellationTokenSource? cancellationTokenSource;
        
        public async Task<Tuple<int, string>> RunYggdrasilAsync()
        {
            if (this.IsInvalid)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var task = await Task.Run(() =>
                {
                    return YggdrasilLibraryImporter.Start();
                }, cancellationTokenSource.Token);
                var model = task;
                var text = model.safeHandle.GetString();
                model.safeHandle.Dispose();
                return new Tuple<int, string>(model.err_code, text);
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
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                return false;
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

        ~YggdrasilTest()
        {
            Dispose(disposing: false);
        }
    }
}

