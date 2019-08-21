using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using GBCLV3.Models.Launcher;

namespace GBCLV3.Utils
{
    [ValueConversion(typeof(VersionType), typeof(string))]
    class VersionTypeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((VersionType)value)
            {
                case VersionType.Release:
                    return "/GBCLV3;component/Resources/Images/grass_block.png";
                case VersionType.Snapshot:
                    return "/GBCLV3;component/Resources/Images/tnt.png";
                case VersionType.Forge:
                case VersionType.NewForge:
                    return "/GBCLV3;component/Resources/Images/observer.png";
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
