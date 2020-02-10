using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GBCLV3.Utils;

namespace GBCLV3.Models.Authentication
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum AccountEditType
    {
        [LocalizedDescription(nameof(AddAccount))]
        AddAccount,

        [LocalizedDescription(nameof(EditAccount))]
        EditAccount,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum AccountEditStatus
    {
        [LocalizedDescription(nameof(EnterAccountInformation))]
        EnterAccountInformation,

        [LocalizedDescription(nameof(CheckingAuthServer))]
        CheckingAuthServer,

        [LocalizedDescription(nameof(CheckAuthServerFailed))]
        CheckAuthServerFailed,

        [LocalizedDescription(nameof(Authenticating))]
        Authenticating,

        [LocalizedDescription(nameof(AuthFailed))]
        AuthFailed,

        [LocalizedDescription(nameof(AuthSuccessful))]
        AuthSuccessful,
    }
}
