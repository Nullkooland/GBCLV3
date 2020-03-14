using GBCLV3.Models.Auxiliary;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class ResourcePackService
    {
        #region Private Fields

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
            string options;

            if (File.Exists(optionsPath))
            {
                options = File.ReadAllText(optionsPath, Encoding.Default);
            }
            else
            {
                options = "resourcePacks:[]";
            }

            string enabledPackIDs = string.Join(",", enabledPacks.Reverse().Select(pack => $"\"{pack.Name}\""));


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

        public ValueTask DeleteFromDiskAsync(ResourcePack pack)
        {
            return pack.IsExtracted ?
                SystemUtil.SendDirToRecycleBinAsync(pack.Path) : SystemUtil.SendFileToRecycleBinAsync(pack.Path);
        }

        public async ValueTask<ResourcePack[]> MoveLoadAllAsync(IEnumerable<string> paths, bool isEnabled)
        {
            Directory.CreateDirectory(_gamePathService.ResourcePacksDir);

            var query = paths.Select(path =>
            {
                string dstPath = $"{_gamePathService.ResourcePacksDir}/{Path.GetFileName(path)}";
                if (File.Exists(dstPath)) return null;

                var pack = LoadZip(path, null);
                if (pack == null) return null;

                try
                {
                    // It is a valid resourcepack and has been successfully loaded, move it into target dir
                    File.Move(path, dstPath);
                }
                catch (IOException ex)
                {
                    // Maybe the file is being accessed by another process
                    Debug.WriteLine(ex);
                    return null;
                }
                
                // Modify properties
                pack.Path = dstPath;
                pack.IsEnabled = isEnabled;

                return pack;
            }).Where(pack => pack != null);

            return await Task.FromResult(query.ToArray());
        }

        #endregion

        #region Private Methods

        private static ResourcePack LoadZip(string path, string[] enabledPackIDs)
        {
            using var archive = ZipFile.OpenRead(path);
            ZipArchiveEntry infoEntry;
            if ((infoEntry = archive.GetEntry("pack.mcmeta")) == null)
            {
                return null;
            }

            using var infoStream = infoEntry.Open();

            var pack = ReadInfo(infoStream);
            pack.Path = path;
            pack.IsEnabled = enabledPackIDs?.Contains(pack.Name) ?? false;
            pack.IsExtracted = false;

            // LoadSkinsAsync cover image (if exists)
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

            using var infoStream = File.OpenRead(infoPath);

            var pack = ReadInfo(infoStream);
            pack.Path = packDir;
            pack.IsEnabled = enabledPackIDs?.Contains(pack.Name) ?? false;
            pack.IsExtracted = true;

            // LoadSkinsAsync cover image (if exists)
            if (File.Exists(imgPath))
            {
                using var fs = File.OpenRead(imgPath);
                pack.Image = ReadImage(fs);
            }

            return pack;
        }

        private static ResourcePack ReadInfo(Stream infoStream)
        {
            using var ms = new MemoryStream();
            infoStream.CopyTo(ms);
            var info = JsonSerializer.Deserialize<JResourcePack>(ms.ToArray());

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
