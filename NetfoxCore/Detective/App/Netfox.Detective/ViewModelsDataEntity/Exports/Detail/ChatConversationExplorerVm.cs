using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Detective.Core.BaseTypes;
using Netfox.Framework.Models.Snoopers;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail
{
    [NotifyPropertyChanged]
    public class ChatConversationExplorerVm : DetectiveExportDetailPaneViewModelBase
    {
        public ChatConversationExplorerVm(WindsorContainer applicationWindsorContainer, ExportVm model,
            IChatConversationExplorerView view)
            : base(applicationWindsorContainer, model, view)
        {
            try
            {
                this.IsHidden = !this.ExportVm.ChatMessages.Any();
                this.IsActive = this.ExportVm.ChatMessages.Any();
                this.IsShown = this.ExportVm.ChatMessages.Any();
                this.DockPositionPosition = DetectiveDockPosition.DockedLeft;
                this.ExportVmObserver.RegisterHandler(p => p.SelectedSnooperExportObject,
                    p => this.OnPropertyChanged(nameof(this.SelectedChatMessage)));

                this.Conversations = this.ExportVm.ChatConversations;
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        #region Overrides of DetectivePaneViewModelBase

        public override string HeaderText => "ChatMessagesExplorer";

        #endregion

        public IEnumerable<KeyValuePair<IChatMessage, IEnumerable<IChatMessage>>> Conversations { get; set; }

        [IgnoreAutoChangeNotification]
        public IEnumerable<IChatMessage> ChatMessages =>
            this.ExportVm.ChatMessages.Select(chatMessageVm => chatMessageVm.ChatMessage);

        public IChatMessage SelectedChatMessage
        {
            get { return this.ExportVm.SelectedSnooperExportObject as IChatMessage; }
            set
            {
                var chatMessage = value as SnooperExportedObjectBase;
                if (chatMessage != null)
                {
                    this.ExportVm.SelectedSnooperExportObject = chatMessage;
                }
            }
        }

        [SafeForDependencyAnalysis] public DateTime TimeStampFirst => this.ExportVm.Export.TimeStampFirst;
        [SafeForDependencyAnalysis] public DateTime TimeStampLast => this.ExportVm.Export.TimeStampLast;
        [SafeForDependencyAnalysis] public TimeSpan Duration => this.ExportVm.Duration;
    }
}