using System.Windows.Media.Imaging;

namespace GBCLV3.Models.Auxiliary
{
    class ResourcePack
    {
        public string Name => System.IO.Path.GetFileName(Path);

        public int Format { get; set; }

        public string Description { get; set; }

        public BitmapImage Image { get; set; }

        public string Path { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsExtracted { get; set; }
    }

    #region Json Class

    class JResourcePack
    {
        public JResourcePackInfo pack { get; set; }
    }

    class JResourcePackInfo
    {
        public int pack_format { get; set; }

        public string description { get; set; }
    }

    #endregion
}
