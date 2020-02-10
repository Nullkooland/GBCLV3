using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBCLV3.Models.Authentication;
using GBCLV3.Services.Authentication;
using GBCLV3.Services.Auxiliary;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Windows
{
    class ProfileSelectViewModel : Screen
    {
        #region Private Fields

        private readonly SkinService _skinService;

        #endregion

        #region Constructor

        [Inject]
        public ProfileSelectViewModel(SkinService skinService)
        {
            _skinService = skinService;
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

        #endregion

        #region Private Methods

        private async Task LoadAvatars(string profileServer)
        {
            IsLoading = true;

            for (int i = 0; i < Profiles.Count; i++)
            {
                Profiles[i].Base64Profile = await _skinService.GetProfileAsync(Profiles[i].Id, profileServer);
                Profiles[i].Skin = await _skinService.GetSkinAsync(Profiles[i].Base64Profile);
            }

            Profiles.Refresh();
            IsLoading = false;
        }

        #endregion
    }
}
