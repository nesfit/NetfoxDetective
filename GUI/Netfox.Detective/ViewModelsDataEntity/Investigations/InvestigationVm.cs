// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.MicroKernel;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Netfox.Core.Attributes;
using Netfox.Core.Collections;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Messages.Base;
using Netfox.Core.Messages.Exports;
using Netfox.Core.Messages.Views;
using Netfox.Core.Models;
using Netfox.Detective.Models;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Models.Conversations;
using Netfox.Detective.Models.Exports;
using Netfox.Detective.Models.SourceLog;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModels.Exports;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
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
    public class InvestigationVm : ConversationsVm<Investigation>, IInitializable
    {
        private readonly object _initLock = new object();
        private readonly object _updateApplicationAnalyzersLock = new object();
        private ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper> _availableSnoopers;
        private bool _createSubGroup;
        private CaptureVm _currentCapture;
        private ConversationsGroupVm _currentConversationsGroupVm;
        private ExportGroupVm _currentExportGroup;
        private bool _exportToRootGroups;

        public InvestigationVm(IWindsorContainer applicationWindsorContainer, ISnooperFactory snooperFactory, IFrameworkControllerFactory frameworkControllerFactory, Investigation model, ExportService exportService) : base(applicationWindsorContainer, model,exportService)
        {
            this.SnooperFactory = snooperFactory;
            this.Investigation = model;
            this.FrameworkController = frameworkControllerFactory.Create();
            this.ApplicationOrInvestigationWindsorContainer = this.InvestigationWindsorContainer;

            this.SourceLogs = new ViewModelVirtualizingIoCObservableCollection<SourceLogVm, SourceLog>(this.Investigation.SourceLogs, this.InvestigationWindsorContainer);

            this.OperationLogs = new ViewModelVirtualizingIoCObservableCollection<OperationLogVm, OperationLog>(this.Investigation.OperationLogs, this.InvestigationWindsorContainer);
            this.Captures = new ViewModelVirtualizingIoCObservableCollection<CaptureVm, PmCaptureBase>(this.Investigation.Captures, this.InvestigationWindsorContainer);

            this.ConversationsGroups = new ViewModelVirtualizingIoCObservableCollection<ConversationsGroupVm, ConversationsGroup>(this.Investigation.ConversationsGroups,
                this.InvestigationWindsorContainer);
            this.ConversationsGroups.CollectionChanged += this.ConversationsGroups_CollectionChanged;

            this.ExportGroups = new ViewModelVirtualizingIoCObservableCollection<ExportGroupVm, ExportGroup>(this.Investigation.ExportsGroups, this.InvestigationWindsorContainer);

            this.ExportGroupName = "Exports - " + DateTime.Now;
            this.CreateSubGroup = true;
        }

        public override ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper> AvailableSnoopers
        {
            get
            {
                if(this._availableSnoopers != null) { return this._availableSnoopers; }
                try
                {
                    this._availableSnoopers = new ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper>(this.SnooperFactory.AvailableSnoopers,
                        this.InvestigationWindsorContainer);
                }
                catch(Exception ex) {
                    this.Logger?.Error("Prototype snooper instantiation have failed", ex);
                }
                return this._availableSnoopers;
            }
        }

        public IFrameworkController FrameworkController { get; set; }
        public bool IsOpened { get; private set; }
        public ISnooperFactory SnooperFactory { get; }
        public Investigation Investigation { get;  }
        public WindsorContainer GlobalWindsorContainer { get; set; }

        public ViewModelVirtualizingIoCObservableCollection<CaptureVm, PmCaptureBase> Captures { get; }

        public ViewModelVirtualizingIoCObservableCollection<ConversationsGroupVm, ConversationsGroup> ConversationsGroups { get; }

        public ViewModelVirtualizingIoCObservableCollection<ExportGroupVm, ExportGroup> ExportGroups { get; }

        public ViewModelVirtualizingIoCObservableCollection<OperationLogVm, OperationLog> OperationLogs { get; private set; }
        public ViewModelVirtualizingIoCObservableCollection<SourceLogVm, SourceLog> SourceLogs { get; }

        public IEnumerable<ExportGroupVm> ExportGroupsFlat => this.ExportGroupVms(this.ExportGroups);

        public CaptureVm CurrentCapture
        {
            get { return this._currentCapture; }
            set
            {
                if(this._currentCapture == value) { return; }
                if(value != null && !this.Captures.Contains(value)) { return; }
                this._currentCapture = value;
                var currentCapture = this.CurrentCapture;
                if(currentCapture != null) currentCapture.CurrentConversation = null;
                this.OnPropertyChanged();

                Task.Factory.StartNew(() =>
                {
                    if(this._currentCapture == null) { return; }
                    CaptureMessage.SendCaptureMessage(this._currentCapture, CaptureMessage.MessageType.CurrentCaptureChanged, false);
                });
            }
        }

        public ConcurrentObservableHashSet<CaptureVm> SelectedCaptureVms { get; } = new ConcurrentObservableHashSet<CaptureVm>();

        public ConversationsGroupVm CurrentConversationsGroupVm
        {
            get { return this._currentConversationsGroupVm; }
            set
            {
                if(this._currentConversationsGroupVm == value) { return; }
                this._currentConversationsGroupVm = value;
                var currentConversationsGroupVm = this.CurrentConversationsGroupVm;
                if(currentConversationsGroupVm != null) currentConversationsGroupVm.CurrentConversation = null;
                this.OnPropertyChanged();

                Task.Run(async () =>
                {
                    if(this._currentConversationsGroupVm == null) { return; }

                    await this._currentConversationsGroupVm.Init();
                    ConversationsGroupMessage.SendConversationsGroupMessage(value, ConversationsGroupMessage.MessageType.GroupVmSelected);
                });
            }
        }

        public ConcurrentObservableHashSet<ConversationsGroupVm> SelectedConversationsGroupVms { get; } = new ConcurrentObservableHashSet<ConversationsGroupVm>();

        public ExportGroupVm CurrentExportGroup
        {
            get { return this._currentExportGroup; }
            set
            {
                this._currentExportGroup = value;
                this.OnPropertyChanged();

                Task.Run(async () =>
                {
                    if(this._currentExportGroup == null) { return; }

                    await this._currentExportGroup.Init();

                    ExportGroupMessage.SendExportGroupMessage(this._currentExportGroup, ExportGroupMessage.MessageType.CurrentExportGroupChanged);
                });
            }
        }

        public ConcurrentObservableHashSet<ExportGroupVm> SelectedExportGroupVms { get; } = new ConcurrentObservableHashSet<ExportGroupVm>();
        public long ToExportTotalSize => 0;

        public ConcurrentObservableCollection<ILxConversation> ToExportConversations => new ConcurrentObservableCollection<ILxConversation>();
        public string ExportGroupName { get; set; }

        public bool ShowExportedObjects { get; set; }

        public bool DontUseApplicationTag { get; set; }

        public bool CreateSubGroup
        {
            get { return this._createSubGroup; }
            set
            {
                this._createSubGroup = value;
                this.OnPropertyChanged();
            }
        }

        public bool ExportToRootGroups
        {
            get { return this._exportToRootGroups; }
            set
            {
                this._exportToRootGroups = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ExportToRootGroupsInver));
            }
        }

        public bool ExportToRootGroupsInver => !this.ExportToRootGroups;

        public ICommand CExport => null; //todo new RelayCommand(()=> this.Export(this.ToExportConversations));
        public ICommand CAddNewGroup => new RelayCommand(this.AddNewExportGroup);

        public ICommand CRemoveGroup => new RelayCommand(this.RemoveGroup, this.CanRemoveGroup);

        public RelayCommand<SourceLogVm> CRemoveLog
        {
            get { return new RelayCommand<SourceLogVm>(log => this.SourceLogs.Remove(log.SourceLog), log => true); }
        }

        public ICommand CRemoveCapture => new RelayCommandAsync(this.RemoveCaptureExecuteAsync, this.CanRemoveCaptureExecute);

        public ICommand CProtocolsRecognition => new RelayCommand(this.CProtocolsRecognitionxecute, this.CanCProtocolsRecognitionExecute);

        public async Task Initialize()
        {
            await Task.WhenAll(Task.Run(() =>
            {
                Messenger.Default.Register<ConversationMessage>(this, this.ConversationMessageHandler);
                Messenger.Default.Register<CaptureMessage>(this, this.CaptureMessageHandler);
                Messenger.Default.Register<WorkspaceMessage>(this, this.WorkspaceMessageHandler);
            }));
            if(this.Investigation.InvestigationInfo.IsInMemory)
            {
                await this.AddExistingPcaps();
                await this.AddExistingLogs();
            }
            await this.UpdateInvestigationAnalyzers();
        }

        private async Task AddExistingPcaps()
        {
            foreach(var pcap in this.Investigation.InvestigationInfo.SourceCaptureDirectoryInfo.GetDirectories().SelectMany(d=>d.EnumerateFiles())) {
                await this.AddCaptureFrameworkControllerAsync(pcap);
            }
        }
        private async Task AddExistingLogs()
        {
            foreach (var log in this.Investigation.InvestigationInfo.SourceLogsDirectoryInfo.GetDirectories().SelectMany(d => d.EnumerateFiles()))
            {
                await this.AddLogFileTaskAsync(log);
            }
        }

        public async Task AddCaptureAsync(string filePath)
        {
            var captureFilePathInfo = new FileInfo(filePath);
            if(!captureFilePathInfo.Exists) { return; }

            var fileName = captureFilePathInfo.Name;

            var targetDirectoryInfo = this.Investigation.InvestigationInfo.SourceCaptureDirectoryInfo.CreateSubdirectory(fileName + "-" + Guid.NewGuid());
            targetDirectoryInfo.Create();
            var coppiedFileInfo = new FileInfo(Path.Combine(targetDirectoryInfo.FullName, captureFilePathInfo.Name));

            await this.CopyCaptureTaskAsync(captureFilePathInfo, coppiedFileInfo);
            await this.AddCaptureFrameworkControllerAsync(coppiedFileInfo);
        }

        public void AddCaptureToExport(CaptureVm capture)
        {
            if(capture == null) { return; }
            Task.Factory.StartNew(() => this.ToExportConversations.AddItems(capture.Capture.L3Conversations));
        }

        public void AddConversationsGroupToExport(ConversationsGroupVm conversationsGroup)
        {
            if(conversationsGroup == null) { return; }

            Task.Factory.StartNew(() => this.ToExportConversations.AddItems(conversationsGroup.ConversationsGroup.Conversations));
        }

        public void AddConversationToExport(ILxConversation conversation) => this.ToExportConversations.Add(conversation);

        public async Task AddLogFile(string filePath)
        {
            var logFilePathInfo = new FileInfo(filePath);
            if(!logFilePathInfo.Exists) { return; }

            var fileName = Path.GetFileName(filePath);
            var targetDirectoryInfo = this.Investigation.InvestigationInfo.SourceLogsDirectoryInfo.CreateSubdirectory(fileName + "-" + Guid.NewGuid());
            targetDirectoryInfo.Create();
            var coppiedFileInfo = new FileInfo(Path.Combine(targetDirectoryInfo.FullName, logFilePathInfo.Name));
            await this.CopyLogTaskAsync(logFilePathInfo, coppiedFileInfo);
            this.Logger?.Info($"Adding log {fileName}");
            await this.AddLogFileTaskAsync(coppiedFileInfo);
        }

        public void AddNewExportGroup()
        {
            //this.Investigation.AddNewExportGroup("New group");
        }

        public async Task AddSourceLog()
        {
            if(this.SystemServices == null) { return; }
            var logFiles = this.SystemServices.OpenFilesDialog("", ".wdat", "Warcraft log (*.wdat)|*.wdat;|All Files (*.*)|*.*");
            foreach(var logFile in logFiles) { await this.AddLogFile(logFile); }
        }

        [AsyncTask(Title = nameof(CheckCapturesChecksums), Description = "Checking integrity of captures.")]
        public async Task CheckCapturesChecksums()
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(this.Captures, async captureVm =>
                {
                    try
                    {
                        var isCorrect = await captureVm.Capture.IsChecksumCorrectAsync;
                        if(!isCorrect) {
                            this.Logger?.Error($"Capture checksum is not correct {Path.GetFileName(captureVm.Capture.FilePath)}");
                        }
                    }
                    catch(Exception ex)
                    {
                        this.Logger?.Error($"Unable to check capture {captureVm.Capture.FileInfo.FullName} checksum!",ex);
                    }
                });
            });
        }

        public void Close()
        {
            foreach(var captureVm in this.Captures)
            {
                //todo captureVm.Capture.FwControllerContext.FwController.RemoveCaptureAsync(captureVm.EncapsulatedData.ProcessingContext);
            }
            this.CurrentCapture = null;
            this.CurrentExportGroup = null;
            this.CurrentConversationsGroupVm = null;
            this.IsOpened = false;
        }

        public ExportGroupVm ExportGroupByExportResult(ExportVm selectedExportResult) { throw new NotImplementedException(); }

        public override async Task Init()
        {
            if(this.Initialized) { return; }

            lock(this._initLock)
            {
                if(this.Initialized) { return; }
                this.Initialized = true;
            }
            await this.CheckCapturesChecksums();
        }

        public void Open()
        {
            if(this.IsOpened) { return; }
            this.IsOpened = true;
            InvestigationMessage.SendInvestigationMessage(this, InvestigationMessage.MessageType.CurrentInvestigationChanged);
        }

        public void ProtocolsRecognition()
        {
            //todo
            //ShowProtocolsRecognitionMessage.SendShowProtocolsRecognitionMessage(new ProtocolsRecognitionContext
            //{
            //    Controller = this.Investigation.FrameworkControllerContext,
            //    RecognitionScope = ProtocolsRecognitionContext.RecognitionScopeType.Investigation,
            //    InvestigationScope = this.Investigation
            //});
        }


        [AsyncTask(Title = nameof(RemoveCaptureAsync), Description = "Copying capture file.")]
        public async Task RemoveCaptureAsync(PmCaptureBase capture)
        {
            await Task.Run(() =>
            {
                if(capture == null) { return; }
                try {
                    this.Logger?.Info("InvestigationVm delete not implemented.");
                }
                catch(Exception ex) {
                    this.Logger?.Error("Remove capture failed", ex);
                }
            });
        }

        public async Task RemoveCaptureAsync(CaptureVm capture) { await this.RemoveCaptureAsync(capture?.EncapsulatedModel as PmCaptureBase); }

        public void RemoveConversationsGroup(ConversationsGroupVm conversationsGroup)
        {
            if(conversationsGroup != null) { this.Investigation.ConversationsGroups.Remove(conversationsGroup.ConversationsGroup); }
        }

        public void RemoveExportGroup(ExportGroupVm exportGroupVm)
        {
            if(exportGroupVm != null)
            {
                //todo this.Investigation.RemoveExportGroup(exportGroupVm.ExportGroup);
            }
        }

        protected override async Task ActualizeExportSet(List<object> parameters)
        {
            await base.ActualizeExportSet(parameters);
            var values = parameters;
            var showExportedObjectsDuringExportation = (bool) values[1];
            await this.ExportService.Export(this.Investigation.SourceLogs, this.CurrentInvestigationSelectedExporters(), null, true, showExportedObjectsDuringExportation);
        }


        internal void Save()
        {
            var serializer = new DataContractSerializer(typeof(InvestigationInfo));
            try
            {
                using(var writer = new FileStream(this.Investigation.InvestigationInfo.InvestigationFileInfo.FullName, FileMode.Create)) {
                    serializer.WriteObject(writer, this.Investigation.InvestigationInfo);
                }
            }
            catch(IOException ex) {
                this.Logger?.Error("Saving investigation failed",ex);
            }
        }

        [AsyncTask(Title = nameof(AddCaptureFrameworkControllerAsync), Description = "Adding capture file.")]
        private async Task AddCaptureFrameworkControllerAsync(FileInfo captureFilePathInfo)
        {
            await Task.Run(() =>
            {
                try
                {
                    this.Logger?.Info($"Adding capture {captureFilePathInfo.Name}");
                    this.FrameworkController.ProcessCapture(captureFilePathInfo);
                    this.Logger?.Info($"Capture file added {captureFilePathInfo.Name}");
                }
                catch(Exception ex) {
                    this.Logger?.Error($"Adding capture file {captureFilePathInfo?.Name} failed",ex);
                }
            });
        }

        [AsyncTask(Title = nameof(CopyLogTaskAsync), Description = "Copying log file.")]
        private async Task AddLogFileTaskAsync(FileInfo logFilePathInfo)
        {
            await Task.Run(() =>
            {
                try
                {
                    if(logFilePathInfo == null) { return; }
                    this.Logger?.Info($"Adding log {logFilePathInfo.Name}");
                   var log = new SourceLog(logFilePathInfo);
                    this.SourceLogs.Add(log);
                    this.Logger?.Info($"Log added {logFilePathInfo.Name}");
                }
                catch(Exception ex) {
                    this.Logger?.Error($"Adding log file {logFilePathInfo?.Name} failed",ex);
                }
            });
        }

        private bool CanCProtocolsRecognitionExecute() { return true; }

        private bool CanRemoveCaptureExecute() => this.CurrentCapture != null;

        private bool CanRemoveGroup() { return this.CurrentExportGroup != null; }

        private void CaptureMessageHandler(CaptureMessage message)
        {
            switch(message.Type)
            {
                case CaptureMessage.MessageType.CurrentCaptureChangedById:
                    Task.Factory.StartNew(() =>
                    {
                        //todo this.SetCurrentCaptureById(message.CaptureId);

                        if(message.BringToFront) { BringToFrontMessage.SendBringToFrontMessage("CaptureView"); }
                    });
                    break;
                case CaptureMessage.MessageType.AddCaptureToExport:
                    Task.Factory.StartNew(() =>
                    {
                        var capture = message.CaptureVm as CaptureVm;

                        this.AddCaptureToExport(capture);

                        this.NavigationService.Show(typeof(SelectiveExportVm));
                    });
                    break;
            }
        }

        private void ConversationMessageHandler(ConversationMessage conversationMessage)
        {
            if(conversationMessage == null) { return; }
            switch(conversationMessage.Type)
            {
                case ConversationMessage.MessageType.CurrentConversationChanged:
                    break;
                case ConversationMessage.MessageType.AddConversationToExport:
                    Task.Factory.StartNew(() =>
                    {
                        var cvm = conversationMessage.ConversationVm as ConversationVm;
                        if(cvm != null) { this.ToExportConversations.Add(cvm.Conversation); }
                    });
                    this.NavigationService.Show(typeof(SelectiveExportVm));
                    break;
            }
        }

        private void ConversationsGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach(var cgVm in this.ConversationsGroups) { cgVm.Captures = this.Captures; }
                return;
            }

            if(e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach(var newItem in e.NewItems)
                {
                    var cgVm = newItem as ConversationsGroupVm;
                    if(cgVm != null) { cgVm.Captures = this.Captures; }
                }
            }
        }

        [AsyncTask(Title = nameof(CopyCaptureTaskAsync), Description = "Copying capture file.")]
        private async Task CopyCaptureTaskAsync(FileSystemInfo sourceFileInfo, FileSystemInfo destinationFileInfo)
        {
            await Task.Run(() =>
            {
                this.Logger?.Info($"Copying capture file {destinationFileInfo.Name}");
                File.Copy(sourceFileInfo.FullName, destinationFileInfo.FullName);
                this.Logger?.Info($"Copying capture file {destinationFileInfo.Name} has finished");
            });
        }

        [AsyncTask(Title = nameof(CopyLogTaskAsync), Description = "Copying log file.")]
        private async Task CopyLogTaskAsync(FileSystemInfo sourceFileInfo, FileSystemInfo destinationFileInfo)
        {
            await Task.Run(() =>
            {
                this.Logger?.Info($"Copying log file {destinationFileInfo.Name}");
                File.Copy(sourceFileInfo.FullName, destinationFileInfo.FullName);
                this.Logger?.Info($"Copying log file {destinationFileInfo.Name} has finished");
            });
        }

        private void CProtocolsRecognitionxecute() { this.ProtocolsRecognition(); }

        private IEnumerable<ExportGroupVm> ExportGroupVms(IEnumerable<ExportGroupVm> groups)
        {
            foreach(var exportGroupVm in groups)
            {
                yield return exportGroupVm;

                var subGroups = this.ExportGroupVms(exportGroupVm.ExportGroups);
                foreach(var subGroup in subGroups) { yield return subGroup; }
            }
        }



        private async Task RemoveCaptureExecuteAsync()
        {
            if(this.CurrentCapture != null) { await this.RemoveCaptureAsync(this.CurrentCapture); }
        }

        private void RemoveGroup()
        {
            if(this.CurrentExportGroup != null) { this.RemoveExportGroup(this.CurrentExportGroup); }
        }

        public CrossContainerHierarchyResolver CrossContainerHierarchyResolver { get; set; }
        private async Task UpdateInvestigationAnalyzers()
        {
            await Task.Run(() =>
            {
                lock(this._updateApplicationAnalyzersLock)
                {
                    foreach(var availableAnalyzerType in this.CrossContainerHierarchyResolver.AvailableAnalyzerTypes.Where(t => typeof(IAnalyzerInvestigation).IsAssignableFrom(t)))
                    {
                        try
                        {
                            if(this.ApplicationShell.AvailableAnalyzers.Any(a => a.GetType() == availableAnalyzerType)) return;
                            this.ApplicationShell.AvailableAnalyzers.Add(this.InvestigationWindsorContainer.Resolve(availableAnalyzerType, new
                            {
                                investigationOrAppWindsorContainer = this.InvestigationWindsorContainer
                            }) as IAnalyzerInvestigation);
                        }
                        catch(ComponentResolutionException ex) {
                            this.Logger?.Error($"Initialization of Analyzer {availableAnalyzerType} failed", ex);
                        }
                    }
                }
            });
        }

        private void WorkspaceMessageHandler(WorkspaceMessage workspaceMessage)
        {
            if(workspaceMessage == null) { return; }
            switch(workspaceMessage.MessageType)
            {
                case WorkspaceMessage.Type.Created:
                    break;
                case WorkspaceMessage.Type.ToCreate:
                    break;
                case WorkspaceMessage.Type.Opened:
                    break;
                case WorkspaceMessage.Type.Closed:
                    this.Close();
                    break;
                case WorkspaceMessage.Type.Opening:
                    break;
            }
        }
    }
}