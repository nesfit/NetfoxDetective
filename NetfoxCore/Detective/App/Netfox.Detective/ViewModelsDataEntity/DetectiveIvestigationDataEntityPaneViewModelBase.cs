using Castle.Windsor;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.ViewModels;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity
{
    /// <summary>
    ///     Use as a base class of any transient data entity ViewModels docked in panes
    /// </summary>
    [NotifyPropertyChanged]
    public abstract class DetectiveIvestigationDataEntityPaneViewModelBase : DetectivePaneViewModelBase, IDataEntityVm
    {
        protected DetectiveIvestigationDataEntityPaneViewModelBase(IWindsorContainer applicationWindsorContainer,
            object model) : base(applicationWindsorContainer)
        {
            this.EncapsulatedModel = model;

            //Call this in constructor of inhereted members
            //DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.InvestigationOrAppWindsorContainer.Resolve<IXXXView>());
        }

        #region Implementation of IDataEntityVm

        public object EncapsulatedModel { get; }

        #endregion
    }
}