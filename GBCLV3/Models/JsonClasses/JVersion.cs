using System;
using System.Collections.Generic;

namespace GBCLV3.Models.JsonClasses
{
    /// <summary>
    /// Json instance of a Minecraft version from local json
    /// </summary>
    internal class JVersion
    {
        public JFile assetIndex { get; set; }

        public string assets { get; set; }

        public JMainJarDownload downloads { get; set; }

        public string id { get; set; }

        public List<JLibrary> libraries { get; set; }

        public string mainClass { get; set; }

        public JArguments arguments { get; set; }

        public string minecraftArguments { get; set; }

        public DateTime time { get; set; }

        public string type { get; set; }

        public string inheritsFrom { get; set; }

        public string jar { get; set; }
    }
}
