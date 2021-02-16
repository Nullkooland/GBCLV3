using GBCLV3.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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
