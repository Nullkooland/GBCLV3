using GBCLV3.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

namespace GBCLV3.Models.Launch
{

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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum AfterLaunchBehavior
    {
        [LocalizedDescription(nameof(Exit))]
        Exit,

        [LocalizedDescription(nameof(Hide))]
        Hide,

        [LocalizedDescription(nameof(KeepVisible))]
        KeepVisible,
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

    #region Json Class

    class JArguments
    {
        // Heinous heterogeneous json
        public List<JsonElement> game { get; set; }

        // Useless for now
        // public List<JsonElement> jvm { get; set; }
    }

    #endregion
}
