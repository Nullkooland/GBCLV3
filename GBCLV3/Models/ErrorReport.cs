using System.ComponentModel;
using GBCLV3.Utils.Binding;

namespace GBCLV3.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ErrorReportType
    {
        [LocalizedDescription("ReportUnexpectedExit")]
        UnexpectedExit,

        [LocalizedDescription("ReportUnhandledException")]
        UnhandledException,
    }
}
