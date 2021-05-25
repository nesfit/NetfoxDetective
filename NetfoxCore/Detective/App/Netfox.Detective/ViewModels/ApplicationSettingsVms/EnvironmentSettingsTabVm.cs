using Castle.Core.Logging;
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Infrastructure;
using Netfox.Logger;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.ApplicationSettingsVms
{
    public class EnvironmentSettingsTabVm : SettingsBaseVm
    {
        public EnvironmentSettingsTabVm(WindsorContainer applicationWindsorContainer, NetfoxLogger netfoxLogger) : base(
            applicationWindsorContainer)
        {
            this.NetfoxLogger = netfoxLogger;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IEnvironmentSettingsTab>());
        }

        public override string HeaderText => "Environment";
        public NetfoxLogger NetfoxLogger { get; }

        [SafeForDependencyAnalysis]
        public LoggerLevel ExplicitLoggingLevels
        {
            get { return this.NetfoxLogger.ExplicitLoggerLevel; }
            set { this.NetfoxLogger.ExplicitLoggerLevel = value; }
        }
    }
}