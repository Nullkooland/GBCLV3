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

        #endregion

        #region Constructor

        [Inject]
        public ModViewModel(
            GamePathService gamePathService,
            ModService modService)
        {
            _gamePathService = gamePathService;
            _modService = modService;

            Mods = new BindableCollection<Mod>();
            _selectedMods = new List<Mod>(32);
        }

        #endregion

        #region Bindings

        public BindableCollection<Mod> Mods { get; private set; }

        public void ChangeExtension(Mod mod) => _modService.ChangeExtension(mod);

        public void DropFiles(ListBox _, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
            var modFiles = dropFiles.Where(file => file.EndsWith(".jar"));

            foreach (var path in modFiles)
            {
                File.Copy(path, $"{_gamePathService.ModsDir}/{Path.GetFileName(path)}");
            }

            Reload();
        }

        public void AddMods()
        {

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

        public void OpenModsDir()
        {
            Directory.CreateDirectory(_gamePathService.ModsDir);
            Process.Start(_gamePathService.ModsDir);
        }

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

        protected override void OnViewLoaded() => Reload();

        #endregion
    }
}
