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
        private bool _isNewOptionsFormat = false;

        #endregion

        #region Constructor

        [Inject]
        public ResourcePackService(GamePathService gamePathService)
        {
            _gamePathService = gamePathService;
        }

        #endregion

        #region Public Methods

        public (IEnumerable<ResourcePack> enabled, IEnumerable<ResourcePack> disabled) LoadAll()
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
                            .Select(id =>
                            {
                                if (string.IsNullOrWhiteSpace(id))
                                {
                                    return null;
                                }

                                id = id[1..^1];

                                if (id == "vanilla")
                                {
                                    _isNewOptionsFormat = true;
                                }

                                if (id.StartsWith("file/"))
                                {
                                    id = id[5..];
                                }

                                return id;
                            })
                            .Where(id => id != null)
                            .ToArray();
                        break;
                    }
                }
            }

            // Make sure directory exists
            Directory.CreateDirectory(_gamePathService.ResourcePacksDir);

            var packs = Directory.EnumerateFileSystemEntries(_gamePathService.ResourcePacksDir)
                .Select(path => Load(path, enabledPackIDs))
                .Where(pack => pack != null)
                .ToLookup(pack => pack.IsEnabled);

            // Enabled resourcepacks (followed the order in options)
            return (packs[true].OrderByDescending(pack => Array.IndexOf(enabledPackIDs, pack.Name)),
                // Disabled resourcepacks
                packs[false].OrderBy(pack => pack.Name));
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

            string enabledPackIDs =
                string.Join(",", enabledPacks.Reverse().Select(pack =>
                {
                    if (!_isNewOptionsFormat) return $"\"{pack.Name}\"";
                    //if (pack.Name == "vanilla" || pack.Name == "programmer_art") return pack.Name;
                    return $"\"file/{pack.Name}\"";
                }));


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
            return pack.IsExtracted
                ? SystemUtil.SendDirToRecycleBinAsync(pack.Path)
                : SystemUtil.SendFileToRecycleBinAsync(pack.Path);
        }

        public async ValueTask<ResourcePack[]> MoveLoadAllAsync(IEnumerable<string> paths, bool isEnabled)
        {
            Directory.CreateDirectory(_gamePathService.ResourcePacksDir);

            var query = paths.Select(path =>
            {
                string dstPath = $"{_gamePathService.ResourcePacksDir}/{Path.GetFileName(path)}";
                if (File.Exists(dstPath)) return null;

                var pack = Load(path, null);
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

        private static ResourcePack Load(string path, string[] enabledPackIds)
        {
            bool isZip = path.EndsWith(".zip");

            using var infoMemStream = new MemoryStream();
            using var imgMemStream = new MemoryStream();

            if (isZip)
            {
                using var archive = ZipFile.OpenRead(path);

                var infoEntry = archive.GetEntry("pack.mcmeta");
                if (infoEntry == null)
                {
                    return null;
                }

                using var infoStream = infoEntry.Open();
                infoStream.CopyTo(infoMemStream);

                var imgEntry = archive.GetEntry("pack.png");
                if (imgEntry != null)
                {
                    using var imgStream = imgEntry.Open();
                    imgStream.CopyTo(imgMemStream);
                }
            }
            else
            {
                string infoFile = path + "/pack.mcmeta";
                if (!File.Exists(infoFile))
                {
                    return null;
                }

                using var infoStream = File.OpenRead(infoFile);
                infoStream.CopyTo(infoMemStream);


                string imgFile = path + "/pack.png";
                if (File.Exists(imgFile))
                {
                    using var imgStream = File.OpenRead(imgFile);
                    imgStream.CopyTo(imgMemStream);
                }
            }

            var infoJson = SystemUtil.RemoveUtf8BOM(infoMemStream.ToArray());
            var info = JsonSerializer.Deserialize<JResourcePack>(infoJson);

            return new ResourcePack
            {
                Path = path,
                IsEnabled = enabledPackIds?.Contains(Path.GetFileName(path)) ?? false,
                IsExtracted = !isZip,
                Format = info.pack?.pack_format ?? -1,
                Description = Regex.Replace(info.pack?.description ?? string.Empty, "§.", string.Empty),
                Image = ReadImage(imgMemStream),
            };
        }

        private static BitmapImage ReadImage(MemoryStream imgStream)
        {
            if (imgStream.Length == 0) return null;

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