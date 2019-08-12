using System.Collections.Generic;
using GBCLV3.Models.Launcher;

namespace GBCLV3.Models.JsonClasses
{
    /// <summary>
    /// Json instance of assets
    /// </summary>
    class JAsset
    {
        public Dictionary<string, AssetObject> objects { get; set; }
    }
}
