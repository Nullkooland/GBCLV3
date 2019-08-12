using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using GBCLV3.Models;
using GBCLV3.Models.JsonClasses;
using GBCLV3.Services.Launcher;

namespace GBCLV3.Services
{
    class ResourcePackService
    {
        #region Private Members

        // IoC
        private readonly GamePathService _gamePathService;

        #endregion

        #region Constructor

        public ResourcePackService(GamePathService gamePathService)
        {
            _gamePathService = gamePathService;
        }

        #endregion


        #region Public Methods

        public IEnumerable<ResourcePack> GetAll()
        {
            if (!Directory.Exists(_gamePathService.ResourcePackDir))
            {
                Directory.CreateDirectory(_gamePathService.ResourcePackDir);
                return null;
            }

            string optionsFile = _gamePathService.WorkingDir + "/options.txt";
            string[] enabledPacks = null;

            if (File.Exists(optionsFile))
            {
                using (var reader = new StreamReader(optionsFile, Encoding.Default))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("resourcePacks"))
                        {
                            // Extract “resourcePacks:[${enabledPacks}]”
                            enabledPacks = line.Substring(15, line.Length - 16).Split(',');
                            break;
                        }
                    }
                }
            }

            return Directory.EnumerateFiles(_gamePathService.ResourcePackDir, "*.zip")
                            .Select(path => LoadZip(path, enabledPacks))
                            .Concat(Directory.EnumerateDirectories(_gamePathService.ResourcePackDir)
                                             .Select(dir => LoadDir(dir, enabledPacks)))
                            .Where(pack => pack != null);
        }

        public bool WriteToOptions(IEnumerable<ResourcePack> packs)
        {
            string optionsPath = _gamePathService.WorkingDir + "/options.txt";

            if (!File.Exists(optionsPath))
            {
                return false;
            }

            string enabledPacks = string.Join(",", packs.Where(pack => pack.IsEnabled)
                                                        .Select(pack => $"\"{pack.Name}\""));

            string options = File.ReadAllText(optionsPath, Encoding.Default);

            if (options.Contains("resourcePacks:["))
            {
                options = Regex.Replace(options, "resourcePacks:\\[.*\\]", $"resourcePacks:[{enabledPacks}]");
            }
            else
            {
                options += $"resourcePacks:[{enabledPacks}]\r\n";
            }

            File.WriteAllText(optionsPath, options, Encoding.Default);
            return true;
        }

        #endregion

        #region Private Methods

        private static ResourcePack LoadZip(string path, string[] enabledPacks)
        {
            using (var archive = ZipFile.OpenRead(path))
            {
                ZipArchiveEntry infoEntry;
                if ((infoEntry = archive.GetEntry("pack.mcmeta")) == null)
                {
                    return null;
                }

                var pack = ReadInfo(infoEntry.Open());
                pack.Path = path;
                pack.IsEnabled = enabledPacks?.Contains($"\"{pack.Name}\"") ?? false;

                // Load cover image (if exists)
                ZipArchiveEntry imgEntry;
                if ((imgEntry = archive.GetEntry("pack.png")) != null)
                {
                    using (var es = imgEntry.Open())
                    using (var ms = new MemoryStream())
                    {
                        es.CopyTo(ms);
                        pack.Image = ReadImage(ms);
                    }
                }

                return pack;
            }
        }

        private static ResourcePack LoadDir(string packDir, string[] enabledPacks)
        {
            string infoPath = packDir + "/pack.mcmeta";
            string imgPath = packDir + "/pack.png";

            if (!File.Exists(infoPath))
            {
                return null;
            }

            var pack = ReadInfo(File.OpenRead(infoPath));
            pack.Path = packDir;
            pack.IsEnabled = enabledPacks?.Contains($"\"{pack.Name}\"") ?? false;

            // Load cover image (if exists)
            if (File.Exists(imgPath))
            {
                using (var fs = File.OpenRead(imgPath))
                {
                    pack.Image = ReadImage(fs);
                }
            }

            return pack;
        }

        private static ResourcePack ReadInfo(Stream infoStream)
        {
            string json = new StreamReader(infoStream, Encoding.UTF8).ReadToEnd();
            var info = JsonSerializer.Deserialize<JResourcePack>(json);

            return new ResourcePack
            {
                Format = info.pack.pack_format,
                Description = info.pack.description,
                IsExtracted = true,
            };
        }

        private static BitmapImage ReadImage(Stream imgStream)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = imgStream;
            img.DecodePixelWidth = 128;
            img.DecodePixelHeight = 128;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();

            return img;
        }

        #endregion
    }
}
