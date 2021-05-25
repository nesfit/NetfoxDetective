using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Core.BaseTypes;
using Netfox.Detective.Models;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Outputs
{
    public class OperationLogVm : DetectiveIvestigationDataEntityPaneViewModelBase, IDataEntityVm
    {
        public OperationLogVm(WindsorContainer applicationWindsorContainer, OperationLog model) : base(
            applicationWindsorContainer, model)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IOperationLogView>());
            this.OperationLog = model;
            this.DockPositionPosition = DetectiveDockPosition.DockedBottom;
        }

        [SafeForDependencyAnalysis] public override string HeaderText => this.OperationLog.Name;

        public OperationLog OperationLog { get; set; }
    }
}