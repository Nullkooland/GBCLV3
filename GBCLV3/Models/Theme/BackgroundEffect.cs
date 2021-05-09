using System.ComponentModel;
using GBCLV3.Utils.Binding;

namespace GBCLV3.Models.Theme
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum BackgroundEffect
    {
        [LocalizedDescription(nameof(SolidColor))]
        SolidColor,

        [LocalizedDescription(nameof(BlurBehind))]
        BlurBehind,

        [LocalizedDescription(nameof(AcrylicMaterial))]
        AcrylicMaterial,
    }
}
