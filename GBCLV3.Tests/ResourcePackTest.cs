using System.Diagnostics;
using System.Linq;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GBCLV3.Tests
{
    [TestClass]
    public class ResourcePackTest
    {
        private const string GAME_ROOT_DIR = "G:/Minecraft/1.11.2/.minecraft";

        private readonly ResourcePackService _resourcePackService;

        public ResourcePackTest()
        {
            var configService = new ConfigService();
            configService.Load();
            configService.Entries.GameDir = GAME_ROOT_DIR;

            var gamePathService = new GamePathService(configService);
            _resourcePackService = new ResourcePackService(gamePathService);
        }

        [TestMethod]
        public void LoadTest()
        {
            var (enabledPacks, disabledPacks) = _resourcePackService.GetAll();
            Debug.WriteLine("[All Resource Packs]");

            foreach (var pack in enabledPacks.Concat(disabledPacks))
            {
                Debug.WriteLine("---------------------------------------------------------------");
                Debug.WriteLine($"Name:         {pack.Name}");
                Debug.WriteLine($"Description:  {pack.Description}");
                Debug.WriteLine($"Format:       {pack.Format}");
                Debug.WriteLine($"Image Width:  {pack.Image.PixelWidth}");
                Debug.WriteLine($"Image Height: {pack.Image.PixelHeight}");
                Debug.WriteLine($"Extracted:    {pack.IsExtracted}");
                Debug.WriteLine($"Enabled:      {pack.IsEnabled}");
            }
        }

        [TestMethod]
        public void WriteOptionsTest()
        {
            var (enabledPacks, disabledPacks) = _resourcePackService.GetAll();
            //packs.ForEach(pack => pack.IsEnabled = true);

            _resourcePackService.WriteToOptions(enabledPacks.Concat(disabledPacks));
        }
    }
}
