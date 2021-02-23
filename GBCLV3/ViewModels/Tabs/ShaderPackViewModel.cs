using GBCLV3.Models.Auxiliary;
using GBCLV3.Services.Auxiliary;
using GBCLV3.Services.Launch;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBCLV3.ViewModels.Tabs
{
    public class ShaderPackViewModel : Screen
    {

        #region Private Fields

        private readonly GamePathService _gamePathService;
        private readonly ShaderPackService _shaderPackService;

        #endregion

        #region Constructor

        [Inject]
        public ShaderPackViewModel(GamePathService gamePathService, ShaderPackService shaderPackService)
        {
            _gamePathService = gamePathService;
            _shaderPackService = shaderPackService;
        }

        #endregion

        #region Bindings

        public BindableCollection<ShaderPack> ShaderPacks { get; }




        #endregion
    }
}
