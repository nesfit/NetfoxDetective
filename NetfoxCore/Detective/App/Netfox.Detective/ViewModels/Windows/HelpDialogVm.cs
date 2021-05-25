using Castle.Windsor;
using Netfox.Core.Interfaces.Views;

namespace Netfox.Detective.ViewModels.Windows
{
    public class HelpDialogVm : DetectiveWindowViewModelBase
    {
        public HelpDialogVm(WindsorContainer applicationOrAppWindsorContainer) : base(applicationOrAppWindsorContainer)
        {
            this.ViewType = typeof(IHelpDialog);
        }

        public override string HeaderText => "Help";
    }
}