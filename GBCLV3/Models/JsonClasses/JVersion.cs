using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace GBCLV3.Models.JsonClasses
{
    /// <summary>
    /// Json instance of a Minecraft version from local json
    /// </summary>
    class JVersion
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

    /// <summary>
    /// Json instance of version downloads list
    /// </summary>
    class JVersionList
    {
        public JLatesetVersion latest { get; set; }

        public List<JDownloadVersion> versions { get; set; }
    }

    /// <summary>
    /// Json instance of the latest Minecraft version
    /// </summary>
    class JLatesetVersion
    {
        public string release { get; set; }

        public string snapshot { get; set; }
    }

    /// <summary>
    /// Json instance of a Minecraft version from download list
    /// </summary>
    class JDownloadVersion
    {
        public string id { get; set; }

        public string type { get; set; }

        public string url { get; set; }

        public DateTime time { get; set; }

        public DateTime releaseTime { get; set; }
    }
}
