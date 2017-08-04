// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.IO;
using Castle.Core.Logging;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using Netfox.Logger.Properties;

namespace Netfox.Logger
{
    public class NetfoxFileAppender : NetfoxAppenderBase
    {
        
        public NetfoxFileAppender()
        {
            var logDirectory = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Settings.Default.AppDataLogPath));
            var loggerFileInfo = GetLoggerFileInfo(logDirectory);
            this.FileAppender = CreateFileAppender(loggerFileInfo);
        }

        private static FileInfo GetLoggerFileInfo(DirectoryInfo logDirectory) { return new FileInfo(Path.Combine(logDirectory.FullName, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log")); }

        private static IAppender CreateFileAppender(FileInfo loggerFilePath)
        {
            var rollingFileAppender = new RollingFileAppender
            {
                Name = "RollFileAppender",
                File = loggerFilePath.FullName,
                StaticLogFileName = true,
                AppendToFile = false,
                LockingModel = new FileAppender.ExclusiveLock(),
                RollingStyle = RollingFileAppender.RollingMode.Size,
                MaxSizeRollBackups = 10,
                MaximumFileSize = "10MB",
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

        public IAppender FileAppender{ get; private set; }

        #region Implementation of IAppender
        public override void Close() { this.FileAppender.Close(); }
        protected override void DoAppendApproved(LoggingEvent loggingEvent) { this.FileAppender.DoAppend(loggingEvent); }
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