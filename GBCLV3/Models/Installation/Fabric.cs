using System.Collections.Generic;
using GBCLV3.Models.JsonClasses;

namespace GBCLV3.Models.Installation
{
    class FabricLoader
    {
        public string Separator { get; set; }

        public int Build { get; set; }

        public string Maven { get; set; }

        public string Version { get; set; }

        public bool Stable { get; set; }
    }

    class FabricIntermediary
    {
        public string Maven { get; set; }

        public string Version { get; set; }

        public bool Stable { get; set; }
    }

    class FabricLibraries
    {
        public List<JLibrary> Common { get; set; }

        public List<JLibrary> Client { get; set; }

        public List<JLibrary> Server { get; set; }
    }

    class FabricMeta
    {
        public int Version { get; set; }

        public FabricLibraries Libraries { get; set; }
    }

    class Fabric
    {
        public FabricLoader Loader { get; set; }

        public FabricIntermediary Intermediary { get; set; }

        public FabricMeta LauncherMeta { get; set; }
    }
}
