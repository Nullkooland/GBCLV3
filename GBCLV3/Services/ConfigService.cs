using System;
using System.IO;
using System.Text;
using System.Text.Json;
using GBCLV3.Models;
using GBCLV3.Models.Launcher;
using GBCLV3.Utils;

namespace GBCLV3.Services
{
    class ConfigService
    {
        #region Properties

        public Config Entries { get; set; }

        #endregion

        #region Private Members

        private const string CONFIG_FILENAME = "GBCL.json";

        #endregion

        #region Public Methods

        public void Load()
        {
            try
            {
                string json = File.ReadAllText(CONFIG_FILENAME, Encoding.UTF8);
                Entries = JsonSerializer.Deserialize<Config>(json);
            }
            catch
            {
                // Default configurations
                Entries = new Config
                {
                    Username = "Steve",
                    OfflineMode = true,
                    JavaMaxMem = SystemUtil.GetRecommendedMemory(),
                    WindowWidth = 854,
                    WindowHeight = 480,
                    AfterLaunch = AfterLaunchBehavior.Hide,
                    DownloadSource = DownloadSource.Official,
                };
            }

            if (string.IsNullOrWhiteSpace(Entries.GameDir))
            {
                Entries.GameDir = Environment.CurrentDirectory + "\\.minecraft";
            }

            if (string.IsNullOrWhiteSpace(Entries.JreDir))
            {
                Entries.JreDir = SystemUtil.GetJavaDir();
            }

            if (Entries.JavaMaxMem == 0)
            {
                Entries.JavaMaxMem = 2048;
            }

            Entries.Language = Entries.Language?.ToLower();
            if (string.IsNullOrWhiteSpace(Entries.Language))
            {
                Entries.Language = "zh-cn";
            }
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(Entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(CONFIG_FILENAME, json, Encoding.UTF8);
        }

        #endregion
    }
}
