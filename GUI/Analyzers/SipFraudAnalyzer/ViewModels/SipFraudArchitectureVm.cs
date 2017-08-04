// Copyright (c) 2017 Jan Pluskal
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Netfox.AnalyzerSIPFraud.Interfaces;
using Netfox.AnalyzerSIPFraud.Models;
using Netfox.AnalyzerSIPFraud.Services;
using Netfox.Core.Collections;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.Core;
using Netfox.Detective.Models.Conversations;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModels;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using PostSharp.Patterns.Model;

namespace Netfox.AnalyzerSIPFraud.ViewModels
{
    public class SipFraudArchitectureVm : DetectiveInvestigationPaneViewModelBase, IAnalyzerInvestigation, IDataErrorInfo, ILoggable
    {
        private string _backEndUrlString = @"telnet://172.16.0.1:9999/";
        private TimeSpan _captureRemoveTimeout = new TimeSpan(0, 60, 0);
        private int _capturingProgress;

        private ExportService _exportService;
        private bool _isCapturing;
        private bool _isDownloading;
        private bool _isIncidentDetecting;
        private bool _isMonitoring;
        private NemeaSipFraudStatsVm _nemeaSipFraudStatsVm;
        private SipFraudAnalyzerVm _sipFraudAnalyzerVm;
        private SnooperSIP.SnooperSIP _snooperSIP;

        public SipFraudArchitectureVm(WindsorContainer investigationOrAppWindsorContainer) : base(investigationOrAppWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<ISipFraudArchitectureView>());
            //this.PropertyObeserver = new PropertyObserver<SipFraudArchitectureVm>(this).RegisterHandler(p => p.IsSelected, vm => this.Init());
            this.SipFraudAnalyzerVm = new SipFraudAnalyzerVm(investigationOrAppWindsorContainer);
        }

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                var conversationsVm = this.ConversationsVm;
                if(conversationsVm != null) { conversationsVm.IsSelected = value; }
            }
        }

        public PropertyObserver<SipFraudArchitectureVm> PropertyObeserver { get; set; }

        [IgnoreAutoChangeNotification]
        public ICommand CBackEndUrlChange => new RelayCommandAsync(this.Init);

        [IgnoreAutoChangeNotification]
        public ICommand CClean => new RelayCommandAsync(this.Clean);

        public CancellationTokenSource CancellationTokenSource { get; set; }
        public NemeaProxy NemeaProxy { get; private set; }

        public IConversationsVm ConversationsVm { get; }

        public NemeaSipFraudStatsVm NemeaSipFraudStatsVm
        {
            get { return this._nemeaSipFraudStatsVm; }
            set
            {
                this._nemeaSipFraudStatsVm = value;
                this.RaisePropertyChanged();
            }
        }

        public SipFraudAnalyzerVm SipFraudAnalyzerVm
        {
            get { return this._sipFraudAnalyzerVm; }
            set
            {
                this._sipFraudAnalyzerVm = value;
                this.RaisePropertyChanged();
            }
        }

        public ConcurrentObservableCollection<JsonModels.Message> NemeaMessages { get; } = new ConcurrentObservableCollection<JsonModels.Message>();

        public int CapturingProgress
        {
            get { return this._capturingProgress; }
            set
            {
                this._capturingProgress = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsMonitoring
        {
            get { return this._isMonitoring; }
            private set
            {
                this._isMonitoring = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsIncidentDetecting
        {
            get { return this._isIncidentDetecting; }
            private set
            {
                this._isIncidentDetecting = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsCapturing
        {
            get { return this._isCapturing; }
            private set
            {
                this._isCapturing = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsDownloading
        {
            get { return this._isDownloading; }
            private set
            {
                this._isDownloading = value;
                this.RaisePropertyChanged();
            }
        }

        public string BackEndUrlString
        {
            get { return this._backEndUrlString; }
            set
            {
                this._backEndUrlString = value;
                this.OnPropertyChanged();
            }
        }

        [IgnoreAutoChangeNotification]
        public RelayCommand CStopBackend => new RelayCommand(this.StopBackendProcessing);

        public TimeSpan CaptureRemoveTimeout
        {
            get { return this._captureRemoveTimeout; }
            set
            {
                if(value == new TimeSpan()) return;
                this._captureRemoveTimeout = value;
                this.OnPropertyChanged();
            }
        }

        private BlockingCollection<IEnumerable<string>> _incidentPcapFileUries { get; } = new BlockingCollection<IEnumerable<string>>();
        private BlockingCollection<ConversationsGroup> _conversationGroupsForAlert { get; } = new BlockingCollection<ConversationsGroup>();

        private ExportService ExportService
            => this._exportService ?? (this._exportService = this.ApplicationOrInvestigationWindsorContainer.Resolve<ExportService>(this.ApplicationOrInvestigationWindsorContainer));

        private SnooperSIP.SnooperSIP SnooperSIP => this._snooperSIP ?? (this._snooperSIP = this.ApplicationOrInvestigationWindsorContainer.Resolve<SnooperSIP.SnooperSIP>());

        private SSHFileDownloadService SSHFileDownloadService { get; } = new SSHFileDownloadService();

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "SIP Fraud overview";
        #endregion

        public async Task  Init()
        {
            this.StopBackendProcessing();

            //this.PropertyObeserver.UnregisterHandler(p => p.IsSelected);
            this.PropertyObeserver = null;

            try
            {
                this.NemeaProxy = new NemeaProxy();
                await this.NemeaProxy.Connect(new Uri(this.BackEndUrlString));

                //this.SipExports = this.InvestigationOrAppWindsorContainer.Resolve<PersistenceCollection<SnooperExportSIP>>(new
                //{
                //    investigationOrAppWindsorContainer = this.InvestigationOrAppWindsorContainer
                //});

                //this.ApplicationShell.CurrentInvestigationVm.Investigation.Captures.Subscribe((c) => this._conversationGroupsForAlert.Add(c));

                await this.Export(this.CancellationTokenSource);
                await this.ProccessCapturedPcaps(this.CancellationTokenSource);
                await this.UpdateNemeaMessages(this.CancellationTokenSource);
            }
            catch(Exception ex) {
                this.Logger?.Error($"SSHFileDownloadService - connection to NEmeaProxy failed", ex);
            }
        }

        public void StopBackendProcessing()
        {
            this.CancellationTokenSource?.Cancel();
            this.ResetVisualizationToDefaultValues();
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                this.ResetVisualizationToDefaultValues();
            }).ConfigureAwait(false);
            this.CancellationTokenSource = new CancellationTokenSource();
        }

        private async Task Clean()
        {
            this.StopBackendProcessing();
            foreach(var capture in this.ApplicationShell.CurrentInvestigationVm.Captures) {await this.ApplicationShell.CurrentInvestigationVm.RemoveCaptureAsync(capture); }
            this.ApplicationShell.CurrentInvestigationVm.Captures.Clear();
            foreach(var exportGroup in this.ApplicationShell.CurrentInvestigationVm.ExportGroups)
            {
                exportGroup.ExportGroup.Exports.Clear();
                exportGroup.Calls.Clear();
                exportGroup.Emails.Clear();
                exportGroup.ChatMessages.Clear();
            }
            this.SipFraudAnalyzerVm.Init();
        }

        private List<FileInfo> DownloadPcaps(IEnumerable<Uri> pcapFileUris) { return pcapFileUris.SelectMany(pfu => this.SSHFileDownloadService.Download(pfu)).ToList(); }

        private async Task Export(CancellationTokenSource cancelationToken)
        {
            await Task.Run(async () =>
            {
                try
                {
                    while(!this._conversationGroupsForAlert.IsCompleted)
                    {
                        try
                        {
                            if(cancelationToken.IsCancellationRequested) return;

                            var conversationsGroup = this._conversationGroupsForAlert.Take(cancelationToken.Token);
                            if(cancelationToken.IsCancellationRequested) return;
                            //Task.Delay(2000);
                            var syncManualResetEvent = new ManualResetEvent(false);
                            DispatcherHelper.CheckBeginInvokeOnUI(() => { syncManualResetEvent.Set(); });
                            syncManualResetEvent.WaitOne();
                            if(cancelationToken.IsCancellationRequested) return;
                            if(conversationsGroup == null) continue;
                            await this.ExportService.Export(conversationsGroup.L7Conversations, new[]
                            {
                                this.SnooperSIP
                            }, conversationsGroup.Name, true, true, false);

                        }
                        catch(OperationCanceledException ex) {
                            this.Logger?.Error($"{nameof(this.SipFraudAnalyzerVm)} failed to process pcap", ex);
                        }
                    }
                }
                catch(OperationCanceledException ex)
                {
                    this.Logger?.Error($"{nameof(this.SipFraudAnalyzerVm)} failed to process pcap", ex);
                }
            }).ConfigureAwait(false);
        }

        private async Task ProccessCapturedPcaps(CancellationTokenSource cancelationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    while(!this._incidentPcapFileUries.IsCompleted)
                    {
                        if(cancelationToken.IsCancellationRequested) return;

                        var fileUris = this._incidentPcapFileUries.Take(cancelationToken.Token).Select(fu => new Uri(fu));
                        if(cancelationToken.IsCancellationRequested) return;
                        var pcapFileInfos = this.DownloadPcaps(fileUris);
                        if(cancelationToken.IsCancellationRequested) return;
                        var pmCaptures = new List<PmCaptureBase>();
                        foreach(var pcapFileInfo in pcapFileInfos)
                        {
                            if(!pcapFileInfo.Exists)
                            {
                                this.Logger?.Error($"Nemea proxy Capture file {pcapFileInfo.Name} does not exist.");
                                continue;
                            }
                            var syncManualResetEvent = new ManualResetEvent(false);
                            DispatcherHelper.CheckBeginInvokeOnUI(async () =>
                            {
                                await this.ApplicationShell.CurrentInvestigationVm.AddCaptureAsync(pcapFileInfo.FullName);
                                Debugger.Break(); //TODO obtain added capture from DB by other means
                                //pmCaptures.Add(addCaptureTask.Result as PmCaptureBase);
                                syncManualResetEvent.Set();
                            });
                            syncManualResetEvent.WaitOne();
                        }
                        if(cancelationToken.IsCancellationRequested) return;
                        var conversationGroupForAlert = this.ApplicationShell.CurrentInvestigationVm.CreateConversationGroup(pmCaptures.SelectMany(pmCapture =>
                        {
                            IEnumerable<ILxConversation> concatenatedConversations = new List<ILxConversation>();
                            if(pmCapture?.L3Conversations != null) concatenatedConversations = concatenatedConversations.Concat(pmCapture.L3Conversations);
                            if(pmCapture?.L4Conversations != null) concatenatedConversations = concatenatedConversations.Concat(pmCapture.L4Conversations);
                            if(pmCapture?.L7Conversations != null) concatenatedConversations = concatenatedConversations.Concat(pmCapture.L7Conversations);
                            return concatenatedConversations;
                        }));


                        if(cancelationToken.IsCancellationRequested) return;
                        this._conversationGroupsForAlert.Add(conversationGroupForAlert);
                        var capture = pmCaptures.FirstOrDefault();
                        if(capture != null) conversationGroupForAlert.Name = capture.FileInfo.Name;

                        Task.Run(async () =>
                        {
                            await Task.Delay(this.CaptureRemoveTimeout);
                            if(cancelationToken.IsCancellationRequested) return;
                            var investigationVm = this.ApplicationOrInvestigationWindsorContainer.Resolve<InvestigationVm>();
                            foreach(var pmCapture in pmCaptures) { await investigationVm.RemoveCaptureAsync(pmCapture); }
                        }).ConfigureAwait(false);
                    }
                }
                catch(OperationCanceledException ex)
                {
                    this.Logger?.Error($"{nameof(this.SipFraudAnalyzerVm)} failed to process pcap", ex);
                }
            }).ConfigureAwait(false);
        }

        private void ResetVisualizationToDefaultValues()
        {
            this.IsCapturing = false;
            this.IsDownloading = false;
            this.IsIncidentDetecting = false;
            this.IsMonitoring = false;
            this.CapturingProgress = 0;
        }

        private async Task UpdateNemeaMessage()
        {
            try
            {
                var msg = await this.NemeaProxy.GetMessageAsync();
                this.NemeaMessages.Add(msg);

                this.IsCapturing = false;
                this.IsDownloading = false;
                this.IsIncidentDetecting = false;
                this.IsMonitoring = false;

                if(msg.Stats != null) { this.NemeaSipFraudStatsVm = new NemeaSipFraudStatsVm(msg.Stats); }
                if(msg.Files != null) { this._incidentPcapFileUries.Add(msg.Files); }
                if(msg.Type == "incident")
                {
                    if(!this.NemeaSipFraudStatsVm.SuspiciousIPs.Contains(msg.SuspiciousHost, StringComparer.InvariantCultureIgnoreCase)) this.NemeaSipFraudStatsVm.SuspiciousIPs.Add(msg.SuspiciousHost);
                    this.IsIncidentDetecting = true;
                }
                if(msg.Type == "visualisation") { this.IsDownloading = true; }
                if(msg.Type == "evidence-capture")
                {
                    this.IsCapturing = true;
                    this.CapturingProgress = msg.Progress;
                }
                if(msg.Type == "monitoring") { this.IsMonitoring = true; }
            }
            catch(Exception ex)
            {
                this.Logger?.Error($"Nemea proxy update failed", ex);
                this.StopBackendProcessing();
            }
        }

        private async Task UpdateNemeaMessages(CancellationTokenSource cancelationToken)
        {
            while(true)
            {
                if(cancelationToken.IsCancellationRequested) return;
                await this.UpdateNemeaMessage();
            }
        }

        #region Implementation of IDataErrorInfo
        /// <summary>Gets the error message for the property with the given name.</summary>
        /// <returns>The error message for the property. The default is an empty string ("").</returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public string this[string columnName] => this.Validate(columnName);

        /// <summary>Gets an error message indicating what is wrong with this object.</summary>
        /// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
        public string Error { get; } = "Error...";

        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            var validationMessage = string.Empty;
            switch(propertyName)
            {
                case nameof(this.BackEndUrlString):
                    validationMessage = Uri.IsWellFormedUriString(this.BackEndUrlString, UriKind.RelativeOrAbsolute)? "" : "URL is invalid - e.g., telnet://172.16.0.1:9999/";
                    break;
            }

            return validationMessage;
        }
        #endregion
    }
}