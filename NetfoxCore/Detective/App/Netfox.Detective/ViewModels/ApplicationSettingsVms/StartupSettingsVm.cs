using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces.Views;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.ApplicationSettingsVms
{
    public class StartupSettingsVm : SettingsBaseVm
    {
        private readonly INetfoxSettings _settings;
        
        public StartupSettingsVm(WindsorContainer applicationWindsorContainer, INetfoxSettings settings) : base(applicationWindsorContainer)
        {
            _settings = settings;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IStartUpSettingsTab>());
        }

        public override string HeaderText => "Startup";

        [SafeForDependencyAnalysis]
        public bool AutoLoadLastSession
        {
            get { return _settings.AutoLoadLastSession; }
            set
            {
                _settings.AutoLoadLastSession = value;
                this.OnPropertyChanged();
            }
        }
    }
}