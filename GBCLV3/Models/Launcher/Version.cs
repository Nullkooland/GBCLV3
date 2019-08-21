using System;
using System.Collections.Generic;
using System.ComponentModel;
using GBCLV3.Utils;

namespace GBCLV3.Models.Launcher
{

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum VersionType
    {
        [Description("Release")]
        Release,

        [Description("Snapshot")]
        Snapshot,

        [Description("Forge")]
        Forge,

        [Description("Forge")]
        NewForge,
    }

    class Version
    {
        public string ID { get; set; }

        public string JarID { get; set; }

        public int Size { get; set; }

        public string SHA1 { get; set; }

        public string Url { get; set; }

        public string InheritsFrom { get; set; }

        public Dictionary<string, string> MinecarftArgsDict { get; set; }

        public string MainClass { get; set; }

        public VersionType Type { get; set; }

        public List<Library> Libraries { get; set; }

        public AssetsInfo AssetsInfo { get; set; }
    }

    class VersionDownload
    {
        public string ID { get; set; }

        public string Url { get; set; }

        public DateTime ReleaseTime { get; set; }

        public VersionType Type { get; set; }
    }

    class LatestVersion
    {
        public string Release { get; set; }

        public string Snapshot { get; set; }
    }
}
