using GBCLV3.Models.Auxiliary;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GBCLV3.Services.Auxiliary
{
    class ResourcePackService
    {
        #region Private Members

        // IoC
        private readonly GamePathService _gamePathService;

        #endregion

        #region Constructor

        [Inject]
        public ResourcePackService(GamePathService gamePathService)
        {
            _gamePathService = gamePathService;
        }

        #endregion

        #region Public Methods

        public (IEnumerable<ResourcePack> enabled, IEnumerable<ResourcePack> disabled) GetAll()
        {
            string optionsFile = _gamePathService.WorkingDir + "/options.txt";
            string[] enabledPackIDs = null;

            if (File.Exists(optionsFile))
            {
                using var reader = new StreamReader(optionsFile, Encoding.Default);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("resourcePacks"))
                    {
                        // Extract “resourcePacks:[${enabledPackIDs}]”
                        enabledPackIDs = line[15..^1]
                                             .Split(',')
                                             .Select(id => id.Trim('\"'))
                                             .ToArray();
                        break;
                    }
                }
            }

            // Make sure directory exists
            Directory.CreateDirectory(_gamePathService.ResourcePacksDir);

            var packs = Directory.EnumerateFiles(_gamePathService.ResourcePacksDir, "*.zip")
                                 .Select(path => LoadZip(path, enabledPackIDs))
                                 .Concat(Directory.EnumerateDirectories(_gamePathService.ResourcePacksDir)
                                                  .Select(dir => LoadDir(dir, enabledPackIDs)))
                                 .Where(pack => pack != null)
                                 .ToLookup(pack => pack.IsEnabled);

            // Enabled resourcepacks (followed the order in options)
            return (packs[true].OrderByDescending(pack => Array.IndexOf(enabledPackIDs, pack.Name)),
                    // Disabled resourcepacks
                    packs[false]);
        }

        public bool WriteToOptions(IEnumerable<ResourcePack> enabledPacks)
        {
            string optionsPath = _gamePathService.WorkingDir + "/options.txt";

            if (!File.Exists(optionsPath))
            {
                return false;
            }

            string enabledPackIDs = string.Join(",", enabledPacks.Reverse().Select(pack => $"\"{pack.Name}\""));
            string options = File.ReadAllText(optionsPath, Encoding.Default);

            if (options.Contains("resourcePacks:["))
            {
                options = Regex.Replace(options, "resourcePacks:\\[.*\\]", $"resourcePacks:[{enabledPackIDs}]");
            }
            else
            {
                options += $"resourcePacks:[{enabledPackIDs}]\r\n";
            }

            File.WriteAllText(optionsPath, options, Encoding.Default);
            return true;
        }

        public async Task DeleteFromDiskAsync(ResourcePack pack)
        {
            if (pack.IsExtracted) await SystemUtil.SendDirToRecycleBin(pack.Path);
            else await SystemUtil.SendFileToRecycleBin(pack.Path);
        }

        public async Task<IEnumerable<ResourcePack>> MoveLoadAll(IEnumerable<string> paths)
        {
            return await Task.Run(() =>
                paths.Select(path =>
                {
                    string dstPath = $"{_gamePathService.ResourcePacksDir}/{Path.GetFileName(path)}";
                    if (File.Exists(dstPath)) return null;

                    var pack = LoadZip(dstPath, null);
                    if (pack != null)
                    {
                        File.Move(path, dstPath);
                        return pack;
                    }
                    else
                    {
                        return null;
                    }
                })
                .Where(pack => pack != null)
                .ToList()
            );
        }

        #endregion

        #region Private Methods

        public bool IsValid(string path)
        {
            try
            {
                using var archive = ZipFile.OpenRead(path);
                return archive.GetEntry("pack.mcmeta") != null;
            }
            catch
            {
                return false;
            }
        }

        private static ResourcePack LoadZip(string path, string[] enabledPackIDs)
        {
            using var archive = ZipFile.OpenRead(path);
            ZipArchiveEntry infoEntry;
            if ((infoEntry = archive.GetEntry("pack.mcmeta")) == null)
            {
                return null;
            }

            var pack = ReadInfo(infoEntry.Open());
            pack.Path = path;
            pack.IsEnabled = enabledPackIDs?.Contains(pack.Name) ?? false;
            pack.IsExtracted = false;

            // Load cover image (if exists)
            ZipArchiveEntry imgEntry;
            if ((imgEntry = archive.GetEntry("pack.png")) != null)
            {
                using var es = imgEntry.Open();
                using var ms = new MemoryStream();
                es.CopyTo(ms);
                pack.Image = ReadImage(ms);
            }

            return pack;
        }

        private static ResourcePack LoadDir(string packDir, string[] enabledPackIDs)
        {
            string infoPath = packDir + "/pack.mcmeta";
            string imgPath = packDir + "/pack.png";

            if (!File.Exists(infoPath))
            {
                return null;
            }

            var pack = ReadInfo(File.OpenRead(infoPath));
            pack.Path = packDir;
            pack.IsEnabled = enabledPackIDs?.Contains(pack.Name) ?? false;
            pack.IsExtracted = true;

            // Load cover image (if exists)
            if (File.Exists(imgPath))
            {
                using var fs = File.OpenRead(imgPath);
                pack.Image = ReadImage(fs);
            }

            return pack;
        }

        private static ResourcePack ReadInfo(Stream infoStream)
        {
            using var reader = new StreamReader(infoStream, Encoding.UTF8);
            var info = JsonSerializer.Deserialize<JResourcePack>(reader.ReadToEnd());

            return new ResourcePack
            {
                Format = info.pack.pack_format,
                Description = info.pack.description,
            };
        }

        private static BitmapImage ReadImage(Stream imgStream)
        {
            var img = new BitmapImage();
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
