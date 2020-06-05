using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Utilities
{
    internal static class NativeMethods
    {
        public const UInt32 LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        public static readonly IntPtr RT_MANIFEST = new IntPtr(24);
        public static readonly IntPtr RT_STRING = new IntPtr(6);

        public static readonly int CUSTOM_RESOURCE_TYPE = 50;
        public static readonly int ID_BINARYTOLAUNCH = 101;

        public static readonly string BINARYTOLAUNCH_STRING = "BINARYTOLAUNCH";

        public static readonly IntPtr CUSTOM_RESOURCE_TYPE_PTR = new IntPtr(CUSTOM_RESOURCE_TYPE);
        public static readonly IntPtr ID_BINARYTOLAUNCH_PTR = new IntPtr(ID_BINARYTOLAUNCH);


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr BeginUpdateResourceW(String fileName, bool deleteExistingResource);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool UpdateResourceW(IntPtr hUpdate, IntPtr lpType, String lpName, short wLanguage, byte[] data, int cbData);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr FindResource(IntPtr hModule, string lpName, IntPtr lpType);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int EnumResourceNames(IntPtr hModule, IntPtr pType, EnumResNameProc enumFunc, IntPtr param);
        public delegate bool EnumResNameProc(IntPtr hModule, IntPtr pType, IntPtr pName, IntPtr param);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryExW(string strFileName, IntPtr hFile, UInt32 ulFlags);

    }
}
