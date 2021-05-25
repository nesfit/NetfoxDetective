using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces.Views;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.ApplicationSettingsVms
{
    public class EncapsulationSettingsVm : SettingsBaseVm
    {
        private readonly INetfoxSettings _settings;
        
        public EncapsulationSettingsVm(WindsorContainer applicationWindsorContainer, INetfoxSettings settings) : base(applicationWindsorContainer)
        {
            _settings = settings;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IEncapsulationSettingsTab>());
        }

        public override string HeaderText => "Encapsulation";

        [SafeForDependencyAnalysis]
        public bool DecapsulateGseOverUdp
        {
            get => _settings.DecapsulateGseOverUdp;
            set
            {
                _settings.DecapsulateGseOverUdp = value;
                _settings.Save();
                this.OnPropertyChanged();
            }
        }
    }
}