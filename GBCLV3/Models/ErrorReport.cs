using GBCLV3.Utils.Binding;
using System.ComponentModel;

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
