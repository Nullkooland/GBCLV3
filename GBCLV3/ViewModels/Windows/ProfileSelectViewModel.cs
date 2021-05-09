using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using GBCLV3.Models.Authentication;
using GBCLV3.Services;
using GBCLV3.Services.Authentication;
using GBCLV3.Services.Auxiliary;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Windows
{
    public class ProfileSelectViewModel : Screen
    {
        #region Private Fields

        private readonly AccountService _accountService;
        private readonly SkinService _skinService;
        private readonly ThemeService _themeService;

        #endregion

        #region Constructor

        [Inject]
        public ProfileSelectViewModel(
            AccountService accountService,
            SkinService skinService,
            ThemeService themeService)
        {
            _accountService = accountService;
            _skinService = skinService;
            _themeService = themeService;
        }

        public void Setup(IEnumerable<AuthUserProfile> profiles, string profileServer)
        {
            Profiles = new BindableCollection<AuthUserProfile>(profiles);
            LoadAvatars(profileServer).ConfigureAwait(false);
        }

        #endregion

        #region Bindings

        public BindableCollection<AuthUserProfile> Profiles { get; private set; }

        public AuthUserProfile SelectedProfile { get; set; }

        public bool IsLoading { get; private set; }

        public void Selected() => this.RequestClose(true);

        public void OnWindowLoaded(Window window, RoutedEventArgs _)
        {
            _themeService.SetBackgroundEffect(window);
        }

        #endregion

        #region Private Methods

        private async Task LoadAvatars(string profileServer)
        {
            IsLoading = true;

            for (int i = 0; i < Profiles.Count; i++)
            {
                Profiles[i].Base64Profile = await _accountService.GetProfileAsync(Profiles[i].Id, profileServer);
                Profiles[i].Skin = await _skinService.GetAsync(Profiles[i].Base64Profile);
            }

            Profiles.Refresh();
            IsLoading = false;
        }

        #endregion
    }
}
