using Castle.Windsor;
using Netfox.Core.Infrastructure;

namespace Netfox.Detective.ViewModels.ApplicationSettingsVms
{
    public abstract class SettingsBaseVm : DetectiveApplicationPaneViewModelBase
    {
        protected SettingsBaseVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
        }
    }
}