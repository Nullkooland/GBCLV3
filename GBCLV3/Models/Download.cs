using System.ComponentModel;
using GBCLV3.Utils;

namespace GBCLV3.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum DownloadSource
    {
        [LocalizedDescription("Official")]
        Official,

        [LocalizedDescription("BMCLAPI")]
        BMCLAPI,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum DownloadType
    {
        [LocalizedDescription("MainJar")]
        MainJar,

        [LocalizedDescription("Libraries")]
        Libraries,

        [LocalizedDescription("Assets")]
        Assets,

        [LocalizedDescription("InstallNewVersion")]
        InstallNewVersion,

        [LocalizedDescription("InstallForge")]
        InstallForge,

        [LocalizedDescription("InstallFabric")]
        InstallFabric,
    }

    enum DownloadResult
    {
        Incomplete,
        Succeeded,
        Canceled,
    }

    class DownloadProgressEventArgs
    {
        public int TotalCount { get; set; }

        public int CompletedCount { get; set; }

        public int FailedCount { get; set; }

        public int TotalBytes { get; set; }

        public int DownloadedBytes { get; set; }

        public double Speed { get; set; }
    }

    class DownloadItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string Url { get; set; }

        public int Size { get; set; }

        public int DownloadedBytes { get; set; }

        public bool IsCompleted { get; set; }
    }
}
