using GBCLV3.Models.Download;
using System.Collections.Generic;

namespace GBCLV3.Models.Launch
{
    enum LibraryType
    {
        Minecraft,
        Native,
        ForgeMain,
        Forge,
        Fabric,
    }

    class Library
    {
        public string Name { get; set; }

        public LibraryType Type { get; set; }

        public string Path { get; set; }

        public int Size { get; set; }

        public string SHA1 { get; set; }

        public string Url { get; set; }

        public string[] Exclude { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Library;
            return other?.Name == this.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    #region Json Class

    internal class JLibrary
    {
        public JExtract extract { get; set; }

        public string name { get; set; }

        public string url { get; set; }

        public Dictionary<string, string> natives { get; set; }

        public List<JRule> rules { get; set; }

        public JLibraryDownload downloads { get; set; }
    }

    internal class JExtract
    {
        public string[] exclude { get; set; }
    }

    internal class JRule
    {
        public string action { get; set; }

        public JOperatingSystem os { get; set; }
    }

    internal class JOperatingSystem
    {
        public string name { get; set; }
    }

    #endregion
}
