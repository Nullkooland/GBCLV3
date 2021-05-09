using System.Windows.Media.Imaging;

namespace GBCLV3.Models.Auxiliary
{
    public class ResourcePack
    {
        public string Id { get; set; }

        public int Format { get; set; }

        public string Description { get; set; }

        public BitmapImage Image { get; set; }

        public string Path { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsExtracted { get; set; }
    }

    #region Json Class

    public class JResourcePack
    {
        public JResourcePackInfo pack { get; set; }
    }

    public class JResourcePackInfo
    {
        public int pack_format { get; set; }

        public string description { get; set; }
    }

    #endregion
}
