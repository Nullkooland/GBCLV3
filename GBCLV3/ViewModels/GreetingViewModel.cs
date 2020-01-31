using GBCLV3.Models;
using GBCLV3.Models.Auxiliary;
using GBCLV3.Services;
using GBCLV3.Services.Auxiliary;
using Stylet;
using StyletIoC;
using System;
using System.Windows.Media.Imaging;

namespace GBCLV3.ViewModels
{
    class GreetingViewModel : Screen
    {
        #region Private Members

        private readonly Config _config;
        private readonly SkinService _skinService;

        #endregion

        #region Constructor

        [Inject]
        public GreetingViewModel(
            ConfigService configService,
            SkinService skinService
            )
        {
            _config = configService.Entries;
            _skinService = skinService;
        }

        #endregion

        #region Bindings

        public bool IsReady { get; private set; }

        public string Username => _config.Username;

        public string Email { get; private set; }

        public BitmapSource SkinFace { get; private set; }

        public async void OnLoaded()
        {
            if (IsReady) return;

            Skin skin = null;
            if (!_config.OfflineMode)
            {
                skin = await _skinService.GetSkinAsync(_config.UUID);
                Email = _config.Email;
            }
            else
            {
                Email = "offline";
            }

            if (skin != null)
            {
                SkinFace = _skinService.GetFace(skin.Body);
            }
            else
            {
                SkinFace = new BitmapImage(new Uri("/Resources/Images/enderman.png", UriKind.Relative));
            }

            IsReady = true;
        }

        public void OnAnimationCompleted()
        {
            if (this.IsActive) this.RequestClose();
        }

        #endregion
    }
}
