using System;
using System.Collections.Generic;
using System.Text;

namespace GBCLV3.Models
{
    enum LogType
    {
        Info,
        Debug,
        Warn,
        Error,
        Crash,
        Minecraft,
    }

    class LogMessage
    {
        public LogType Type { get; set; }

        public DateTime Timestamp { get; set; }

        public string Tag { get; set; }

        public string Message { get; set; }

    }
}
