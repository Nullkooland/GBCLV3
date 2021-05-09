using System;

namespace GBCLV3.Models
{
    internal enum LogLevel
    {
        Info,
        Debug,
        Warn,
        Error,
        Fatal,
        Minecraft,
    }

    internal class LogMessage
    {
        public LogLevel Level { get; set; }

        public DateTime Timestamp { get; set; }

        public string Tag { get; set; }

        public string Message { get; set; }

    }
}
