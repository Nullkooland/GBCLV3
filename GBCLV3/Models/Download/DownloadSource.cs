using System.ComponentModel;
using GBCLV3.Utils.Binding;

namespace GBCLV3.Models.Download
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DownloadSource
    {
        [LocalizedDescription(nameof(Official))]
        Official,

        [LocalizedDescription(nameof(BMCLAPI))]
        BMCLAPI,

        [LocalizedDescription(nameof(MCBBS))]
        MCBBS,
    }

    public interface IDownloadUrlBase
    {
        string VersionList { get; }

        string Version { get; }

        string Library { get; }

        string Json { get; }

        string Asset { get; }

        string ForgeList { get; }

        string Forge { get; }

        string ForgeMaven { get; }

        string Fabric { get; }

        string FabricMaven { get; }

        string AuthlibInjector { get; }
    }

    public class OfficialUrlBase : IDownloadUrlBase
    {
        public string VersionList => "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public string Version => "https://launcher.mojang.com/";

        public string Library => "https://libraries.minecraft.net/";

        public string Json => "https://launchermeta.mojang.com/";

        public string Asset => "https://resources.download.minecraft.net/";

        public string ForgeList => "https://bmclapi2.bangbang93.com/forge/minecraft/";

        public string Forge => "https://files.minecraftforge.net/maven/net/minecraftforge/forge/";

        public string ForgeMaven => "https://files.minecraftforge.net/maven/";

        public string Fabric => "https://meta.fabricmc.net/v2/versions/loader/";

        public string FabricMaven => "https://maven.fabricmc.net/";

        public string AuthlibInjector => "https://authlib-injector.yushi.moe/artifact/latest.json";
    }

    public class BMCLAPIUrlBase : IDownloadUrlBase
    {
        public string VersionList => "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json";

        public string Version => "https://bmclapi2.bangbang93.com/";

        public string Library => "https://bmclapi2.bangbang93.com/libraries/";

        public string Json => "https://bmclapi2.bangbang93.com/";

        public string Maven => "https://bmclapi2.bangbang93.com/maven/";

        public string Asset => "https://bmclapi2.bangbang93.com/assets/";

        public string ForgeList => "https://bmclapi2.bangbang93.com/forge/minecraft/";

        public string Forge => "https://bmclapi2.bangbang93.com/maven/net/minecraftforge/forge/";

        public string ForgeMaven => Maven;

        public string Fabric => "http://bmclapi.bangbang93.com/fabric-meta/v2/versions/loader/";

        public string FabricMaven => Maven;

        public string AuthlibInjector => "https://bmclapi2.bangbang93.com/mirrors/authlib-injector/artifact/latest.json";
    }

    public class MCBBSUrlBase : IDownloadUrlBase
    {
        public string VersionList => "https://download.mcbbs.net/mc/game/version_manifest.json";

        public string Version => "https://download.mcbbs.net/";

        public string Library => "https://download.mcbbs.net/libraries/";

        public string Json => "https://download.mcbbs.net/";

        public string Maven => "https://download.mcbbs.net/maven/";

        public string Asset => "https://download.mcbbs.net/assets/";

        public string ForgeList => "https://download.mcbbs.net/forge/minecraft/";

        public string Forge => "https://download.mcbbs.net/maven/net/minecraftforge/forge/";

        public string ForgeMaven => Maven;

        public string Fabric => "http://bmclapi.bangbang93.com/fabric-meta/v2/versions/loader/";

        public string FabricMaven => Maven;

        public string AuthlibInjector => "https://download.mcbbs.net/mirrors/authlib-injector/artifact/latest.json";
    }

}
