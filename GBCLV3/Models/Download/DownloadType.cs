using GBCLV3.Utils;
using System.ComponentModel;

namespace GBCLV3.Models.Download
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum DownloadType
    {
        [LocalizedDescription(nameof(MainJar))]
        MainJar,

        [LocalizedDescription(nameof(Libraries))]
        Libraries,

        [LocalizedDescription(nameof(Assets))]
        Assets,

        [LocalizedDescription(nameof(InstallNewVersion))]
        InstallNewVersion,

        [LocalizedDescription(nameof(InstallForge))]
        InstallForge,

        [LocalizedDescription(nameof(InstallFabric))]
        InstallFabric,
    }
}
