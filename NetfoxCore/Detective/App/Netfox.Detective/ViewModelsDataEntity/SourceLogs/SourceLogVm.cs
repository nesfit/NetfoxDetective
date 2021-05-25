using Castle.Windsor;
using Netfox.Detective.Models.SourceLog;

namespace Netfox.Detective.ViewModelsDataEntity.SourceLogs
{
    public class SourceLogVm : DetectiveDataEntityViewModelBase
    {
        public SourceLogVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
        }

        public SourceLogVm(WindsorContainer applicationWindsorContainer, SourceLog model) : base(
            applicationWindsorContainer, model)
        {
            this.SourceLog = model;
        }

        public SourceLog SourceLog { get; set; }
    }
}