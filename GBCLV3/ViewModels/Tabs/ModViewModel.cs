using GBCLV3.Models.Auxiliary;
using GBCLV3.Services;
using GBCLV3.Services.Auxiliary;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GBCLV3.Models;

namespace GBCLV3.ViewModels.Tabs
{
    public class ModViewModel : Screen
    {
        #region Private Fields

        private readonly List<Mod> _selectedMods;

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly ModService _modService;
        private readonly LanguageService _languageService;
        private readonly Config _config;

        #endregion

        #region Constructor

        [Inject]
        public ModViewModel(
            GamePathService gamePathService,
            ModService modService,
            LanguageService languageService,
            ConfigService configService)
        {
            _gamePathService = gamePathService;
            _modService = modService;
            _languageService = languageService;
            _config = configService.Entries;

            Mods = new BindableCollection<Mod>();
            _selectedMods = new List<Mod>(32);
        }

        #endregion

        #region Bindings

        public BindableCollection<Mod> Mods { get; }

        public bool IsCopy { get; set; }

        public void ChangeExtension(Mod mod) => _modService.ChangeExtension(mod);

        public async void OnDrop(ListBox _, DragEventArgs e)
        {
            var modFiles = (e.Data.GetData(DataFormats.FileDrop) as string[])
                .Where(file => file.EndsWith(".jar") || file.EndsWith(".jar.disabled"));

            Mods.AddRange(await _modService.MoveLoadAllAsync(modFiles, IsCopy));
        }

        public async void AddNew()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = true,
                Title = _languageService.GetEntry("SelectMods"),
                Filter = "Minecraft mod | *.jar; *.jar.disabled;",
            };

            if (dialog.ShowDialog() ?? false)
            {
                Mods.AddRange(await _modService.MoveLoadAllAsync(dialog.FileNames, IsCopy));
            }
        }

        public async void Reload()
        {
            Mods.Clear();
            Mods.AddRange(await _modService.LoadAllAsync());
        }

        public void OpenDir() => SystemUtil.OpenLink(_gamePathService.ModsDir);

        public void OpenLink(string url) => SystemUtil.OpenLink(url);

        public void SelectionChanged(ListBox _, SelectionChangedEventArgs e)
        {
            foreach (object item in e.AddedItems) _selectedMods.Add(item as Mod);
            foreach (object item in e.RemovedItems) _selectedMods.Remove(item as Mod);
        }

        public void Enable()
        {
            var modsToEnable = _selectedMods.Where(mod => !mod.IsEnabled).ToArray();
            Mods.RemoveRange(modsToEnable);

            foreach (var mod in modsToEnable)
            {
                mod.IsEnabled = true;
                _modService.ChangeExtension(mod);
                Mods.Insert(0, mod);
            }
        }

        public void Disable()
        {
            var modsToDisable = _selectedMods.Where(mod => mod.IsEnabled).ToArray();
            Mods.RemoveRange(modsToDisable);

            foreach (var mod in modsToDisable)
            {
                mod.IsEnabled = false;
                _modService.ChangeExtension(mod);
                Mods.Add(mod);
            }
        }

        public void Delete()
        {
            _modService.DeleteFromDiskAsync(_selectedMods);
            Mods.RemoveRange(_selectedMods);
        }

        #endregion
    }
}