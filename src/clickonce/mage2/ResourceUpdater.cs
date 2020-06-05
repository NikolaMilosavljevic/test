using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Utilities
{
    internal class ResourceUpdater
    {
        private const int ERROR_SHARING_VIOLATION = -2147024864;
        private ArrayList stringResources;

        public ResourceUpdater()
        {
            this.stringResources = new ArrayList();
        }

        public void AddStringResource(int type, string name, string data)
        {
            stringResources.Add(new StringResource(type, name, data));
        }

        public bool UpdateResources(string filename/*, BuildResults results*/)
        {
            bool returnValue = true;
            int beginUpdateRetries = 20;    // Number of retries
            const int beginUpdateRetryInterval = 100; // In milliseconds
            bool endUpdate = false; // Only call EndUpdateResource() if this is true

            // Environment.CurrentDirectory has previously been set to the project location
            string filePath = System.IO.Path.Combine(Environment.CurrentDirectory, filename);

            if (stringResources.Count == 0)
                return true;
            IntPtr hUpdate = IntPtr.Zero;

            try
            {
                hUpdate = NativeMethods.BeginUpdateResourceW(filePath, false);
                while (IntPtr.Zero == hUpdate && Marshal.GetHRForLastWin32Error() == ResourceUpdater.ERROR_SHARING_VIOLATION && beginUpdateRetries > 0) // If it equals 0x80070020 (ERROR_SHARING_VIOLATION), sleep & retry
                {
                    // This warning can be useful for debugging, but shouldn't be displayed to an actual user
                    // results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.General", String.Format("Unable to begin updating resource for {0} with error {1:X}, trying again after short sleep", filename, Marshal.GetHRForLastWin32Error())));
                    hUpdate = NativeMethods.BeginUpdateResourceW(filePath, false);
                    beginUpdateRetries--;
                    Thread.Sleep(beginUpdateRetryInterval);
                }
                // If after all that we still failed, throw a build error
                if (IntPtr.Zero == hUpdate)
                {
                    // TODO:
                    //results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", String.Format("Unable to begin updating resource for {0} with error {1:X}", filename, Marshal.GetHRForLastWin32Error())));
                    return false;
                }

                endUpdate = true;

                if (hUpdate != IntPtr.Zero)
                {
                    foreach (StringResource resource in stringResources)
                    {
                        byte[] data = StringToByteArray(resource.Data);

                        if (!NativeMethods.UpdateResourceW(hUpdate, (IntPtr)resource.Type, resource.Name, 0, data, data.Length))
                        {
                            // TODO:
                            //results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", String.Format("Unable to update resource for {0} with error {1:X}", filename, Marshal.GetHRForLastWin32Error())));
                            return false;
                        }
                    }
                }
            }
            finally
            {
                if (endUpdate && !NativeMethods.EndUpdateResource(hUpdate, false))
                {
                    // TODO:
                    //results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", String.Format("Unable to finish updating resource for {0} with error {1:X}", filename, Marshal.GetHRForLastWin32Error())));
                    returnValue = false;
                }
            }

            return returnValue;
        }

        private byte[] StringToByteArray(string str)
        {
            byte[] strBytes = System.Text.Encoding.Unicode.GetBytes(str);
            byte[] data = new byte[strBytes.Length + 2];
            strBytes.CopyTo(data, 0);
            data[data.Length - 2] = 0;
            data[data.Length - 1] = 0;
            return data;
        }

        private class StringResource
        {
            public int Type;
            public string Name;
            public string Data;

            public StringResource(int type, string name, string data)
            {
                Type = type;
                Name = name;
                Data = data;
            }
        }

        private class FileResource
        {
            public string Filename;
            public string Key;

            public FileResource(string filename, string key)
            {
                Filename = filename;
                Key = key;
            }
        }
    }
}
