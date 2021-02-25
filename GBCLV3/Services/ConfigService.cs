using GBCLV3.Models;
using GBCLV3.Models.Authentication;
using GBCLV3.Models.Download;
using GBCLV3.Models.Launch;
using GBCLV3.Models.Theme;
using GBCLV3.Utils;
using GBCLV3.Utils.Native;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GBCLV3.Services
{
    public class ConfigService
    {
        #region Properties

        public Config Entries { get; set; }

        #endregion

        #region Private Fields

        private const string CONFIG_FILENAME = "GBCL.json";

        #endregion

        #region Public Methods

        public void Load()
        {
            try
            {
                var jsonData = File.ReadAllBytes(CONFIG_FILENAME);
                var json = CryptoUtil.RemoveUtf8BOM(jsonData);
                Entries = JsonSerializer.Deserialize<Config>(json);
            }
            catch
            {
                // Default configurations
                Entries = new Config
                {
                    JavaMaxMem = MemoryStatusUtil.GetRecommendedMemoryMB(),
                    WindowWidth = 854,
                    WindowHeight = 480,
                    AfterLaunch = AfterLaunchBehavior.Hide,
                    DownloadSource = DownloadSource.Official,
                    BackgroundEffect = BackgroundEffect.BlurBehind,
                };
            }

            Entries.Accounts ??= new List<Account>(4);

            if (string.IsNullOrWhiteSpace(Entries.GameDir))
            {
                Entries.GameDir = Environment.CurrentDirectory + "\\.minecraft";
            }

            if (!File.Exists(Entries.JreDir + "\\javaw.exe"))
            {
                Entries.JreDir = LocateJRE();
            }

            if (Entries.JavaMaxMem == 0)
            {
                Entries.JavaMaxMem = 2048;
            }

            if (Entries.Build < 104)
            {
                Entries.Language = null; // Migrate lower version language settings
            }

            Entries.Build = AssemblyUtil.Build;
        }

        public void Save()
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(Entries,
                new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });
            File.WriteAllBytes(CONFIG_FILENAME, json);
        }

        #endregion

        #region Helper Methods

        private static string LocateJRE()
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
                //Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        #endregion
    }
}
