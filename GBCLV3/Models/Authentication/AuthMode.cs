using System.ComponentModel;
using GBCLV3.Utils.Binding;

namespace GBCLV3.Models.Authentication
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AuthMode
    {
        [LocalizedDescription(nameof(Offline))]
        Offline,

        [LocalizedDescription(nameof(Yggdrasil))]
        Yggdrasil,

        [LocalizedDescription(nameof(AuthLibInjector))]
        AuthLibInjector
    }
}
