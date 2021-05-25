using Castle.Windsor;
using Netfox.Detective.Services;
using Netfox.Framework.Models.PmLib.Captures;

namespace Netfox.Detective.ViewModelsDataEntity.ConversationsCollections
{
    public abstract class MockConversationsVm : ConversationsVm<PmCaptureBase>
    {
        protected MockConversationsVm(WindsorContainer applicationWindsorContainer, PmCaptureBase model,
            ExportService exportService) : base(applicationWindsorContainer, model, exportService)
        {
        }
    }
}