using GBCLV3.Utils;
using System.ComponentModel;

namespace GBCLV3.Models.Download
{
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
}
