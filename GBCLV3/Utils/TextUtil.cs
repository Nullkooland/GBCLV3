using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GBCLV3.Utils
{
    static class TextUtil
    {
        public static bool IsValidEmailAddress(string emailAddress)
        {
            var regex = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            return !string.IsNullOrEmpty(emailAddress) && regex.IsMatch(emailAddress);
        }
    }
}
