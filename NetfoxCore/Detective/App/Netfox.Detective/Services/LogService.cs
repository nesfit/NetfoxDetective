using System.Threading.Tasks;
using Castle.Core;
using Castle.Core.Logging;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Investigations;
using Netfox.Detective.Messages.Workspaces;
using Netfox.Detective.Models.Base;
using Netfox.Logger;

namespace Netfox.Detective.Services
{
    public sealed class LogService : IStartableDetectiveService
    {
        public NetfoxLogger NetfoxLogger { get; private set; }

        private readonly INetfoxSettings _settings;

        public LogService(NetfoxLogger netfoxLogger, IDetectiveMessenger messenger, INetfoxSettings settings)
        {
            this.NetfoxLogger = netfoxLogger;
            _settings = settings;
            
            Task.Factory.StartNew(() =>
                messenger.Register<OpenedInvestigationMessage>(this, this.OpenedInvestigationMessageReceived));
            Task.Factory.StartNew(() =>
                messenger.Register<ClosedWorkspaceMessage>(this, this.ClosedWorkspaceMessageReceived));
            this.LoadSettings();
        }

        private void OpenedInvestigationMessageReceived(OpenedInvestigationMessage msg)
        {
            var investigation = msg.Investigation as Investigation;
            this.NetfoxLogger.ChangeLoggingDirectory(investigation?.InvestigationInfo?.LogsDirectoryInfo);
        }

        private void ClosedWorkspaceMessageReceived(ClosedWorkspaceMessage msg)
        {
            NetfoxLogger.CloseLoggingDirectory();
        }

        private void LoadSettings()
        {
            var levelString = _settings.ExplicitNotifications;
            var level = LoggerLevel.Debug;
            if (LoggerLevel.TryParse(levelString, true, out level))
            {
                this.NetfoxLogger.ExplicitLoggerLevel = level;
            }

            levelString = _settings.ToLogMessages;
            if (LoggerLevel.TryParse(levelString, true, out level))
            {
                this.NetfoxLogger.BackgroundLoggerLevel = level;
            }
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