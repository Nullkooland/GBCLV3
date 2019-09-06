using System;

namespace GBCLV3.Models.Installation
{
    class Forge
    {
        public string Version { get; set; }

        public int Build { get; set; }

        public DateTime ReleaseTime { get; set; }

        public string Branch { get; set; }

        public string GameVersion { get; set; }

        public bool IsAutoInstall { get; set; }

        public bool HasSuffix { get; set; }
    }
}
