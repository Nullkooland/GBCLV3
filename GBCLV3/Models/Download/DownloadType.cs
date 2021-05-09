using System.ComponentModel;
using GBCLV3.Utils.Binding;

namespace GBCLV3.Models.Download
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DownloadType
    {
        [LocalizedDescription(nameof(AuthlibInjector))]
        AuthlibInjector,

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
