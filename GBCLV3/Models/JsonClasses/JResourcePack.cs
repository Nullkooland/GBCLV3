namespace GBCLV3.Models.JsonClasses
{
    class JResourcePack
    {
        public JResourcePackInfo pack { get; set; }
    }

    class JResourcePackInfo
    {
        public int pack_format { get; set; }

        public string description { get; set; }
    }
}
