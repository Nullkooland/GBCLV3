using System;
using System.Collections.Generic;

namespace GBCLV3.Models.Launcher
{
    /// <summary>
    /// Minecraft version instance used in launcher
    /// </summary>
    class Version
    {
        public string ID { get; set; }

        public string JarID { get; set; }

        public int Size { get; set; }

        public string SHA1 { get; set; }

        public string Url { get; set; }

        public string InheritsFrom { get; set; }

        public string MinecarftArguments { get; set; }

        public string MainClass { get; set; }

        public List<Library> Libraries { get; set; }

        public AssetsInfo AssetsInfo { get; set; }
    }

    /// <summary>
    /// Json instance of a Minecraft version from download list
    /// </summary>
    class VersionDownload
    {
        public string ID { get; set; }

        public string Type { get; set; }

        public string Url { get; set; }

        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Json instance of the latest Minecraft version
    /// </summary>
    class LatestVersion
    {
        public string Release { get; set; }

        public string Snapshot { get; set; }
    }

    /// <summary>
    /// Json instance of version download list
    /// </summary>
    class VersionDownloadList
    {
        public LatestVersion Latest { get; set; }

        public List<VersionDownload> Downloads { get; set; }

    }
}
