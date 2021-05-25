using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;

namespace Netfox.Detective.ViewModelsDataEntity.Conversations
{
    public class ConversationDetailVm : DetectiveIvestigationDataEntityPaneViewModelBase
    {
        public ConversationDetailVm(WindsorContainer applicationWindsorContainer, ConversationVm model) : base(
            applicationWindsorContainer, model)
        {
            this.ConversationVm = model;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IConversationDetailView>());
        }

        #region Overrides of DetectivePaneViewModelBase

        public override string HeaderText => "Conversation detail";

        #endregion

        public ConversationVm ConversationVm { get; }
    }
}