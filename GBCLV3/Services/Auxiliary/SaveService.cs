using GBCLV3.Services.Launch;
using StyletIoC;

namespace GBCLV3.Models.Auxiliary
{
    class SaveService
    {
        #region Private Members

        // IoC
        private readonly GamePathService _gamePathService;

        #endregion

        #region Constructor

        [Inject]
        public SaveService(GamePathService gamePathService)
        {
            _gamePathService = gamePathService;
        }

        #endregion
    }
}
