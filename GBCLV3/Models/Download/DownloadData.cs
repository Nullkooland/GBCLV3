using System.Collections.Generic;

namespace GBCLV3.Models.Download
{
    public class DownloadItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string Url { get; set; }

        public int Size { get; set; }

        public int DownloadedBytes { get; set; }

        public bool IsPartialContentSupported { get; set; }

        public bool IsCompleted { get; set; }
    }

    #region Json Class

    public class JMainJarDownload
    {
        public JFile client { get; set; }

        public JFile server { get; set; }
    }

    public class JLibraryDownload
    {
        public JFile artifact { get; set; }

        public Dictionary<string, JFile> classifiers { get; set; }
    }

    public class JFile
    {
        public string id { get; set; }

        public string sha1 { get; set; }

        public int size { get; set; }

        public string path { get; set; }

        public string url { get; set; }

        public int totalSize { get; set; }
    }

    #endregion
}
