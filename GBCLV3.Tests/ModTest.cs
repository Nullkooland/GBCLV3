using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GBCLV3.Tests
{
    [TestClass]
    public class ModTest
    {
        private const string GAME_ROOT_DIR = "G:/Minecraft/1.12.2/.minecraft";

        private readonly ModService _modService;

        public ModTest()
        {
            var configService = new ConfigService();
            configService.Load();
            configService.Entries.GameDir = GAME_ROOT_DIR;

            var gamePathService = new GamePathService(configService);

            _modService = new ModService(gamePathService);
        }

        [TestMethod]
        public void LoadModsTest()
        {
            Debug.WriteLine("[All Mods]");

            foreach (var mod in _modService.GetAll())
            {
                Debug.WriteLine("---------------------------------------------------------------");
                Debug.WriteLine($"Name:         {mod.Name}");
                Debug.WriteLine($"Description:  {mod.Description}");
                Debug.WriteLine($"Version:      {mod.Version}");
                Debug.WriteLine($"Game Version: {mod.GameVersion}");
                Debug.WriteLine($"Url:          {mod.Url}");
                Debug.WriteLine($"Authors:      {mod.Authors}");
                Debug.WriteLine($"Enabled:      {mod.IsEnabled}");
            }
        }
    }
}
