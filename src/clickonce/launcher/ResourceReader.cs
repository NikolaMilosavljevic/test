using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Utilities
{
    class ResourceReader
    {

        ResourceReader()
        {

        }

        public static string GetBinaryToLaunch(string path)
        {
            IntPtr hModule = IntPtr.Zero;
            string binaryToLaunch = string.Empty;

            try
            {
                hModule = NativeMethods.LoadLibraryExW(path, IntPtr.Zero, NativeMethods.LOAD_LIBRARY_AS_DATAFILE);
                if (hModule != IntPtr.Zero)
                {
                    binaryToLaunch = GetResourceString(hModule, NativeMethods.CUSTOM_RESOURCE_TYPE_PTR, NativeMethods.BINARYTOLAUNCH_STRING);
                }
            }
            catch(Exception)
            {
                // TODO: actually throw and catch exceptions
            }
            finally
            {
                if (hModule != IntPtr.Zero)
                    NativeMethods.FreeLibrary(hModule);
            }

            return binaryToLaunch;
        }

        private static string GetResourceString(IntPtr hModule, IntPtr pType, string pName)
        {
            string value = string.Empty;
            IntPtr hResInfo = NativeMethods.FindResource(hModule, pName, pType);
            if (hResInfo != IntPtr.Zero)
            {
                IntPtr hResource = NativeMethods.LoadResource(hModule, hResInfo);
                if (hResource != IntPtr.Zero)
                {
                    IntPtr hResLock = NativeMethods.LockResource(hResource);
                    if (hResLock != IntPtr.Zero)
                    {
                        uint bufsize = NativeMethods.SizeofResource(hModule, hResInfo);
                        byte[] buffer = new byte[bufsize];

                        Marshal.Copy(hResource, buffer, 0, buffer.Length);
                        value = Encoding.Unicode.GetString(buffer, 0, buffer.Length);
                        // strip trailing '\0' - VS code might add it
                        if (value.EndsWith("\0"))
                        {
                            value = value.Substring(0, value.Length - 1);
                        }
                    }
                }
            }

            return value;
        }
    }
}
