using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Collections;
using Netfox.Detective.Models;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Models.Conversations;
using Netfox.Detective.Models.Exports;
using Netfox.Detective.Models.SourceLog;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Outputs;
using Netfox.Detective.ViewModelsDataEntity.SourceLogs;
using Netfox.Framework.ApplicationProtocolExport.Infrastructure;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Interfaces;

namespace Netfox.Detective.Interfaces.ViewModelsDataEntity.Investigations
{
    public interface IInvestigationVm
    {
        ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper> AvailableSnoopers { get; }

        IFrameworkController FrameworkController { get; set; }
        bool IsOpened { get; }
        ISnooperFactory SnooperFactory { get; }
        Investigation Investigation { get; }
        WindsorContainer GlobalWindsorContainer { get; set; }

        ViewModelVirtualizingIoCObservableCollection<CaptureVm, PmCaptureBase> Captures { get; }

        ViewModelVirtualizingIoCObservableCollection<ConversationsGroupVm, ConversationsGroup> ConversationsGroups
        {
            get;
        }

        ViewModelVirtualizingIoCObservableCollection<ExportGroupVm, ExportGroup> ExportGroups { get; }

        ViewModelVirtualizingIoCObservableCollection<OperationLogVm, OperationLog> OperationLogs { get; }
        ViewModelVirtualizingIoCObservableCollection<SourceLogVm, SourceLog> SourceLogs { get; }

        IEnumerable<ExportGroupVm> ExportGroupsFlat { get; }

        CaptureVm CurrentCapture { get; set; }

        ConcurrentObservableHashSet<CaptureVm> SelectedCaptureVms { get; }

        ConversationsGroupVm CurrentConversationsGroupVm { get; set; }

        ConcurrentObservableHashSet<ConversationsGroupVm> SelectedConversationsGroupVms { get; }

        ExportGroupVm CurrentExportGroup { get; set; }
        ConcurrentObservableHashSet<ExportGroupVm> SelectedExportGroupVms { get; }
        long ToExportTotalSize { get; }

        ConcurrentObservableCollection<ILxConversation> ToExportConversations { get; }
        string ExportGroupName { get; set; }

        bool ShowExportedObjects { get; set; }

        bool DontUseApplicationTag { get; set; }

        bool CreateSubGroup { get; set; }

        bool ExportToRootGroups { get; set; }

        bool ExportToRootGroupsInver { get; }

        ICommand CExport { get; }

        ICommand CAddNewGroup { get; }

        ICommand CRemoveGroup { get; }

        RelayCommand<SourceLogVm> CRemoveLog { get; }

        ICommand CRemoveCapture { get; }

        ICommand CProtocolsRecognition { get; }

        Task Initialize();

        Task AddCaptureAsync(string filePath);

        void AddCaptureToExport(CaptureVm capture);

        void AddConversationsGroupToExport(ConversationsGroupVm conversationsGroup);

        void AddConversationToExport(ILxConversation conversation);

        Task AddLogFile(string filePath);

        void AddNewExportGroup();

        Task AddSourceLog();

        Task CheckCapturesChecksums();

        void Close();

        ExportGroupVm ExportGroupByExportResult(ExportVm selectedExportResult);

        Task Init();

        void Open();

        void ProtocolsRecognition();

        Task RemoveCaptureAsync(PmCaptureBase capture);

        Task RemoveCaptureAsync(CaptureVm capture);

        void RemoveConversationsGroup(ConversationsGroupVm conversationsGroup);

        void RemoveExportGroup(ExportGroupVm exportGroupVm);

        CrossContainerHierarchyResolver CrossContainerHierarchyResolver { get; set; }
    }
}