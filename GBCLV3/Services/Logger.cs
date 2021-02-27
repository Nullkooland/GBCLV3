using System;
using System.Text;
using GBCLV3.Models;
using System.Threading.Tasks.Dataflow;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GBCLV3.Services
{
    public class Logger
    {
        //private const int MAX_MESSAGE_COUNT = 4096;

        private readonly ActionBlock<LogMessage> _outputJobs;

        private const string LOG_FILE = "GBCL.log";

        private readonly StreamWriter _writer;

        #region Constructor

        public Logger()
        {
            _outputJobs = new ActionBlock<LogMessage>(async logMessage =>
            {
                await WriteLogAsync(logMessage);
            });

            _writer = new StreamWriter(LOG_FILE, true);
        }

        #endregion

        #region Public Methods

        public void Info(string tag, string message)
        {
            _outputJobs.Post(new LogMessage { Level = LogLevel.Info, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Debug(string tag, string message)
        {
            _outputJobs.Post(new LogMessage { Level = LogLevel.Debug, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Warn(string tag, string message)
        {
            _outputJobs.Post(new LogMessage { Level = LogLevel.Warn, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Error(string tag, string message)
        {
            _outputJobs.Post(new LogMessage { Level = LogLevel.Error, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Fatal(string tag, string message)
        {
            _outputJobs.Post(new LogMessage { Level = LogLevel.Fatal, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Minecraft(string message)
        {
            _outputJobs.Post(new LogMessage { Level = LogLevel.Minecraft, Message = message });
        }

        public void Finish()
        {
            _outputJobs.Complete();
            _outputJobs.Completion.Wait();
            _writer.Flush();
            _writer.Dispose();
        }

        public void ClearLogs()
        {
            File.Delete(LOG_FILE);
        }

        #endregion

        #region Private Methods

        private async ValueTask WriteLogAsync(LogMessage logMessage)
        {
            if (logMessage.Level == LogLevel.Minecraft)
            {
                await _writer.WriteLineAsync(logMessage.Message);
                return;
            }

            var builder = new StringBuilder(1024);
            builder.Append($"[{logMessage.Timestamp:HH:mm:ss}] ");
            builder.Append($"[{logMessage.Level.ToString().ToUpper()}] ");
            builder.Append($"[{logMessage.Tag}] ");
            builder.Append(logMessage.Message);

            await _writer.WriteLineAsync(builder.ToString());
#if DEBUG
            System.Diagnostics.Debug.WriteLine(builder.ToString());
#endif
        }

        #endregion
    }
}
