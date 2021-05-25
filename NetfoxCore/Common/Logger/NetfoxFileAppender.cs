using System;
using System.IO;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace Netfox.Logger
{
    public class NetfoxFileAppender : NetfoxAppenderBase
    {
        public NetfoxFileAppender(ILoggerSettings settings)
        {
            var logDirectory = new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), settings.AppDataLogPath));
            var loggerFileInfo = GetLoggerFileInfo(logDirectory);
            this.FileAppender = CreateFileAppender(loggerFileInfo);
        }

        private static FileInfo GetLoggerFileInfo(DirectoryInfo logDirectory)
        {
            return new FileInfo(Path.Combine(logDirectory.FullName,
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log"));
        }

        private static IAppender CreateFileAppender(FileInfo loggerFilePath)
        {
            var rollingFileAppender = new RollingFileAppender
            {
                Name = "RollFileAppender",
                File = loggerFilePath.FullName,
                StaticLogFileName = true,
                AppendToFile = true,
                LockingModel = new FileAppender.ExclusiveLock(),
                RollingStyle = RollingFileAppender.RollingMode.Size,
                MaxSizeRollBackups = 10,
                MaximumFileSize = "10MB",
                ImmediateFlush = true
            };

            var layout = new PatternLayout
            {
                ConversionPattern = "%date{yyyy-MM-dd} %date{hh:mm:ss.ff}  [%level]  %message%newline"
            };

            rollingFileAppender.Layout = layout;
            rollingFileAppender.ActivateOptions();
            layout.ActivateOptions();

            var bufappender = new BufferingForwardingAppender
            {
                Name = "BuffAppender",
                BufferSize = 512,
                Fix = 0,
                Lossy = false,
                Evaluator = new LevelEvaluator(Level.Error)
            };

            bufappender.ActivateOptions();
            bufappender.AddAppender(rollingFileAppender);

            return bufappender;
        }

        public IAppender FileAppender { get; private set; }

        #region Implementation of IAppender

        public override void Close()
        {
            this.FileAppender.Close();
        }

        protected override void DoAppendApproved(LoggingEvent loggingEvent)
        {
            this.FileAppender.DoAppend(loggingEvent);
        }

        #endregion

        public void ChangeLoggingDirectory(DirectoryInfo directoryInfo)
        {
            this.FileAppender.Close();
            this.FileAppender = CreateFileAppender(GetLoggerFileInfo(directoryInfo));
        }


        #region Overrides of NetfoxAppenderBase

        public override string Name { get; set; } = "Netfox file logger";

        #endregion
    }
}