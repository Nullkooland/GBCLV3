using GBCLV3.Services.Launcher;
using StyletIoC;

namespace GBCLV3.Services
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
