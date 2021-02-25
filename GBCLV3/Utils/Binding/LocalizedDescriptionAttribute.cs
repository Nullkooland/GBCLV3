using GBCLV3.Services;
using System.ComponentModel;

namespace GBCLV3.Utils.Binding
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public static LanguageService LanguageService { get; set; }

        private readonly string _resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey)
        {
            _resourceKey = resourceKey;
        }

        public override string Description => LanguageService.GetEntry(_resourceKey);
    }
}
