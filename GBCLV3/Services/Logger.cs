using System;
using System.Text;
using GBCLV3.Models;
using System.Threading.Tasks.Dataflow;
using System.IO;
using System.Linq;

namespace GBCLV3.Services
{
    public class Logger
    {
        //private const int MAX_MESSAGE_COUNT = 4096;

        private readonly ActionBlock<LogMessage> _outJobs;

        private readonly string _logFile;

        private readonly StreamWriter _writer;

        #region Constructor

        public Logger()
        {
            _outJobs = new ActionBlock<LogMessage>(logMessage =>
            {
                ProcessOutMessages(logMessage);
            });

            _logFile = "GBCL_logs_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".log";
            _writer = new StreamWriter(File.OpenWrite(_logFile));
        }

        #endregion

        #region Public Methods

        public void Info(string tag, string message)
        {
            _outJobs.Post(new LogMessage { Type = LogType.Info, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Debug(string tag, string message)
        {
            _outJobs.Post(new LogMessage { Type = LogType.Debug, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Warn(string tag, string message)
        {
            _outJobs.Post(new LogMessage { Type = LogType.Warn, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Error(string tag, string message)
        {
            _outJobs.Post(new LogMessage { Type = LogType.Error, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Error(string tag, Exception exception)
        {
            _outJobs.Post(new LogMessage { Type = LogType.Error, Tag = tag, Message = exception.Message, Timestamp = DateTime.Now });
        }

        public void Crash(string tag, string message)
        {
            _outJobs.Post(new LogMessage { Type = LogType.Crash, Tag = tag, Message = message, Timestamp = DateTime.Now });
        }

        public void Finish()
        {
            _writer.Flush();
            _writer.Dispose();
        }

        public void ClearLogs()
        {
            var logFiles = Directory.EnumerateFiles(Environment.CurrentDirectory)
                                    .Where(file => Path.GetFileName(file).StartsWith("GBCL_logs_"));

            foreach (var file in logFiles)
            {
                File.Delete(file);
            }
        }

        #endregion

        #region Private Methods

        private void ProcessOutMessages(LogMessage logMessage)
        {
            var builder = new StringBuilder(1024);
            builder.Append($"[{logMessage.Timestamp}] ");
            builder.Append($"[{logMessage.Type.ToString().ToUpper()}] ");
            builder.Append($"[{logMessage.Tag}] ");
            builder.Append(logMessage.Message);
            builder.AppendLine();

            _writer.Write(builder.ToString());
        }

        #endregion
    }
}
