using System.ComponentModel;
using GBCLV3.Utils;

namespace GBCLV3.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum ErrorReportType
    {
        [LocalizedDescription("ReportUnexpectedExit")]
        UnexpectedExit,

        [LocalizedDescription("ReportUnhandledException")]
        UnhandledException,
    }
}
