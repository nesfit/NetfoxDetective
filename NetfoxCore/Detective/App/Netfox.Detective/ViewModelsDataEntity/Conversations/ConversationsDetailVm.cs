using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;

namespace Netfox.Detective.ViewModelsDataEntity.Conversations
{
    public class ConversationsDetailVm : DetectiveIvestigationDataEntityPaneViewModelBase
    {
        public ConversationsDetailVm(WindsorContainer applicationWindsorContainer, IConversationsVm model) : base(
            applicationWindsorContainer, model)
        {
            this.ConversationsVm = model;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IConversationsDetailView>());
        }

        public IConversationsVm ConversationsVm { get; }

        #region Overrides of DetectivePaneViewModelBase

        public override string HeaderText => "Conversations detail";

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                var conversationsVm = this.ConversationsVm;
                if (conversationsVm != null)
                {
                    conversationsVm.IsSelected = value;
                }
            }
        }

        #endregion
    }
}