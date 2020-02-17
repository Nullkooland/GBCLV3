using GBCLV3.Utils;
using System.ComponentModel;

namespace GBCLV3.Models.Installation
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum VersionInstallStatus
    {
        [LocalizedDescription("VersionListLoading")]
        ListLoading,

        [LocalizedDescription("VersionListLoaded")]
        ListLoaded,

        [LocalizedDescription("VersionListLoadFailed")]
        ListLoadFailed,

        [LocalizedDescription("VersionFetchingJson")]
        FetchingJson,

        [LocalizedDescription("VersionDownloadingDependencies")]
        DownloadingDependencies,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ForgeInstallStatus
    {
        [LocalizedDescription("ForgeListLoading")]
        ListLoading,

        [LocalizedDescription("SelectForgeVersion")]
        ListLoaded,

        [LocalizedDescription("ForgeDownloadingInstaller")]
        DownloadingInstaller,

        [LocalizedDescription("ForgeManualInstalling")]
        ManualInstalling,

        [LocalizedDescription("ForgeDownloadingLibraries")]
        DownloadingLibraries,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FabricInstallStatus
    {
        [LocalizedDescription("FabricListLoading")]
        ListLoading,

        [LocalizedDescription("SelectFabricVersion")]
        ListLoaded,

        [LocalizedDescription("FabricDownloadingLibraries")]
        DownloadingLibraries,
    }
}
