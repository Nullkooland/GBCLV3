using System;
using GBCLV3.Models.Auxiliary;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using Microsoft.VisualBasic.FileIO;
using StyletIoC;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GBCLV3.Services.Auxiliary
{
    public class ModService
    {
        #region Private Fields

        // IoC
        private readonly GamePathService _gamePathService;

        #endregion

        #region Constructor

        [Inject]
        public ModService(GamePathService gamePathService)
        {
            _gamePathService = gamePathService;
        }

        #endregion

        #region Public Methods

        public IEnumerable<Mod> LoadAll()
        {
            // Make sure directory exists
            Directory.CreateDirectory(_gamePathService.ModsDir);

            return Directory.EnumerateFiles(_gamePathService.ModsDir)
                .Where(file => file.EndsWith(".jar") || file.EndsWith(".jar.disabled"))
                .Select(path => Load(path))
                .OrderByDescending(mod => mod.IsEnabled);
        }

        public async ValueTask<Mod[]> MoveLoadAllAsync(IEnumerable<string> paths, bool isCopy)
        {
            Directory.CreateDirectory(_gamePathService.ModsDir);

            var query = paths.Select(path =>
            {
                string dstPath = $"{_gamePathService.ModsDir}/{Path.GetFileName(path)}";
                if (File.Exists(dstPath)) return null;

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
                }
                catch (IOException ex)
                {
                    // Maybe the file is being accessed by another process
                    Debug.WriteLine(ex);
                    return null;
                }

                return Load(dstPath);
            }).Where(mod => mod != null);

            return await Task.FromResult(query.ToArray());
        }

        public void ChangeExtension(Mod mod)
        {
            string newName = Path.GetFileNameWithoutExtension(mod.Path) + (mod.IsEnabled ? ".jar" : ".jar.disabled");
            FileSystem.RenameFile(mod.Path + (mod.IsEnabled ? ".disabled" : null), newName);
        }

        public async Task DeleteFromDiskAsync(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
            {
                await SystemUtil.SendFileToRecycleBinAsync(mod.Path + (mod.IsEnabled ? null : ".disabled"));
            }
        }

        #endregion

        #region Private Methods

        private static Mod Load(string path)
        {
            using var archive = ZipFile.OpenRead(path);
            using var fabricModInfo = archive.GetEntry("fabric.mod.json")?.Open();
            using var forgeModInfo = archive.GetEntry("mcmod.info")?.Open();

            Mod mod;

            if (fabricModInfo != null)
            {
                mod = LoadFabricMods(fabricModInfo);
            }
            else if (forgeModInfo != null)
            {
                mod = LoadForgeMods(forgeModInfo);
            }
            else
            {
                mod = new Mod();
            }

            mod.IsEnabled = path.EndsWith(".jar");
            mod.Path = mod.IsEnabled ? path : path[..^9];
            mod.Name ??= Path.GetFileNameWithoutExtension(mod.Path);
            mod.FileName = Path.GetFileName(mod.Path);

            mod.DisplayName = !string.IsNullOrWhiteSpace(mod.Description)
                ? mod.Description + (!string.IsNullOrEmpty(mod.Authors) ? $"\nby {mod.Authors}" : null)
                : mod.Name;

            return mod;
        }

        private static Mod LoadFabricMods(Stream infoStream)
        {
            using var memoryStream = new MemoryStream();
            infoStream.CopyTo(memoryStream);

            Mod mod = null;

            try
            {
                var infoJson = SystemUtil.RemoveUtf8BOM(memoryStream.ToArray());
                var fabricMod = JsonSerializer.Deserialize<FabricMod>(infoJson);
                string authors = (fabricMod?.authors != null) ? string.Join(", ", fabricMod.authors) : null;

                mod = new Mod
                {
                    Name = fabricMod?.name,
                    Description = fabricMod?.description.Split('.')[0], // Make it terse!
                    Version = fabricMod?.version,
                    Url = fabricMod?.contact?.homepage,
                    Authors = authors,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return mod;
        }

        private static Mod LoadForgeMods(Stream infoStream)
        {
            using var memoryStream = new MemoryStream();
            infoStream.CopyTo(memoryStream);

            Mod mod = null;

            try
            {
                // This is utterly ugly...thanks to the capriciousness of modders
                var infoJson = SystemUtil.RemoveUtf8BOM(memoryStream.ToArray());
                var forgeMod = JsonSerializer.Deserialize<ForgeMod[]>(infoJson)[0];

                if (forgeMod?.modList != null)
                {
                    // I don't understand what are these modders thinking...
                    forgeMod = forgeMod.modList[0];
                }

                var authorList = forgeMod?.authorList ?? forgeMod?.authors;
                string authors = authorList != null ? string.Join(", ", authorList) : null;

                mod = new Mod
                {
                    Name = forgeMod?.name,
                    Description = forgeMod?.description.Split('.')[0], // Make it terse!
                    Version = forgeMod?.version,
                    GameVersion = forgeMod?.mcversion,
                    Url = forgeMod?.url,
                    Authors = authors,
                };
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return mod;
        }

        #endregion
    }
}