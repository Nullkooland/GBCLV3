using System;
using System.Collections.Generic;
using GBCLV3.Models.Launch;

namespace GBCLV3.Models.Installation
{
    public class Forge
    {
        public int Build { get; set; }

        public string Version { get; set; }

        public string ID { get; set; }

        public string FullName { get; set; }

        public DateTime ReleaseTime { get; set; }
    }

    #region Json Class

    public class JForgeInstallProfile
    {
        public List<JLibrary> libraries { get; set; }
    }

    public class JForgeFile
    {
        public string category { get; set; }

        public string format { get; set; }

        public string hash { get; set; }
    }

    public class JForgeVersion
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