using System.ComponentModel;
using GBCLV3.Services;

namespace GBCLV3.Utils
{
    class LocalizedDescriptionAttribute : DescriptionAttribute
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
