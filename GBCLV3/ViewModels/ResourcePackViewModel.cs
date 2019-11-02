using System.Threading.Tasks;
using GBCLV3.Models;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class ResourcePackViewModel : Screen
    {
        #region Private Members

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly ResourcePackService _resourcePackService;
        private readonly LanguageService _languageService;

        #endregion

        #region Constructor

        [Inject]
        public ResourcePackViewModel(
            GamePathService gamePathService,
            ResourcePackService resourcePackService,
            LanguageService languageService)
        {
            _gamePathService = gamePathService;
            _resourcePackService = resourcePackService;
            _languageService = languageService;

            EnabledPacks = new BindableCollection<ResourcePack>();
            DisabledPacks = new BindableCollection<ResourcePack>();
        }

        #endregion

        #region Bindings

        public BindableCollection<ResourcePack> EnabledPacks { get; private set; }

        public BindableCollection<ResourcePack> DisabledPacks { get; private set; }

        public void OpenDir() => SystemUtil.OpenLink(_gamePathService.ResourcePacksDir);

        public void Open(string path) => SystemUtil.OpenLink(path);

        public async void Delete(ResourcePack pack)
        {
            bool _ = (pack.IsEnabled) ? EnabledPacks.Remove(pack) : DisabledPacks.Remove(pack);
            await _resourcePackService.DeleteFromDiskAsync(pack);
        }

        public async void Reload()
        {
            await Task.Run(() =>
            {
                EnabledPacks.Clear();
                DisabledPacks.Clear();

                var (enabledPacks, disabledPacks) = _resourcePackService.GetAll();
                EnabledPacks.AddRange(enabledPacks);
                DisabledPacks.AddRange(disabledPacks);
            });
        }

        public void Enable(ResourcePack pack)
        {
            DisabledPacks.Remove(pack);
            pack.IsEnabled = true;
            EnabledPacks.Insert(0, pack);
        }

        public void Disable(ResourcePack pack)
        {
            EnabledPacks.Remove(pack);
            pack.IsEnabled = false;
            DisabledPacks.Insert(0, pack);
        }

        public void MoveUp(ResourcePack pack)
        {
            int index = EnabledPacks.IndexOf(pack);
            if (index != 0)
            {
                EnabledPacks.Remove(pack);
                EnabledPacks.Insert(index - 1, pack);
            }
        }

        public void MoveDown(ResourcePack pack)
        {
            int index = EnabledPacks.IndexOf(pack);
            if (index != EnabledPacks.Count - 1)
            {
                EnabledPacks.Remove(pack);
                EnabledPacks.Insert(index + 1, pack);
            }
        }

        public async void AddNew()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = true,
                Title = _languageService.GetEntry("SelectResourcepacks"),
                Filter = "Minecraft resourcepack | *.zip",
            };

            if (dialog.ShowDialog() ?? false)
            {
                DisabledPacks.AddRange(await _resourcePackService.MoveLoadAll(dialog.FileNames));
            }
        }

        public void SaveToOptions() => _resourcePackService.WriteToOptions(EnabledPacks);

        #endregion
    }
}
