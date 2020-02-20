using GBCLV3.Models;
using GBCLV3.Models.Authentication;
using GBCLV3.Models.Download;
using GBCLV3.Models.Launch;
using GBCLV3.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
                var json = SystemUtil.ReadUtf8File(CONFIG_FILENAME);
                Entries = JsonSerializer.Deserialize<Config>(json);
            }
            catch
            {
                // Default configurations
                Entries = new Config
                {
                    JavaMaxMem = NativeUtil.GetRecommendedMemory(),
                    WindowWidth = 854,
                    WindowHeight = 480,
                    AfterLaunch = AfterLaunchBehavior.Hide,
                    DownloadSource = DownloadSource.Official,
                };
            }

            Entries.Accounts ??= new List<Account>(4);

            if (string.IsNullOrWhiteSpace(Entries.GameDir))
            {
                Entries.GameDir = Environment.CurrentDirectory + "\\.minecraft";
            }

            if (!File.Exists(Entries.JreDir + "\\javaw.exe"))
            {
                Entries.JreDir = SystemUtil.GetJavaDir();
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
    }
}
