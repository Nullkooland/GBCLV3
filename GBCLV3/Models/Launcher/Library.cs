namespace GBCLV3.Models.Launcher
{
    enum LibraryType
    {
        Minecraft,
        Native,
        Forge,
    }

    /// <summary>
    /// Dependent library for Minecraft
    /// </summary>
    class Library
    {
        /// <summary>
        /// Name of library
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of library
        /// </summary>
        public LibraryType Type { get; set; }

        /// <summary>
        /// Relative path to ./libraries directory
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///  Size of library
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// SHA-1 digital signature of library
        /// </summary>
        public string SHA1 { get; set; }

        /// <summary>
        /// Entries excluded when extracting (applies only for native lib)
        /// </summary>
        public string[] Exclude { get; set; }

        /// <summary>
        /// Override comparator
        /// </summary>
        public override bool Equals(object obj)
        {
            Library other = obj as Library;
            return other?.Name == this.Name;
        }

        /// <summary>
        /// Override hash
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
