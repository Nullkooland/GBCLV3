using GBCLV3.Utils.Binding;
using System.ComponentModel;

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
