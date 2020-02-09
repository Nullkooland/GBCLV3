using GBCLV3.Utils;
using System.ComponentModel;

namespace GBCLV3.Models.Download
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum DownloadSource
    {
        [LocalizedDescription("Official")]
        Official,

        [LocalizedDescription("BMCLAPI")]
        BMCLAPI,
    }

    public interface IDownloadUrlBase
    {
        string VersionList { get; }

        string Version { get; }

        string Library { get; }

        string Json { get; }

        string Asset { get; }

        string Forge { get; }

        string ForgeMaven { get; }

        string Fabric { get; }

        string FabricMaven { get; }
    }

    class OfficialUrlBase : IDownloadUrlBase
    {
        public string VersionList => "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public string Version => "https://launcher.mojang.com/";

        public string Library => "https://libraries.minecraft.net/";

        public string Json => "https://launchermeta.mojang.com/";

        public string Asset => "https://resources.download.minecraft.net/";

        public string Forge => "https://files.minecraftforge.net/maven/net/minecraftforge/forge/";

        public string ForgeMaven => "https://files.minecraftforge.net/maven/";

        public string Fabric => "https://meta.fabricmc.net/v2/versions/loader/";

        public string FabricMaven => "https://maven.fabricmc.net/";
    }

    class BMCLAPIUrlBase : IDownloadUrlBase
    {
        public string VersionList => "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json";

        public string Version => "https://bmclapi2.bangbang93.com/";

        public string Library => "https://bmclapi2.bangbang93.com/libraries/";

        public string Json => "https://bmclapi2.bangbang93.com/";

        public string Maven => "https://bmclapi2.bangbang93.com/maven/";

        public string Asset => "https://bmclapi2.bangbang93.com/assets/";

        public string Forge => "https://bmclapi2.bangbang93.com/maven/net/minecraftforge/forge/";

        public string ForgeMaven => Maven;

        public string Fabric => "http://bmclapi.bangbang93.com/fabric-meta/v2/versions/loader/";

        public string FabricMaven => Maven;
    }

}
