using System.ComponentModel;
using GBCLV3.Utils;

namespace GBCLV3.Models.Launcher
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum AfterLaunchBehavior
    {
        [LocalizedDescription("Exit")]
        Exit,

        [LocalizedDescription("Hide")]
        Hide,

        [LocalizedDescription("KeepVisible")]
        KeepVisible,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum LaunchStatus
    {
        Downloading,

        [LocalizedDescription("ProcessingDependencies")]
        ProcessingDependencies,

        [LocalizedDescription("LoggingIn")]
        LoggingIn,

        [LocalizedDescription("StartingGameProcess")]
        StartingProcess,

        [LocalizedDescription("Boohoo")]
        Failed,

        [LocalizedDescription("HappyGame")]
        Running,
    }

    class LaunchProfile
    {
        public bool IsDebugMode { get; set; }

        public string JvmArgs { get; set; }

        public uint MaxMemory { get; set; }

        public string Username { get; set; }

        public string UUID { get; set; }

        public string AccessToken { get; set; }

        public string UserType { get; set; }

        public string VersionType { get; set; }

        public bool IsFullScreen { get; set; }

        public uint WinWidth { get; set; }

        public uint WinHeight { get; set; }

        public string ServerAddress { get; set; }

        public string ExtraArgs { get; set; }
    }
}
