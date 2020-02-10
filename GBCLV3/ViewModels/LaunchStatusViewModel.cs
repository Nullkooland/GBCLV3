using GBCLV3.Models.Launch;
using Stylet;

namespace GBCLV3.ViewModels
{
    class LaunchStatusViewModel : Screen
    {
        #region Bindings

        public bool IsInLaunchProcess =>
            Status == LaunchStatus.LoggingIn ||
            Status == LaunchStatus.ProcessingDependencies ||
            Status == LaunchStatus.StartingProcess;

        public bool IsSucceeded =>
            Status == LaunchStatus.Running;

        public bool IsFailed =>
            Status == LaunchStatus.Failed;

        public LaunchStatus Status { get; set; }

        public string GameOutputLog { get; set; }

        public void OnAnimationCompleted()
        {
            if (this.IsActive) this.RequestClose();
        }

        #endregion
    }
}
