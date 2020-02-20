using GBCLV3.Services.Launch;
using StyletIoC;

namespace GBCLV3.Models.Auxiliary
{
    public class SaveService
    {
        #region Private Fields

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
