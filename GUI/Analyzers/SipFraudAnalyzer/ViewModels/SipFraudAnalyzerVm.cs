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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.AnalyzerSIPFraud.Models;
using Netfox.Core.Collections;
using Netfox.Core.Properties;
using Netfox.Detective.Properties;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperSIP.Models;
using Netfox.SnooperSIP.Models.Message;
using Telerik.Windows.Data;
using VDS.Common.Tries;
using ConnectionModel = Netfox.AnalyzerSIPFraud.Models.ConnectionModel;

namespace Netfox.AnalyzerSIPFraud.ViewModels
{
    public class SipFraudAnalyzerVm : INotifyPropertyChanged
    {
        private QueryableCollectionView _sipMessages;
        private ClusterNodeModel[] _nodes = new ClusterNodeModel[0];
        private bool _isActive;
        private int _sipMessagesCount;
        public WindsorContainer InvestigationOrAppWindsorContainer { get; set; }
        public StringTrie<SIPMsg> PrefixTrie { get; private set; }

        public ClusterNodeModel[] Nodes
        {
            get { return this._nodes; }
            private set
            {
                if(Equals(value, this._nodes)) return;
                this._nodes = value;
                this.OnPropertyChanged();
            }
        }

   

        private ConcurrentObservableCollection<ClusterNodeModel> PrepareNodes { get;  set; } = new ConcurrentObservableCollection<ClusterNodeModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="investigationOrAppWindsorContainer"></param>
        public SipFraudAnalyzerVm(WindsorContainer investigationOrAppWindsorContainer)
        {
            this.InvestigationOrAppWindsorContainer = investigationOrAppWindsorContainer;

            this.Init();
        }

        private object RecalculateNodesLock { get; } = new object();

        public void RecalculateNodes()
        {
            lock(this.RecalculateNodesLock) {
                this.RecalculateNodes(null, this.PrefixTrie.Root, 4);
                this.Nodes = this.PrepareNodes.ToArray();
                this.PrepareNodes = new ConcurrentObservableCollection<ClusterNodeModel>();
            }
        }

        private void RecalculateNodes(ClusterNodeModel parrent, ITrieNode<char, SIPMsg> current, int depth)
        {
            if(depth<=0) return;
            var node = new ClusterNodeModel();
            while (true)
            {
                node.Label += current.KeyBit;
                if (current.Count == 1) { current = current.Children.First(); }
                else
                { break; }
            }
            node.Label = ReverseString(node.Label);
            if (parrent != null)
            {
                node.Connections.Add(new ConnectionModel
                {
                    Target = parrent
                });
                parrent.Connections.Add(new ConnectionModel
                {
                    Target = node
                });
            }
            this.PrepareNodes.Add(node);
            foreach (var descendant in current.Children) { this.RecalculateNodes(node, descendant, depth-1); }
        }

        public void Init()
        {
            this.ExportSubscription?.Dispose();
            this.ExportRemovedSubscription?.Dispose();
            this.IsActiveResetEvent.Set();
            this.SipMessagesSource = new ConcurrentObservableCollection<SIPMsg>();
            this.SipMessages = new QueryableCollectionView(this.SipMessagesSource);
            this.PrefixTrie = new StringTrie<SIPMsg>();
            this.Nodes = new ClusterNodeModel[0];

           
            var investigationVm = this.InvestigationOrAppWindsorContainer.Resolve<InvestigationVm>();

            this.ExportSubscription = investigationVm.SelectedExportGroupVms.Subscribe(exportGroup =>
            {
                Task.Run(() =>
                {
                   // this.IsActiveResetEvent.WaitOne();
                    this.ProcessExportGroup(exportGroup, CollectionChangeAction.Add);
                    this.RecalculateNodes();
                }).ConfigureAwait(false);
            });

            this.ExportRemovedSubscription = investigationVm.SelectedExportGroupVms.RemovedItems.Subscribe(exportGroup =>
            {
                Task.Run(() =>
                {
                  //  this.IsActiveResetEvent.WaitOne();
                    this.ProcessExportGroup(exportGroup, CollectionChangeAction.Remove);
                    this.RecalculateNodes();
                }).ConfigureAwait(false);
            });
        }

        public ConcurrentObservableCollection<SIPMsg> SipMessagesSource { get; set; }

        public IDisposable ExportRemovedSubscription { get; set; }

        private void ProcessExportGroup(ExportGroupVm exportGroup, CollectionChangeAction action) {
            foreach(var exportGroupVm in exportGroup.ExportGroups) {
                this.ProcessExportGroup(exportGroupVm, action);
            }
            this.ProcessExports(exportGroup, action);
        }

        private void ProcessExports(ExportGroupVm exportGroup, CollectionChangeAction action)
        {
            foreach(var snooperExport in  exportGroup.ExportGroup.Exports)
            {
                var exports = (snooperExport as SnooperExportBase)?.ExportObjects;
                if(exports == null) continue;

                foreach (var snooperExportedObjectBase in exports) {
                    var sipMessages = (snooperExportedObjectBase as SIPEvent)?.SipMessages;
                    if (sipMessages == null)
                    {
                        continue;
                    }
                    foreach (var sipMsg in sipMessages)
                    {
                        switch(action)
                        {
                            case CollectionChangeAction.Add:
                            {
                                    DispatcherHelper.CheckBeginInvokeOnUI(() => { this.SipMessages.AddNew(sipMsg);});
                                    if (sipMsg.RequestLine?.Method == "INVITE")
                                    {
                                        var to = sipMsg.Headers.To.Substring(sipMsg.Headers.To.IndexOf("sip:") + 4).TrimEnd('>');
                                        var reverseTo = ReverseString(to);
                                        this.PrefixTrie.Add(reverseTo, sipMsg);
                                    }
                                }
                                break;
                            case CollectionChangeAction.Remove:
                            {
                                    DispatcherHelper.CheckBeginInvokeOnUI(() =>{this.SipMessages.Remove(sipMsg);});
                                    if (sipMsg.RequestLine?.Method == "INVITE")
                                    {
                                        var to = sipMsg.Headers.To.Substring(sipMsg.Headers.To.IndexOf("sip:") + 4).TrimEnd('>');
                                        var reverseTo = ReverseString(to);
                                        if(this.PrefixTrie.Contains(reverseTo,sipMsg))
                                            this.PrefixTrie.Remove(reverseTo);
                                    }
                                }
                                break;
                        }

                       
                    }
                }
            }
        }

        private ManualResetEvent IsActiveResetEvent { get; } = new ManualResetEvent(false);

        private void Activate() { }

        public IDisposable ExportSubscription { get; set; }

        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        public QueryableCollectionView SipMessages
        {
            get { return this._sipMessages; }
            set
            {
                this._sipMessages = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.SipMessagesCount));
                this._sipMessages.CollectionChanged += (sender, args) => { this.SipMessagesCount = this.SipMessages.Count; };
            }
        }

        public int SipMessagesCount
        {
            get { return this._sipMessagesCount; }
            set
            {
                if(value == this._sipMessagesCount) return;
                this._sipMessagesCount = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get { return this._isActive; }
            set
            {
                if(this._isActive == value) return;
                this._isActive = value;
                if(value)
                {
                    this.IsActiveResetEvent.Set();
                    if(this.ExportSubscription == null) this.Init();
                }
                else
                { this.Deactivate(); }
            }
        }

        private void Deactivate() { this.IsActiveResetEvent.Reset(); }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}