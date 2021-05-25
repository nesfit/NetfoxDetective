using Castle.Windsor;
using Netfox.Core.Interfaces.Model.Exports;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers
{
    public class CallVm : DetectiveDataEntityViewModelBase
    {
        public CallVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
        }

        public CallVm(WindsorContainer applicationWindsorContainer, object model, ExportVm exportVm) : base(
            applicationWindsorContainer, model)
        {
            this.Call = model as ICall;
            this.ExportVm = exportVm;
        }

        public ICall Call { get; }
        public ExportVm ExportVm { get; }
    }
}