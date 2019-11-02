using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

namespace GBCLV3.Utils
{
    static class SystemUtil
    {
        public static string GetJavaDir()
        {
            try
            {
                using var localMachineKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var javaKey = localMachineKey.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment\");

                string currentVersion = javaKey.GetValue("CurrentVersion").ToString();
                using var subkey = javaKey.OpenSubKey(currentVersion);
                return subkey.GetValue("JavaHome").ToString() + @"\bin";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public static void OpenLink(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }

        public static uint GetAvailableMemory()
        {
            using var counter = new PerformanceCounter("Memory", "Available MBytes", true);
            return (uint)counter.NextValue();
        }

        public static uint GetRecommendedMemory() => (uint)Math.Pow(2.0, Math.Floor(Math.Log(GetAvailableMemory(), 2.0)));

        public static async Task SendDirToRecycleBin(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            await Task.Run(() =>
            {
                FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            });
        }

        public static async Task SendFileToRecycleBin(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            await Task.Run(() =>
            {
                FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            });
        }

        public static void DeleteEmptyDirs(string dir)
        {
            if (!Directory.Exists(dir)) return;

            while (!Directory.EnumerateFileSystemEntries(dir).Any())
            {
                Directory.Delete(dir);
                dir = Path.GetDirectoryName(dir);
            }
        }
    }
}