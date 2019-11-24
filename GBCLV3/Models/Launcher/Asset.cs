using System.Collections.Generic;

namespace GBCLV3.Models.Launcher
{
    class AssetsInfo
    {
        public string ID { get; set; }

        public int IndexSize { get; set; }

        public string IndexUrl { get; set; }

        public string IndexSHA1 { get; set; }

        public int TotalSize { get; set; }

        public bool IsLegacy => ID == "legacy";

        public Dictionary<string, AssetObject> Objects { get; set; }
    }

    /// <summary>
    /// Asset file for Minecraft
    /// </summary>
    class AssetObject
    {
        /// <summary>
        /// Hash value of asset file
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Relative path to ./assets/objects directory
        /// </summary>
        public string Path => $"{Hash[..2]}/{Hash}";

        /// <summary>
        ///  Size of asset file
        /// </summary>
        public int Size { get; set; }
    }
}
