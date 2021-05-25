using Castle.Windsor;
using Netfox.Detective.Core.BaseTypes;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels
{
    /// <summary>
    ///     Base class to be derivated only by abstract classes defining base types for pane components
    /// </summary>
    [NotifyPropertyChanged]
    public abstract class DetectivePaneViewModelBase : DetectiveViewModelBase
    {
        protected DetectivePaneViewModelBase(IWindsorContainer applicationWindsorContainer) : base(
            applicationWindsorContainer)
        {
            this.IsHidden = true;
            //Call this in constructor of inhereted members
            //DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.InvestigationOrAppWindsorContainer.Resolve<IXXXView>());
        }

        public virtual DetectiveDockPosition DockPositionPosition { get; set; } = DetectiveDockPosition.DockedDocument;
    }
}