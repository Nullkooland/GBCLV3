namespace GBCLV3.Models.Auxiliary
{
    public class Mod
    {
        public string Name { get; set; }

        public string FileName { get; set; }

        public string DisplayName => !string.IsNullOrWhiteSpace(Description) ? 
                                        $"{Description}\nby {Authors}" : null;

        public string Description { get; set; }

        public string Version { get; set; }

        public string GameVersion { get; set; }

        public string Authors { get; set; }

        public string Url { get; set; }

        public string Path { get; set; }

        public bool IsEnabled { get; set; }
    }

    #region Json Class

    public class JMod
    {
        public JMod[] modList { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string version { get; set; }

        public string mcversion { get; set; }

        public string[] authors { get; set; }

        public string[] authorList { get; set; }

        public string url { get; set; }
    }

    #endregion
}
