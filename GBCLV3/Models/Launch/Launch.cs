using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using GBCLV3.Models.Authentication;
using GBCLV3.Utils.Binding;

namespace GBCLV3.Models.Launch
{

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum LaunchStatus
    {
        Downloading,

        [LocalizedDescription("LoggingIn")]
        LoggingIn,

        [LocalizedDescription("ProcessingDependencies")]
        ProcessingDependencies,

        [LocalizedDescription("StartingGameProcess")]
        StartingProcess,

        [LocalizedDescription("Boohoo")]
        Failed,

        [LocalizedDescription("HappyGame")]
        Running,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AfterLaunchBehavior
    {
        [LocalizedDescription(nameof(Exit))]
        Exit,

        [LocalizedDescription(nameof(Hide))]
        Hide,

        [LocalizedDescription(nameof(KeepVisible))]
        KeepVisible,
    }

    public class LaunchProfile
    {
        public bool IsDebugMode { get; set; }

        public string JvmArgs { get; set; }

        public uint MaxMemory { get; set; }

        public Account Account { get; set; }

        public string VersionType { get; set; }

        public bool IsFullScreen { get; set; }

        public uint WinWidth { get; set; }

        public uint WinHeight { get; set; }

        public string ServerAddress { get; set; }

        public string ExtraArgs { get; set; }
    }

    #region Json Class

    public class JArguments
    {
        // Heinous heterogeneous json
        public List<JsonElement> game { get; set; }

        // Useless for now
        // public List<JsonElement> jvm { get; set; }
    }

    #endregion
}
