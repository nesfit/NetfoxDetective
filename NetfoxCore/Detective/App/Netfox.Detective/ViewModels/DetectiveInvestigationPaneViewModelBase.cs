using Castle.Windsor;

namespace Netfox.Detective.ViewModels
{
    /// <summary>
    ///     Use as a base class of any singletonouse investigation related ViewModel docked in panes
    /// </summary>
    public abstract class DetectiveInvestigationPaneViewModelBase : DetectivePaneViewModelBase
    {
        protected DetectiveInvestigationPaneViewModelBase(IWindsorContainer applicationWindsorContainer) : base(
            applicationWindsorContainer)
        {
            //Call this in constructor of inhereted members
            //DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.InvestigationOrAppWindsorContainer.Resolve<IXXXView>());
        }
    }
}