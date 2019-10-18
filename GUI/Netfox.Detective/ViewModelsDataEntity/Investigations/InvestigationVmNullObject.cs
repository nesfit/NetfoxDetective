// Copyright (c) 2018 Hana Slamova
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Collections;
using Netfox.Detective.Interfaces.ViewModelsDataEntity.Investigations;
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
using Netfox.NetfoxFrameworkAPI.Interfaces;

namespace Netfox.Detective.ViewModelsDataEntity.Investigations
{
    class InvestigationVmNullObject:IInvestigationVm
    {
        #region Implementation of IInvestigationVm
        public ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper> AvailableSnoopers { get; }
        public IFrameworkController FrameworkController { get; set; }
        public bool IsOpened { get; }
        public ISnooperFactory SnooperFactory { get; }
        public Investigation Investigation { get; }
        public WindsorContainer GlobalWindsorContainer { get; set; }
        public ViewModelVirtualizingIoCObservableCollection<CaptureVm, PmCaptureBase> Captures { get; }
        public ViewModelVirtualizingIoCObservableCollection<ConversationsGroupVm, ConversationsGroup> ConversationsGroups { get; }
        public ViewModelVirtualizingIoCObservableCollection<ExportGroupVm, ExportGroup> ExportGroups { get; }
        public ViewModelVirtualizingIoCObservableCollection<OperationLogVm, OperationLog> OperationLogs { get; }
        public ViewModelVirtualizingIoCObservableCollection<SourceLogVm, SourceLog> SourceLogs { get; }
        public IEnumerable<ExportGroupVm> ExportGroupsFlat { get; }
        public CaptureVm CurrentCapture { get; set; }
        public ConcurrentObservableHashSet<CaptureVm> SelectedCaptureVms { get; }
        public ConversationsGroupVm CurrentConversationsGroupVm { get; set; }
        public ConcurrentObservableHashSet<ConversationsGroupVm> SelectedConversationsGroupVms { get; }
        public ExportGroupVm CurrentExportGroup { get; set; }
        public ConcurrentObservableHashSet<ExportGroupVm> SelectedExportGroupVms { get; }
        public long ToExportTotalSize { get; }
        public ConcurrentObservableCollection<ILxConversation> ToExportConversations { get; }
        public string ExportGroupName { get; set; }
        public bool ShowExportedObjects { get; set; }
        public bool DontUseApplicationTag { get; set; }
        public bool CreateSubGroup { get; set; }
        public bool ExportToRootGroups { get; set; }
        public bool ExportToRootGroupsInver { get; }
        public ICommand CExport { get; }
        public ICommand CAddNewGroup { get; }
        public ICommand CRemoveGroup { get; }
        public RelayCommand<SourceLogVm> CRemoveLog { get; }
        public ICommand CRemoveCapture { get; }
        public ICommand CProtocolsRecognition { get; }
        public Task Initialize() { return Task.CompletedTask; }
        public Task AddCaptureAsync(string filePath) { return Task.CompletedTask; }
        public void AddCaptureToExport(CaptureVm capture) {  }
        public void AddConversationsGroupToExport(ConversationsGroupVm conversationsGroup) { }
        public void AddConversationToExport(ILxConversation conversation) { }
        public Task AddLogFile(string filePath) { return Task.CompletedTask; }
        public void AddNewExportGroup() {  }
        public Task AddSourceLog() { return Task.CompletedTask; }
        public Task CheckCapturesChecksums() { return Task.CompletedTask; }
        public void Close() {  }
        public ExportGroupVm ExportGroupByExportResult(ExportVm selectedExportResult) { return null; }
        public Task Init() { return Task.CompletedTask; }
        public void Open() {  }
        public void ProtocolsRecognition() { }
        public Task RemoveCaptureAsync(PmCaptureBase capture) { return Task.CompletedTask; }
        public Task RemoveCaptureAsync(CaptureVm capture) { return Task.CompletedTask; }
        public void RemoveConversationsGroup(ConversationsGroupVm conversationsGroup) { }
        public void RemoveExportGroup(ExportGroupVm exportGroupVm) { }
        public CrossContainerHierarchyResolver CrossContainerHierarchyResolver { get; set; }
        #endregion
    }
}
