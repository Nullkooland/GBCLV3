using GBCLV3.Services.Launcher;

namespace GBCLV3.Services
{
    class SaveService
    {
        #region Private Members

        // IoC
        private readonly GamePathService _gamePathService;

        #endregion

        #region Constructor

        public SaveService(GamePathService gamePathService)
        {
            _gamePathService = gamePathService;
        }

        #endregion
    }
}
