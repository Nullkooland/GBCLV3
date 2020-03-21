using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GBCLV3.Utils
{
    public static class SystemUtil
    {
        public static ReadOnlySpan<byte> ReadUtf8File(string path)
        {
            ReadOnlySpan<byte> data = File.ReadAllBytes(path);

            // Read past the UTF-8 BOM bytes if a BOM exists.
            if (data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            {
                return data[3..];
            }

            return data;
        }

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

        public static async ValueTask SendDirToRecycleBinAsync(string path)
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

        public static async ValueTask SendFileToRecycleBinAsync(string path)
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