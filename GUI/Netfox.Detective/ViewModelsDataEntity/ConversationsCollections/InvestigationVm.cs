using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netfox.Detective.Core.Interfaces.ViewModels;
using Netfox.Detective.Models.Base;
using Netfox.Detective.ViewModels;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Framework.Interfaces;

namespace Netfox.Detective.ViewModelsDataEntity.ConversationsCollections
{
    public class InvestigationVm : DetectiveDataEntityViewModelBase
    {
        public Investigation Investigation { get; set; }
        public IEnumerable<ExportGroupVm> ExportGroups { get; set; }
        public CaptureVmBase CurrentCapture { get; set; }
        public ObservableCollection<IConversationsVm> ToExportConversations { get; set; }
        public ConversationsGroupVmBase CurrentConversationsGroupVmBase { get; set; }
        public ConversationVm CurrentConversation { get; set; }
        public ExportGroupVm CurrentExportGroup { get; set; }

        public InvestigationVm(Investigation investigation) {
            this.Investigation = investigation;
        }

        public void ExportExecute() { throw new NotImplementedException(); }
        public ExportGroupVm ExportGroupByExportResult(ExportResultVm selectedExportResult) { throw new NotImplementedException(); }
        public void AddNewExportGroup() { throw new NotImplementedException(); }
        public void AddCapture(string captureFile) { throw new NotImplementedException(); }
    }
}
