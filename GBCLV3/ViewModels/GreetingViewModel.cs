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
        #region Private Fields

        private readonly Config _config;

        #endregion

        #region Constructor

        [Inject]
        public GreetingViewModel(
            ConfigService configService
            )
        {
            _config = configService.Entries;
        }

        #endregion

        #region Bindings

        public bool IsReady { get; private set; }

        public string Username { get; private set; }

        public string Email { get; private set; }

        public BitmapSource SkinFace { get; private set; }

        public void OnAnimationCompleted()
        {
            if (this.IsActive) this.RequestClose();
        }

        #endregion
    }
}
