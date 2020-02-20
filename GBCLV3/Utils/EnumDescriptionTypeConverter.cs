using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace GBCLV3.Utils
{
    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type) : base(type)
        {
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            var fi = value?.GetType().GetField(value.ToString());
            var attributes = fi?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes.Any() ? attributes[0].Description : null;
        }
    }
}