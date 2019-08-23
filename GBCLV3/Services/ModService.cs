using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GBCLV3.Models;
using GBCLV3.Models.JsonClasses;
using GBCLV3.Services.Launcher;
using GBCLV3.Utils;
using Microsoft.VisualBasic.FileIO;
using StyletIoC;

namespace GBCLV3.Services
{
    class ModService
    {
        #region Private Members

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

        public IEnumerable<Mod> GetAll()
        {
            if (!Directory.Exists(_gamePathService.ModsDir))
            {
                Directory.CreateDirectory(_gamePathService.ModsDir);
                return null;
            }

            return Directory.EnumerateFiles(_gamePathService.ModsDir)
                            .Where(file => file.EndsWith(".jar") || file.EndsWith(".jar.disabled"))
                            .Select(path => Load(path))
                            .OrderByDescending(mod => mod.IsEnabled);
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
                await SystemUtil.SendFileToRecycleBin(mod.Path + (mod.IsEnabled ? null : ".disabled"));
            }
        }

        public async Task<IEnumerable<Mod>> MoveLoadAll(IEnumerable<string> paths)
        {
            return await Task.Run(() =>
                paths.Select(path =>
                {
                    var dstPath = $"{_gamePathService.ModsDir}/{Path.GetFileName(path)}";
                    if (File.Exists(dstPath)) return null;

                    File.Move(path, dstPath);
                    return Load(dstPath);
                })
                .Where(mod => mod != null)
                .ToList()
            );
        }

        #endregion

        #region Private Methods

        private static Mod Load(string path)
        {
            using (var archive = ZipFile.OpenRead(path))
            {
                bool isEnabled = path.EndsWith(".jar");
                if (!isEnabled) path = path.Substring(0, path.Length - 9);

                var info = archive.GetEntry("mcmod.info");
                if (info != null)
                {
                    using (var reader = new StreamReader(info.Open(), Encoding.UTF8))
                    {
                        JMod jmod = null;

                        try
                        {
                            // This is utterly ugly...thanks to the capriciousness of modders
                            jmod = JsonSerializer.Deserialize<JMod[]>(reader.ReadToEnd())[0];
                        }
                        catch (JsonException ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }

                        if (jmod?.modList != null)
                        {
                            // I don't understand what are these modders thinking...
                            jmod = jmod.modList[0];
                        }

                        string[] authorList = jmod?.authorList ?? jmod?.authors;
                        string auhtors = (authorList != null) ? string.Join(", ", authorList) : null;

                        return new Mod
                        {
                            Name = jmod?.name ?? Path.GetFileNameWithoutExtension(path),
                            FileName = Path.GetFileName(path),
                            Description = jmod?.description.Split('.')[0], // Make it terse!
                            Version = jmod?.version,
                            GameVersion = jmod?.mcversion,
                            Url = jmod?.url,
                            Authors = auhtors,
                            Path = path,
                            IsEnabled = isEnabled,
                        };
                    }
                }

                return new Mod
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    FileName = Path.GetFileName(path),
                    Description = "no comment",
                    Path = path,
                    IsEnabled = isEnabled,
                };
            }
        }

        #endregion
    }
}