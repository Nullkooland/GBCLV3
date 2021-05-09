using System;
using GBCLV3.Models.Auxiliary;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using Microsoft.VisualBasic.FileIO;
using StyletIoC;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Immutable;
using GBCLV3.Utils.Native;

namespace GBCLV3.Services.Auxiliary
{
    public class ModService
    {
        #region Private Fields

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly LogService _logService;

        #endregion

        #region Constructor

        [Inject]
        public ModService(GamePathService gamePathService, LogService logService)
        {
            _gamePathService = gamePathService;
            _logService = logService;
        }

        #endregion

        #region Public Methods

        public Task<ImmutableArray<Mod>> LoadAllAsync()
        {
            // Make sure directory exists
            Directory.CreateDirectory(_gamePathService.ModsDir);

            var query = Directory.EnumerateFiles(_gamePathService.ModsDir)
                .Where(file => file.EndsWith(".jar") || file.EndsWith(".jar.disabled"))
                .Select(path => Load(path))
                .OrderByDescending(mod => mod.IsEnabled);

            return Task.FromResult(query.ToImmutableArray());
        }

        public Task<ImmutableArray<Mod>> MoveLoadAllAsync(IEnumerable<string> paths, bool isCopy)
        {
            var query = paths.Select(path =>
            {
                string dstPath = $"{_gamePathService.ModsDir}/{Path.GetFileName(path)}";
                if (File.Exists(dstPath)) return null;

                var mod = Load(path);
                if (mod == null) return null;

                try
                {
                    if (isCopy)
                    {
                        File.Copy(path, dstPath);
                    }
                    else
                    {
                        File.Move(path, dstPath);
                    }

                    _logService.Info(nameof(ModService), $"Succeeded to {(isCopy ? "copy" : "move")} mod from \"{path}\"");
                }
                catch (IOException ex)
                {
                    // Maybe the file is being accessed by another process
                    _logService.Error(nameof(ModService), $"Failed to {(isCopy ? "copy" : "move")} mod from \"{path}\"\n{ex.Message}");
                    return null;
                }

                mod.Path = dstPath;

                return mod;
            }).Where(mod => mod != null);

            return Task.FromResult(query.ToImmutableArray());
        }

        public void ChangeExtension(Mod mod)
        {
            _logService.Info(nameof(ModService), $"Mod \"{mod.Id}\" is {(mod.IsEnabled ? "enabled" : "disabled")}");

            string newName = Path.GetFileNameWithoutExtension(mod.Path) + (mod.IsEnabled ? ".jar" : ".jar.disabled");
            FileSystem.RenameFile(mod.Path + (mod.IsEnabled ? ".disabled" : null), newName);
        }

        public void DeleteFromDisk(IEnumerable<Mod> mods)
        {
            var paths = mods.Select(mod => mod.Path + (mod.IsEnabled ? null : ".disabled"));
            RecycleBinUtil.Send(paths);

            var names = mods.Select(mod => mod.Id);
            _logService.Info(nameof(ModService), $"Mods deleted:\n{string.Join(", ", names)}");
        }

        #endregion

        #region Private Methods

        private Mod Load(string path)
        {
            Mod mod = null;

            try
            {
                using var archive = ZipFile.OpenRead(path);
                using var fabricModInfo = archive.GetEntry("fabric.mod.json")?.Open();
                using var forgeModInfo = archive.GetEntry("mcmod.info")?.Open();

                if (fabricModInfo != null)
                {
                    mod = LoadFabricMod(fabricModInfo);
                }
                else if (forgeModInfo != null)
                {
                    mod = LoadForgeMod(forgeModInfo);
                }
            }
            catch (Exception ex)
            {
                _logService.Error(nameof(ModService), $"Failed to load mod at \"{path}\"\n{ex.Message}");
            }

            // In case failed to load mod from info data
            mod ??= new Mod();

            mod.IsEnabled = path.EndsWith(".jar");
            mod.Path = mod.IsEnabled ? path : path[..^9];
            mod.Id ??= Path.GetFileNameWithoutExtension(mod.Path);
            mod.FileName = Path.GetFileName(mod.Path);

            mod.DisplayName = !string.IsNullOrWhiteSpace(mod.Description)
                ? mod.Description + (!string.IsNullOrEmpty(mod.Authors) ? $"\nby {mod.Authors}" : null)
                : mod.Id;

            _logService.Info(nameof(ModService), $"Mod \"{mod.Id}\" loaded");
            return mod;
        }

        private static Mod LoadFabricMod(Stream infoStream)
        {
            using var memoryStream = new MemoryStream();
            infoStream.CopyTo(memoryStream);

            var infoJson = CryptoUtil.RemoveUtf8BOM(memoryStream.ToArray());
            var fabricMod = JsonSerializer.Deserialize<FabricMod>(infoJson);
            var authorList = fabricMod?.authors.Select(element =>
            {
                if (element.ValueKind == JsonValueKind.String) return element.GetString();
                return element.TryGetProperty("name", out element) ? element.GetString() : null;
            });

            string authors = (fabricMod?.authors != null) ? string.Join(", ", authorList) : null;

            return new Mod
            {
                Id = fabricMod?.name,
                Description = fabricMod?.description.Split('.')[0], // Make it terse!
                Version = fabricMod?.version,
                Url = fabricMod?.contact?.homepage,
                Authors = authors,
            };
        }

        private static Mod LoadForgeMod(Stream infoStream)
        {
            using var memoryStream = new MemoryStream();
            infoStream.CopyTo(memoryStream);

            // This is utterly ugly...thanks to the capriciousness of modders
            var infoJson = CryptoUtil.RemoveUtf8BOM(memoryStream.ToArray());
            var forgeMod = JsonSerializer.Deserialize<ForgeMod[]>(infoJson)[0];

            if (forgeMod?.modList != null)
            {
                // I don't understand what are these modders thinking...
                forgeMod = forgeMod.modList[0];
            }

            var authorList = forgeMod?.authorList ?? forgeMod?.authors;
            string authors = authorList != null ? string.Join(", ", authorList) : null;

            return new Mod
            {
                Id = forgeMod?.name,
                Description = forgeMod?.description.Split('.')[0], // Make it terse!
                Version = forgeMod?.version,
                GameVersion = forgeMod?.mcversion,
                Url = forgeMod?.url,
                Authors = authors,
            };
        }

        #endregion
    }
}