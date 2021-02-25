using GBCLV3.Utils.Binding;
using System.ComponentModel;

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
