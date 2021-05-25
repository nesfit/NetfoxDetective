using Castle.Windsor;
using Netfox.Core.Interfaces.Model.Exports;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers
{
    public class ChatMessageVm : DetectiveDataEntityViewModelBase
    {
        public ChatMessageVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
        }

        public ChatMessageVm(WindsorContainer applicationWindsorContainer, object model, ExportVm exportVm) : base(
            applicationWindsorContainer, model)
        {
            this.ChatMessage = model as IChatMessage;
            this.ExportVm = exportVm;
        }

        public IChatMessage ChatMessage { get; }
        public ExportVm ExportVm { get; }
    }
}