namespace GBCLV3.Models.Launcher
{
    enum LibraryType
    {
        Minecraft,
        Native,
        Maven,
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
}
