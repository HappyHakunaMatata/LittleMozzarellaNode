using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Node.YggDrasil.models;

namespace Node.YggDrasil.cmd
{
	public class Yggdrasil: IDisposable
    {
        
        public YggdrasilReturnModel model;
        private IntPtr handle;


        public Yggdrasil(YggdrasilReturnModel model, IntPtr ptr)
        {
            this.model.Handle = model.Handle;
            this.model.err_code = model.err_code;
            this.handle = ptr;
        }


        public Yggdrasil() : this(new YggdrasilReturnModel()
        {
            err_code = 0,
            Handle = IntPtr.Zero,
        }, IntPtr.Zero) { }

        public bool ModelHandleIsInvalid
        {
            get
            {
                return (this.model.Handle == IntPtr.Zero);
            }
        }

        public bool HandleIsInvalid
        {
            get
            {
                return (this.handle == IntPtr.Zero);
            }
        }

        static bool CheckFileExistence(string filename = "yggdrasil.conf")
        {
            return File.Exists(filename);
        }
        
        public Tuple<int, string> GetIPAddress(string path = "yggdrasil.conf")
        {
            if (CheckFileExistence() == false)
            {
                Genconf();
            }
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                model = YggdrasilLibrary.GetAddr(handle);
                string? text = Marshal.PtrToStringUTF8(model.Handle);

                Marshal.FreeHGlobal(model.Handle);
                Marshal.FreeHGlobal(handle);

                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }

        public Tuple<int, string> GetSnet(string path = "yggdrasil.conf")
        {
            if (CheckFileExistence() == false)
            {
                Genconf();
            }
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(path + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                model = YggdrasilLibrary.GetSnet(handle);
                string? text = Marshal.PtrToStringUTF8(model.Handle);

                Marshal.FreeHGlobal(model.Handle);
                Marshal.FreeHGlobal(handle);

                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }

        public Tuple<int, string> Version()
        {
            try
            {
                model = YggdrasilLibrary.Ver();
                string? text = Marshal.PtrToStringUTF8(model.Handle);
                Marshal.FreeHGlobal(model.Handle);
                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }

        public Tuple<int, string> BuildName()
        {
            try
            {
                model = YggdrasilLibrary.BuildName();
                string? text = Marshal.PtrToStringUTF8(model.Handle);
                Marshal.FreeHGlobal(model.Handle);
                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }

        internal async Task<Tuple<int, string>> Autoconf()
        {
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var task = await Task.Run(() =>
                {
                    return YggdrasilLibrary.Autoconf();
                }, cancellationTokenSource.Token);
                this.model = task;
                string? text = Marshal.PtrToStringUTF8(model.Handle);
                Marshal.FreeHGlobal(model.Handle);
                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }

        public Tuple<int, string> Genconf(bool json = true)
        {
            byte convertedByte = json ? (byte)1 : (byte)0;
            try
            {
                model = YggdrasilLibrary.Genconf(convertedByte);
                string? text = Marshal.PtrToStringUTF8(model.Handle);
                Marshal.FreeHGlobal(model.Handle);
                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string> (1, e.Message);
            }
        }
        
        public Tuple<int, string> Useconffile(string? path = "yggdrasil.conf", bool normaliseconf = true, bool json = true)
        {
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
                model = YggdrasilLibrary.Useconffile(handle, normaliseconfByte, jsonByte);
                string? text = Marshal.PtrToStringUTF8(model.Handle);
                Marshal.FreeHGlobal(handle);
                Marshal.FreeHGlobal(model.Handle);
                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }

        public Tuple<int, string> SetLogLevel(LogLevels logLevel = LogLevels.error)
        {
            try
            {
                byte[] stringBytes = Encoding.ASCII.GetBytes(logLevel.ToString() + '\0');
                handle = Marshal.AllocHGlobal(stringBytes.Length);
                Marshal.Copy(stringBytes, 0, handle, stringBytes.Length);
                model = YggdrasilLibrary.LogLevels(handle);
                string? text = Marshal.PtrToStringUTF8(model.Handle);
                Marshal.FreeHGlobal(handle);
                Marshal.FreeHGlobal(model.Handle);
                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }

        public Tuple<int, string> SetLogPath(Logs log = Logs.custom, string path = "")
        {
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
                model = YggdrasilLibrary.Logto(handle);
                string? text = Marshal.PtrToStringUTF8(model.Handle);
                Marshal.FreeHGlobal(handle);
                Marshal.FreeHGlobal(model.Handle);
                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }

        public Tuple<int, string> Help()
        {
            try
            {
                model = YggdrasilLibrary.Help();
                string? text = Marshal.PtrToStringUTF8(model.Handle);
                Marshal.FreeHGlobal(model.Handle);
                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }


        private CancellationTokenSource? cancellationTokenSource;

        public async Task<Tuple<int, string>> RunYggdrasilAsync()
        {
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var task = await Task.Run(() =>
                {
                    return YggdrasilLibrary.Start();
                }, cancellationTokenSource.Token);
                this.model = task;
                string? text = Marshal.PtrToStringUTF8(model.Handle);
                Marshal.FreeHGlobal(model.Handle);
                if (text != null)
                {
                    return new Tuple<int, string>(model.err_code, text);
                }
                return new Tuple<int, string>(model.err_code, "null");
            }
            catch (Exception e)
            {
                return new Tuple<int, string>(1, e.Message);
            }
        }
        
        public bool ExitYggdrasil()
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
            try
            {
                return YggdrasilLibrary.Exit();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                return false;
            }
        }


        private void CloseHandle()
        {
            if (this.ModelHandleIsInvalid && this.HandleIsInvalid)
            {
                return;
            }
            

            if (!YggdrasilLibrary.Exit())
            {
                Trace.WriteLine("yggdrasil.dll exit exception was thrown");
            }

            handle = IntPtr.Zero;
            model.Handle = IntPtr.Zero;
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed resources.
            }
            CloseHandle();
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~Yggdrasil()
        {
            Dispose(disposing: false);
        }
    }
}

