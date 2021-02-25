using GBCLV3.Models.Download;
using GBCLV3.Utils.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GBCLV3.Models.Launch
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum VersionType
    {
        [Description("Release")]
        Release,

        [Description("Snapshot")]
        Snapshot,

        [Description("Forge")]
        Forge,

        [Description("Forge")]
        NewForge,

        [Description("OptiFine")]
        OptiFine,

        [Description("Fabric")]
        Fabric,
    }

    public class Version
    {
        public string ID { get; set; }

        public string JarID { get; set; }

        public int Size { get; set; }

        public string SHA1 { get; set; }

        public string Url { get; set; }

        public string InheritsFrom { get; set; }

        public Dictionary<string, string> MinecraftArgsDict { get; set; }

        public string MainClass { get; set; }

        public VersionType Type { get; set; }

        public List<Library> Libraries { get; set; }

        public AssetsInfo AssetsInfo { get; set; }

        public int CompatibilityVersion { get; set; }
    }

    public class VersionDownload
    {
        public string ID { get; set; }

        public string Url { get; set; }

        public DateTime ReleaseTime { get; set; }

        public VersionType Type { get; set; }
    }

    public class LatestVersion
    {
        public string Release { get; set; }

        public string Snapshot { get; set; }
    }

    #region Json Class

    /// <summary>
    /// Json instance of a Minecraft version from local json
    /// </summary>
    public class JVersion
    {
        public JFile assetIndex { get; set; }

        public string assets { get; set; }

        public JMainJarDownload downloads { get; set; }

        public string id { get; set; }

        public List<JLibrary> libraries { get; set; }

        public string mainClass { get; set; }

        public JArguments arguments { get; set; }

        public string minecraftArguments { get; set; }

        public string type { get; set; }

        public string inheritsFrom { get; set; }

        public string jar { get; set; }

        // forge json 里面的 minimumLauncherVersion 字段你格老子用浮点数？？？ 这些瓜批脑壳是不是有包？？？
        public double minimumLauncherVersion { get; set; }
    }

    /// <summary>
    /// Json instance of version downloads list
    /// </summary>
    public class JVersionList
    {
        public JLatesetVersion latest { get; set; }

        public List<JDownloadVersion> versions { get; set; }
    }

    /// <summary>
    /// Json instance of the latest Minecraft version
    /// </summary>
    public class JLatesetVersion
    {
        public string release { get; set; }

        public string snapshot { get; set; }
    }

    /// <summary>
    /// Json instance of a Minecraft version from download list
    /// </summary>
    public class JDownloadVersion
    {
        public string id { get; set; }

        public string type { get; set; }

        public string url { get; set; }

        public DateTime time { get; set; }

        public DateTime releaseTime { get; set; }
    }

    #endregion
}
