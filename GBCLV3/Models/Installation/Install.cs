using System.ComponentModel;
using GBCLV3.Utils;

namespace GBCLV3.Models.Installation
{
    enum InstallType
    {
        Version,
        Forge,
        Fabric,
        OptiFine,
    }

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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum FabricInstallStatus
    {
        [LocalizedDescription("FabricListLoading")]
        ListLoading,

        [LocalizedDescription("SelectFabricVersion")]
        ListLoaded,

        [LocalizedDescription("FabricDownloadingLibraries")]
        DownloadingLibraries,
    }
}
