using GBCLV3.Models.Launch;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GBCLV3.Utils
{
    [ValueConversion(typeof(VersionType), typeof(string))]
    internal class VersionTypeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((VersionType)value)
            {
                case VersionType.Release:
                    return "/GBCL;component/Resources/Images/grass_block.png";
                case VersionType.Snapshot:
                    return "/GBCL;component/Resources/Images/tnt.png";
                case VersionType.Forge:
                case VersionType.NewForge:
                    return "/GBCL;component/Resources/Images/observer.png";
                case VersionType.OptiFine:
                    return "/GBCL;component/Resources/Images/snow.png";
                case VersionType.Fabric:
                    return "/GBCL;component/Resources/Images/glass.png";
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
