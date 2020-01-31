using GBCLV3.Utils;
using System.ComponentModel;

namespace GBCLV3.Models.Download
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum DownloadSource
    {
        [LocalizedDescription("Official")]
        Official,

        [LocalizedDescription("BMCLAPI")]
        BMCLAPI,
    }
}
