using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Core.BaseTypes;
using Netfox.Detective.ViewModelsDataEntity;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Conversations
{
    [NotifyPropertyChanged]
    public class ConversationsOverviewVm : DetectiveIvestigationDataEntityPaneViewModelBase
    {
        private IConversationsVm _conversationsVm;

        public ConversationsOverviewVm(WindsorContainer applicationWindsorContainer, IConversationsVm model) : base(
            applicationWindsorContainer, model)
        {
            this.ConversationsVm = model;
            this.NavigationService.Show(typeof(ConversationsDetailVm), model,
                false); //hack to open pane in background while opening overview ... temporary todo
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IConversationsOverviewView>());
            //Binder.Bind(this, "IsSelected", this.ConversationsVm, "IsSelected", BindingDirection.OneWay);
            //Task.Factory.StartNew(() => Messenger.Default.Register<CaptureMessage>(this, this.CaptureActionHandler));
            //this.PropertyObserver = new PropertyObserver<IConversationsVm>(this.ConversationsVm);
            //this.PropertyObserver.RegisterHandler(n => n.CurrentConversation,
            //    n => ConversationMessage.SendConversationMessage(n.CurrentConversation, ConversationMessage.MessageType.CurrentConversationChanged, false));
        }

        public override string HeaderText => "Conversations overview";

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

        public PropertyObserver<IConversationsVm> PropertyObserver { get; set; }

        public IConversationsVm ConversationsVm
        {
            get { return this._conversationsVm; }
            set
            {
                this._conversationsVm = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("IsCaptureVm");
                this.OnPropertyChanged("IsConversationGroupVm");
            }
        }

        [SafeForDependencyAnalysis] public bool IsCaptureVm => this.ConversationsVm?.GetType() == typeof(CaptureVm);

        [SafeForDependencyAnalysis]
        public bool IsConversationGroupVm => this.ConversationsVm?.GetType() == typeof(ConversationsGroupVm);
    }
}