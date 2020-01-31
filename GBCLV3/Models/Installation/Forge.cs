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

    #region Json Class

    class JForgeFile
    {
        public string category { get; set; }

        public string format { get; set; }

        public string hash { get; set; }
    }

    class JForgeVersion
    {
        public int build { get; set; }

        public DateTime modified { get; set; }

        public string branch { get; set; }

        public string mcversion { get; set; }

        public string version { get; set; }

        public JForgeFile[] files { get; set; }
    }

    #endregion
}
