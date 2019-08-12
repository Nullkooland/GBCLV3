using System;

namespace GBCLV3.Models
{
    class ForgeDownload
    {
        public int Build { get; set; }

        public DateTime Date { get; set; }

        public string Branch { get; set; }

        public string GameVersion { get; set; }

        public string Version { get; set; }
    }
}
