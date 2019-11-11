using System.Windows.Media.Imaging;
using GBCLV3.Models.Launcher;
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

        public LaunchStatus Status { get; set; }

        public string GameOutputLog { get; set; }

        public void OnAnimationCompleted()
        {
            if (this.IsActive) this.RequestClose();
        }

        #endregion
    }
}
