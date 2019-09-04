using System.ComponentModel;
using GBCLV3.Utils;

namespace GBCLV3.Models.Installation
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum VersionInstallStatus
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
    enum ForgeInstallStatus
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
}
