using System;
using System.Linq;
using Castle.Windsor;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Detective.Core.BaseTypes;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail
{
    [NotifyPropertyChanged]
    public class ChatConversationDetailVm : DetectiveExportDetailPaneViewModelBase
    {
        public ChatConversationDetailVm(WindsorContainer applicationWindsorContainer, ExportVm model,
            IChatConversationDetailView view)
            : base(applicationWindsorContainer, model, view)
        {
            try
            {
                this.IsHidden = !this.ExportVm.SelectedChatConversation.Value?.Any() ?? true;
                this.IsActive = this.ExportVm.SelectedChatConversation.Value?.Any() ?? false;
                this.DockPositionPosition = DetectiveDockPosition.DockedDocument;
                this.ExportVmObserver.RegisterHandler(p => p.SelectedChatConversation, p =>
                {
                    this.IsHidden = !this.ExportVm.SelectedChatConversation.Value?.Any() ?? true;
                    this.IsActive = this.ExportVm.SelectedChatConversation.Value?.Any() ?? false;
                });
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        #region Overrides of DetectivePaneViewModelBase

        public override string HeaderText => "Chat conversation";

        #endregion
    }
}