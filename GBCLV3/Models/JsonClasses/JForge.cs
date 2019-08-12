using System;

namespace GBCLV3.Models.JsonClasses
{
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
}
