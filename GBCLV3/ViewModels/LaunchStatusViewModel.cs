using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using GBCLV3.Models.Launcher;
using GBCLV3.Views;
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
