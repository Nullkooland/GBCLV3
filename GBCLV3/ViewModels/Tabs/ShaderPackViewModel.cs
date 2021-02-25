using GBCLV3.Models;
using GBCLV3.Models.Auxiliary;
using GBCLV3.Services;
using GBCLV3.Services.Auxiliary;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GBCLV3.ViewModels.Tabs
{
    public class ShaderPackViewModel : Screen
    {

        #region Private Fields

        private readonly GamePathService _gamePathService;
        private readonly ShaderPackService _shaderPackService;
        private readonly LanguageService _languageService;

        private ShaderPack _enabledPack;

        #endregion

        #region Constructor

        [Inject]
        public ShaderPackViewModel(
            GamePathService gamePathService,
            ShaderPackService shaderPackService,
            LanguageService languageService)
        {
            _gamePathService = gamePathService;
            _shaderPackService = shaderPackService;
            _languageService = languageService;

            Packs = new BindableCollection<ShaderPack>();
            _enabledPack = null;
        }

        #endregion

        #region Bindings

        public BindableCollection<ShaderPack> Packs { get; }

        public bool IsCopy { get; set; }

        public async void AddNew()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = true,
                Title = _languageService.GetEntry("SelectShaderPacks"),
                Filter = "Minecraft shaderpacks | *.zip;",
            };

            if (dialog.ShowDialog() ?? false)
            {
                Packs.AddRange(await _shaderPackService.MoveLoadAllAsync(dialog.FileNames, IsCopy));
            }
        }

        public async void OnDrop(object _, DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            Packs.AddRange(await _shaderPackService.MoveLoadAllAsync(paths, IsCopy));
        }

        public async void Reload()
        {
            Packs.Clear();

            foreach (var pack in await _shaderPackService.LoadAllAsync())
            {
                if (pack.IsEnabled)
                {
                    _enabledPack = pack;
                }

                Packs.Add(pack);
            }
        }

        public void OpenDir() => SystemUtil.OpenLink(_gamePathService.ShaderPacksDir);

        public void Open(string path) => SystemUtil.OpenLink(path);

        public void Enable(ShaderPack pack)
        {
            pack.IsEnabled = true;
            OnEnableStatusChanged(pack);
        }

        public void OnEnableStatusChanged(ShaderPack pack)
        {
            if (pack.IsEnabled)
            {
                if (_enabledPack != null)
                {
                    _enabledPack.IsEnabled = false;
                }

                _enabledPack = pack;
            }
            else if (pack == _enabledPack)
            {
                _enabledPack = null;
            }

            NotifyOfPropertyChange(nameof(Packs));
        }

        public async void Delete(ShaderPack pack)
        {
            if (_enabledPack == pack)
            {
                _enabledPack = null;
            }

            Packs.Remove(pack);
            await _shaderPackService.DeleteFromDiskAsync(pack);
        }

        public void SaveToOptions() => _shaderPackService.WriteToOptions(_enabledPack);

        #endregion
    }
}
