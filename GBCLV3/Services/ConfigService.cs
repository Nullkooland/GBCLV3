using GBCLV3.Models;
using GBCLV3.Models.Authentication;
using GBCLV3.Models.Download;
using GBCLV3.Models.Launch;
using GBCLV3.Models.Theme;
using GBCLV3.Utils;
using GBCLV3.Utils.Native;
using Microsoft.Win32;
using StyletIoC;
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
        private readonly LogService _logService;

        #endregion

        #region Constructor

        [Inject]
        public ConfigService(LogService logService)
        {
            _logService = logService;
        }

        #endregion

        #region Public Methods

        public void Load()
        {
            _logService.Info(nameof(ConfigService), "Loading config json");

            try
            {
                var jsonData = File.ReadAllBytes(CONFIG_FILENAME);
                var json = CryptoUtil.RemoveUtf8BOM(jsonData);
                Entries = JsonSerializer.Deserialize<Config>(json);
            }
            catch (Exception ex)
            {
                _logService.Error(nameof(ConfigService), $"Failed to load config json.\n{ex.Message}");

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

            _logService.Info(nameof(ConfigService), $"Minecraft root dir: \"{Entries.GameDir}\"");

            if (!File.Exists(Entries.JreDir + "\\javaw.exe"))
            {
                Entries.JreDir = LocateJRE();
            }

            _logService.Info(nameof(ConfigService), $"JRE path: \"{Entries.JreDir}\"");

            if (Entries.JavaMaxMem == 0)
            {
                Entries.JavaMaxMem = MemoryStatusUtil.GetRecommendedMemoryMB();
            }

            _logService.Info(nameof(ConfigService), $"JRE max memeory (MB): {Entries.JavaMaxMem}");

            if (Entries.Build < 104)
            {
                Entries.Language = null; // Migrate lower version language settings
            }

            Entries.Build = AssemblyUtil.Build;
        }

        public void Save()
        {
            _logService.Info(nameof(ConfigService), "Saving config json");

            var json = JsonSerializer.SerializeToUtf8Bytes(Entries,
                new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

            try
            {
                File.WriteAllBytes(CONFIG_FILENAME, json);
            }
            catch(Exception ex)
            {
                _logService.Error(nameof(ConfigService), $"Failed to save config json.\n{ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private string LocateJRE()
        {
            _logService.Info(nameof(ConfigService), "Trying to located JRE from registry");

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
                _logService.Error(nameof(ConfigService), $"Failed to located JRE from registry.\n{ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
