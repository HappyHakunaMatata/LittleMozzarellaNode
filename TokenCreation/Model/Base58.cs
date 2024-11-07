using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using TokenCreation.DLL;



namespace TokenCreation.Model
{
	public class Base58 : IDisposable
    {

        private IntPtr handle;

        public bool HandleIsInvalid
        {
            get
            {
                return (this.handle == IntPtr.Zero);
            }
        }

        public Base58(IntPtr ptr)
        {
            this.handle = ptr;
        }


        public Base58() : this(IntPtr.Zero)
        { }


        public string? Encode(byte[] input)
        {
            ArgumentNullException.ThrowIfNull(input);
            try
            {
                var inputStr = Encoding.UTF8.GetString(input);
                handle = Marshal.StringToHGlobalAnsi(inputStr);
                var result =  Base58Import.Encode(handle, inputStr.Length);
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public string? CheckEncode(byte[] input, byte version)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(version);
            try
            {
                var inputStr = Encoding.UTF8.GetString(input);
                handle = Marshal.StringToHGlobalAnsi(inputStr);
                var result = Base58Import.CheckEncode(handle, inputStr.Length, version);
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        
        public byte[]? Decode(string input)
        {
            ArgumentNullException.ThrowIfNull(input);
            try
            {
                handle = Marshal.StringToHGlobalAnsi(input);
                var result = Base58Import.Decode(handle);
                var array = Encoding.UTF8.GetBytes(result);
                return array;
            }
            catch
            {
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public byte[]? CheckDecode(string input)
        {
            ArgumentNullException.ThrowIfNull(input);
            try
            {
                handle = Marshal.StringToHGlobalAnsi(input);
                var result = Base58Import.CheckDecode(handle);
                var array = Encoding.UTF8.GetBytes(result);
                return array;
            }
            catch
            {
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        private void CloseHandle()
        {
            if (this.HandleIsInvalid)
            {
                return;
            }
            handle = IntPtr.Zero;
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

        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~Base58()
        {
            Dispose(disposing: false);
        }


    }
}

