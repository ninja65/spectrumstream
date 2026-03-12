using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TyphoonClient.APIs
{
    public class TyphoonSDK
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDllDirectory(string lpPathName);

        public static void Setup()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var exeFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) ?? Directory.GetCurrentDirectory();
                SetDllDirectory(Path.Combine(exeFolder, "TyphoonSDK"));
            }
        }
    }
}
