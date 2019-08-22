using System.Collections.Generic;

namespace GBCLV3.Models.JsonClasses
{
    /// <summary>
    /// Json instance of a dependent library for Minecraft
    /// </summary>
    internal class JLibrary
    {
        public JExtract extract { get; set; }

        public string name { get; set; }

        public string url { get; set; }

        public Dictionary<string, string> natives { get; set; }

        public List<JRule> rules { get; set; }

        public JLibraryDownload downloads { get; set; }
    }

    /// <summary>
    /// Json instance specifies ignored entries when extracting a native library
    /// </summary>
    internal class JExtract
    {
        public string[] exclude { get; set; }
    }

    /// <summary>
    /// Json instance specifies rule for a native library
    /// </summary>
    internal class JRule
    {
        public string action { get; set; }

        public JOperatingSystem os { get; set; }
    }

    /// <summary>
    /// Json instance of a signal string...so disgusting!
    /// </summary>
    internal class JOperatingSystem
    {
        public string name { get; set; }
    }
}
