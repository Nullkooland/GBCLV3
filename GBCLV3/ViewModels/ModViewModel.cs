using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using GBCLV3.Models;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class ModViewModel : Screen
    {
        #region Private Members

        private readonly List<Mod> _selectedMods;

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly ModService _modService;
        private readonly LanguageService _languageService;

        #endregion

        #region Constructor

        [Inject]
        public ModViewModel(
            GamePathService gamePathService,
            ModService modService,
            LanguageService languageService)
        {
            _gamePathService = gamePathService;
            _modService = modService;
            _languageService = languageService;

            Mods = new BindableCollection<Mod>();
            _selectedMods = new List<Mod>(32);
        }

        #endregion

        #region Bindings

        public BindableCollection<Mod> Mods { get; private set; }

        public void ChangeExtension(Mod mod) => _modService.ChangeExtension(mod);

        public void DropFiles(ListBox _, DragEventArgs e) 
            => CopyMods(e.Data.GetData(DataFormats.FileDrop) as string[]);

        public void AddNew()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = _languageService.GetEntry("SelectMods"),
                Filter = "Minecraft mod | *.jar",
            };

            if (dialog.ShowDialog() ?? false)
            {
                CopyMods(dialog.FileNames);
            }
        }

        public async void Reload()
        {
            Mods.Clear();
            var availableMods = await _modService.GetAll();

            if (availableMods != null)
            {
                Mods.AddRange(await _modService.GetAll());
            }
        }

        public void OpenDir()
        {
            Directory.CreateDirectory(_gamePathService.ModsDir);
            Process.Start(_gamePathService.ModsDir);
        }

        public void OpenLink(string url) => Process.Start(url);

        public void SelectionChanged(ListBox _, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems) _selectedMods.Add(item as Mod);
            foreach (var item in e.RemovedItems) _selectedMods.Remove(item as Mod);
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

        public async void Delete()
        {
            await _modService.DeleteFromDiskAsync(_selectedMods);
            Mods.RemoveRange(_selectedMods);
        }

        #endregion

        #region Private Methods

        private void CopyMods(string[] srcPaths)
        {
            var modFiles = srcPaths.Where(path => path.EndsWith(".jar"))
                                   .Where(path => _modService.IsValid(path));

            foreach (var path in modFiles)
            {
                File.Move(path, $"{_gamePathService.ModsDir}/{Path.GetFileName(path)}");
            }

            Reload();
        }

        protected override void OnViewLoaded() => Reload();

        #endregion
    }
}
