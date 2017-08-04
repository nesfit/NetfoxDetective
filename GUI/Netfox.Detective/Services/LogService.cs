// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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

using System.Threading.Tasks;
using Castle.Core;
using Castle.Core.Logging;
using GalaSoft.MvvmLight.Messaging;
using Netfox.Core.Interfaces;
using Netfox.Core.Messages.Base;
using Netfox.Core.Properties;
using Netfox.Detective.Models.Base;
using Netfox.Logger;

namespace Netfox.Detective.Services
{
    public sealed class LogService : IStartableDetectiveService
    {
        public NetfoxLogger NetfoxLogger { get; private set; }

        public LogService(NetfoxLogger netfoxLogger)
        {
            this.NetfoxLogger = netfoxLogger;
            Task.Factory.StartNew(() => Messenger.Default.Register<InvestigationMessage>(this, this.InvestigationMessageHandler));
            this.LoadSettings();
        }
        

        private void InvestigationMessageHandler(InvestigationMessage investigationMessage)
        {
            if(investigationMessage == null) { return; }
            switch(investigationMessage.Type)
            {
                case InvestigationMessage.MessageType.InvestigationCreated:
                    break;
                case InvestigationMessage.MessageType.CurrentInvestigationChanged:
                    var investigation = investigationMessage.Investigation as Investigation;
                    this.NetfoxLogger.ChangeLoggingDirectory(investigation?.InvestigationInfo?.LogsDirectoryInfo);
                    break;
            }
        }


        private void LoadSettings()
        {
            var levelString = NetfoxSettings.Default.ExplicitNotifications;
            var level = LoggerLevel.Debug;
            if(LoggerLevel.TryParse(levelString, true, out level)) { this.NetfoxLogger.ExplicitLoggerLevel= level; }

            levelString = NetfoxSettings.Default.ToLogMessages;
            if (LoggerLevel.TryParse(levelString, true, out level)) { this.NetfoxLogger.BackgroundLoggerLevel= level; }
        }

        #region Implementation of IStartableDetectiveService
        [DoNotWire]
        public ILogger Logger
        {
            get { return this.NetfoxLogger; }
            set { this.NetfoxLogger = value as NetfoxLogger; }
        }
        #endregion
    }
}