using System.Collections.Generic;

namespace GBCLV3.Models.JsonClasses
{
    /// <summary>
    /// Json instance of main jar download info
    /// </summary>
    internal class JMainJarDownload
    {
        public JFile client { get; set; }

        public JFile server { get; set; }
    }

    /// <summary>
    /// Json instance of library download info
    /// </summary>
    internal class JLibraryDownload
    {
        public JFile artifact { get; set; }

        public Dictionary<string, JFile> classifiers { get; set; }
    }

    /// <summary>
    /// Json instance of single file download info
    /// </summary>
    internal class JFile
    {
        public string id { get; set; }

        public string sha1 { get; set; }

        public int size { get; set; }

        public string path { get; set; }

        public string url { get; set; }

        public int totalSize { get; set; }
    }
}
