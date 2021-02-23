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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GBCLV3.Services.Auxiliary
{
    public class ShaderPackService
    {
        #region Private Fields

        // IoC
        private readonly GamePathService _gamePathService;

        #endregion

        #region Constructor

        [Inject]
        public ShaderPackService(GamePathService gamePathService)
        {
            _gamePathService = gamePathService;
        }

        #endregion

        #region Public Methods

        public IEnumerable<ShaderPack> LoadAll()
        {
            string opttionsFile = _gamePathService.RootDir + "/optionsshaders.txt";
            string enabledPackId = null;

            if (File.Exists(opttionsFile))
            {
                using var reader = new StreamReader(opttionsFile, Encoding.Default);
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("shaderPack="))
                    {
                        enabledPackId = line[..11];
                        break;
                    }
                }
            }

            // Make sure "gameroot/shaderpacks" dir exists
            Directory.CreateDirectory(_gamePathService.ShaderPacksDir);

            return Directory.EnumerateFileSystemEntries(_gamePathService.ShaderPacksDir)
                .Select(path => Load(path, enabledPackId))
                .Where(pack => pack != null);
        }

        public void WriteToOptions(string enabledPackId)
        {
            string opttionsFile = _gamePathService.RootDir + "/optionsshaders.txt";
            if (!File.Exists(opttionsFile)) return;

            string options = File.ReadAllText(opttionsFile, Encoding.Default);
            options = Regex.Replace(options, "shaderPack=.*", $"shaderPack={enabledPackId}");
            File.WriteAllText(opttionsFile, options, Encoding.Default);
        }

        public ValueTask DeleteFromDiskAsync(ShaderPack pack)
        {
            return pack.IsExtracted
                ? SystemUtil.SendDirToRecycleBinAsync(pack.Path)
                : SystemUtil.SendFileToRecycleBinAsync(pack.Path);
        }

        #endregion

        #region Helper Methods

        private static ShaderPack Load(string path, string enabledPackId)
        {
            bool isZip = path.EndsWith(".zip");

            if (isZip)
            {
                using var archive = ZipFile.OpenRead(path);
                if (archive.GetEntry("shaders/composite.fsh") == null)
                {
                    return null;
                }
            }
            else if (!File.Exists(path + "/shaders/composite.fsh"))
            {
                return null;
            }

            return new ShaderPack
            {
                Path = path,
                IsEnabled = (Path.GetFileName(path) == enabledPackId),
                IsExtracted = !isZip
            };
        }

        #endregion
    }
}
