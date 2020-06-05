using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace MageCore
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = Assembly.GetExecutingAssembly().FullName;
            //string path = @"D:\_clickonce\binaries\Launcher.modified.exe";
            string path = @"C:\Users\nikolam\source\repos\PEResources\Launcher\bin\Debug\Launcher.exe";

            // first, try to write it
            ResourceUpdater updater = new ResourceUpdater();
            updater.AddStringResource(NativeMethods.CUSTOM_RESOURCE_TYPE, NativeMethods.BINARYTOLAUNCH_STRING, "1235.dll");
            updater.UpdateResources(path);

            // now try to read it
            //string a = ResourceReader.GetBinaryToLaunch(path);
        }
    }
}
