using System.Linq;
using GBCLV3.Models.Auxiliary;
using GBCLV3.Services;
using GBCLV3.Services.Auxiliary;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GBCLV3.ViewModels.Tabs
{
    public class ResourcePackViewModel : Screen
    {
        #region Private Fields

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

        public BindableCollection<ResourcePack> EnabledPacks { get; }

        public BindableCollection<ResourcePack> DisabledPacks { get; }

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

                var (enabledPacks, disabledPacks) = _resourcePackService.LoadAll();
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

        public async void OnDropDisabledPacks(ListBox _, DragEventArgs e)
        {
            var paths = (e.Data.GetData(DataFormats.FileDrop) as string[])
                .Where(path => path.EndsWith(".zip"));

            DisabledPacks.AddRange(await _resourcePackService.MoveLoadAllAsync(paths, false));
        }

        public async void OnDropEnabledPacks(ListBox _, DragEventArgs e)
        {
            var paths = (e.Data.GetData(DataFormats.FileDrop) as string[])
                .Where(path => path.EndsWith(".zip"));

            EnabledPacks.AddRange(await _resourcePackService.MoveLoadAllAsync(paths, true));
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
                DisabledPacks.AddRange(await _resourcePackService.MoveLoadAllAsync(dialog.FileNames, false));
            }
        }

        public void SaveToOptions() => _resourcePackService.WriteToOptions(EnabledPacks);

        #endregion
    }
}
