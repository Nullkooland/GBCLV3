using System;
using System.Windows.Markup;

namespace GBCLV3.Utils
{
    class EnumBindingSourceExtension : MarkupExtension
    {
        private readonly Type _enumType;

        public EnumBindingSourceExtension(Type enumType)
        {
            _enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_enumType);
        }
    }
}
