using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
            if (!Directory.Exists(_gamePathService.ModDir))
            {
                Directory.CreateDirectory(_gamePathService.ModDir);
                return null;
            }

            return Directory.EnumerateFiles(_gamePathService.ModDir)
                            .Where(file => file.EndsWith(".jar") || file.EndsWith(".jar.disabled"))
                            .Select(path => Load(path))
                            .OrderBy(mod => mod.IsEnabled);
        }

        public void RewriteExtension(Mod mod)
        {
            string newPath = Path.ChangeExtension(mod.Path, mod.IsEnabled ? ".jar.disabled" : ".jar");
            FileSystem.RenameFile(mod.Path, newPath);

            mod.Path = newPath;
            mod.IsEnabled = !mod.IsEnabled;
        }

        public async Task DeleteFromDiskAsync(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
            {
                await SystemUtil.SendFileToRecycleBin(mod.Path);
            }
        }

        #endregion

        #region Private Methods

        private static Mod Load(string path)
        {
            using (var archive = ZipFile.OpenRead(path))
            {
                var info = archive.GetEntry("mcmod.info");

                if (info != null)
                {
                    // Remove the top-level braces pair for a cleaner json deserialization
                    // This is utterly ugly...thanks to the capriciousness of modders
                    var match = Regex.Match(new StreamReader(info.Open(), Encoding.UTF8).ReadToEnd(), "\\{[\\s\\S]*\\}");

                    if (match.Success)
                    {
                        JMod jmod = null;

                        try
                        {
                            JsonSerializer.Deserialize<JMod>(match.Value);
                        }
                        catch (JsonException ex)
                        {
                            // Well, nothing I can do.
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
                            Description = jmod?.description,
                            Version = jmod?.version,
                            GameVersion = jmod?.mcversion,
                            Url = jmod?.url,
                            Authors = auhtors,
                            Path = path,
                            IsEnabled = path.EndsWith(".jar"),
                        };
                    }
                }

                return new Mod
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    IsEnabled = path.EndsWith(".jar"),
                };
            }
        }

        #endregion
    }
}