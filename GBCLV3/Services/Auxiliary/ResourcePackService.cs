using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GBCLV3.Models.Auxiliary;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using GBCLV3.Utils.Native;
using StyletIoC;

namespace GBCLV3.Services.Auxiliary
{
    public class ResourcePackService
    {
        #region Private Fields

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly LogService _logService;

        private bool _isNewOptionsFormat = false;

        #endregion

        #region Constructor

        [Inject]
        public ResourcePackService(GamePathService gamePathService, LogService logService)
        {
            _gamePathService = gamePathService;
            _logService = logService;
        }

        #endregion

        #region Public Methods

        public async Task<ImmutableArray<ResourcePack>> LoadAllAsync()
        {
            string optionsFile = _gamePathService.WorkingDir + "/options.txt";
            string[] enabledPacksIds = null;

            if (File.Exists(optionsFile))
            {
                using var reader = new StreamReader(optionsFile, Encoding.Default);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("resourcePacks"))
                    {
                        // Extract “resourcePacks:[${enabledPackIDs}]”
                        enabledPacksIds = line[15..^1]
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

            var query = Directory.EnumerateFileSystemEntries(_gamePathService.ResourcePacksDir)
                .Select(path => Load(path, enabledPacksIds))
                .Where(pack => pack != null);

            var packs = await Task.FromResult(query.ToLookup(pack => pack.IsEnabled));

            var enabledPacks = packs[true].OrderByDescending(pack => Array.IndexOf(enabledPacksIds, pack.Id));
            var disabledPacks = packs[false].OrderBy(pack => pack.Id);

            return enabledPacks.Concat(disabledPacks).ToImmutableArray();
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
                    if (!_isNewOptionsFormat)
                    {
                        return $"\"{pack.Id}\"";
                    }
                    //if (pack.Name == "vanilla" || pack.Name == "programmer_art") return pack.Name;
                    return $"\"file/{pack.Id}\"";
                }));


            if (options.Contains("resourcePacks:["))
            {
                options = Regex.Replace(options, "resourcePacks:\\[.*\\]", $"resourcePacks:[{enabledPackIDs}]");
            }
            else
            {
                options += $"resourcePacks:[{enabledPackIDs}]\r\n";
            }

            _logService.Info(nameof(ResourcePackService), $"Selected resourcepacks:\n{enabledPackIDs}");

            File.WriteAllText(optionsPath, options, Encoding.Default);

            _logService.Info(nameof(ResourcePackService), $"Wrote user selections into options.txt");

            return true;
        }

        public void DeleteFromDisk(ResourcePack pack)
        {
            _logService.Info(nameof(ResourcePackService), $"Pack \"{pack.Id}\" deleted");

            RecycleBinUtil.Send(Enumerable.Repeat(pack.Path, 1));
        }

        public Task<ImmutableArray<ResourcePack>> MoveLoadAllAsync(IEnumerable<string> paths, bool isEnabled, bool isCopy)
        {
            var query = paths.Select(path =>
            {
                string dstPath = $"{_gamePathService.ResourcePacksDir}/{Path.GetFileName(path)}";
                if (File.Exists(dstPath))
                {
                    return null;
                }

                var pack = Load(path);
                if (pack == null)
                {
                    return null;
                }

                try
                {
                    // It is a valid resourcepack and has been successfully loaded, move or copy it into target dir
                    if (isCopy)
                    {
                        File.Copy(path, dstPath);
                    }
                    else
                    {
                        File.Move(path, dstPath);
                    }

                    _logService.Info(nameof(ResourcePackService), $"Succeeded to {(isCopy ? "copy" : "move")} resourcepack from \"{path}\"");
                }
                catch (IOException ex)
                {
                    // Maybe the file is being accessed by another process
                    _logService.Error(nameof(ResourcePackService), $"Failed to {(isCopy ? "copy" : "move")} resourcepack from \"{path}\"\n{ex.Message}");
                    return null;
                }

                // Modify properties
                pack.Path = dstPath;
                pack.IsEnabled = isEnabled;

                return pack;
            }).Where(pack => pack != null);

            return Task.FromResult(query.ToImmutableArray());
        }

        #endregion

        #region Private Methods

        private ResourcePack Load(string path, string[] enabledPacksIds = null)
        {
            string id = Path.GetFileName(path);
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

            try
            {
                var infoJson = CryptoUtil.RemoveUtf8BOM(infoMemStream.ToArray());
                var info = JsonSerializer.Deserialize<JResourcePack>(infoJson);

                _logService.Info(nameof(ResourcePackService), $"Pack \"{id}\" loaded");

                return new ResourcePack
                {
                    Id = id,
                    Path = path,
                    IsEnabled = enabledPacksIds?.Contains(id) ?? false,
                    IsExtracted = !isZip,
                    Format = info.pack?.pack_format ?? -1,
                    Description = Regex.Replace(info.pack?.description ?? string.Empty, "§.", string.Empty),
                    Image = ReadImage(imgMemStream),
                };
            }
            catch (JsonException ex)
            {
                _logService.Error(nameof(ResourcePackService), $"Failed to read info in pack \"{id}\"\n{ex.Message}");
            }

            return null;
        }

        private static BitmapImage ReadImage(MemoryStream imgStream)
        {
            if (imgStream.Length == 0)
            {
                return null;
            }

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