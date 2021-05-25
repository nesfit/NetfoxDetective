using System.Collections.Generic;
using System.Text;
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Conversations
{
    public class ReassembledStreamDetailVm : DetectiveIvestigationDataEntityPaneViewModelBase
    {
        public ReassembledStreamDetailVm(WindsorContainer applicationWindsorContainer, ConversationVm model) : base(
            applicationWindsorContainer, model)
        {
            this.ConversationVm = model;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IReassembledStreamDetailView>());
        }

        #region Overrides of DetectivePaneViewModelBase

        [SafeForDependencyAnalysis]
        public override string HeaderText => "Reassembled stream " + this.ConversationVm.Conversation.Name;

        #endregion

        public ConversationVm ConversationVm { get; }

        public IEnumerable<EncodingInfo> Encodings => Encoding.GetEncodings();
    }
}