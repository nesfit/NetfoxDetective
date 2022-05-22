using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.Outputs;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Outputs
{
    public class OperationOutputVm : DetectiveApplicationPaneViewModelBase
    {
        public OperationOutputVm(WindsorContainer applicationWindsorContainer, OperationLogVm logVm) : base(
            applicationWindsorContainer)
        {
            this.OperationLog = logVm;
        }

        [SafeForDependencyAnalysis]
        public override string HeaderText => "Operation - " + this.OperationLog.OperationLog.Name;

        public OperationLogVm OperationLog { get; set; }
    }
}