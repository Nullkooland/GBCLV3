using System.Collections.Generic;

namespace GBCLV3.Models.Launch
{
    public class AssetsInfo
    {
        public string ID { get; set; }

        public int IndexSize { get; set; }

        public string IndexUrl { get; set; }

        public string IndexSHA1 { get; set; }

        public int TotalSize { get; set; }

        public bool IsLegacy { get; set; }

        public Dictionary<string, AssetObject> Objects { get; set; }
    }

    public class AssetObject
    {
        public string Hash { get; set; }

        public string Path => $"{Hash[..2]}/{Hash}";

        public int Size { get; set; }
    }

    public class JAsset
    {
        public Dictionary<string, AssetObject> objects { get; set; }
    }
}
