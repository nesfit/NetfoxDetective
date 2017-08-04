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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Netfox.Core.Collections;
using Netfox.Core.Database;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Messages.Base;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Models.Conversations;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModels.Frame;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Frames;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;
using Telerik.Windows.Controls.ChartView;

namespace Netfox.Detective.ViewModelsDataEntity.ConversationsCollections
{
    public abstract class MockConversationsVm : ConversationsVm<PmCaptureBase>
    {
        protected MockConversationsVm(WindsorContainer applicationWindsorContainer, PmCaptureBase model, ExportService exportService) : base(applicationWindsorContainer, model,exportService) { }
    }

    public abstract class ConversationsVm<TType> : DetectiveDataEntityViewModelBase, IDataEntityVm, IConversationsVm where TType : IConversationsModel
    {
        private readonly object _getFilteredL7ConversationsArrayLock = new object();
        private readonly object _isSelectedLock = new object();
        private ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper> _availableSnoopers;
        private bool _canBroadcastSelectedConversationChange = true;

        private NotifyTaskCompletion<ConversationVm[]> _conversationsByDuration;
        private NotifyTaskCompletion<ConversationVm[]> _conversationsByTraffic;
        private NotifyTaskCompletion<ConversationsStatisticsVm> _conversationsStatisticsVm;

        private RelayCommand<ConversationVm> _cShowConversationDetail;
        private ConversationVm _currentConversation;
        private FrameVm _currentFrame;

        private ConversationVm[] _filteredL7ConversationsArray;
        private ViewModelVirtualizingIoCObservableCollection<FrameVm, PmFrameBase> _frames;
        private InvestigationVm _investigationVm;

        private ViewModelVirtualizingIoCObservableCollection<ConversationVm, L3Conversation> _l3Conversations;
        private ViewModelVirtualizingIoCObservableCollection<ConversationVm, L4Conversation> _l4Conversations;
        private ViewModelVirtualizingIoCObservableCollection<ConversationVm, L7Conversation> _l7Conversations;

        protected ConversationsVm(IWindsorContainer applicationWindsorContainer, TType model, ExportService exportService) : base(applicationWindsorContainer, model)
        {
            this.ExportService = exportService;
            if (model is IWindsorContainerChanger) { this.ApplicationOrInvestigationWindsorContainer = (model as IWindsorContainerChanger).InvestigationWindsorContainer; }
            this.Model = model;
            this.InvestigationWindsorContainer = this.ApplicationOrInvestigationWindsorContainer;
            this.Frames = new ViewModelVirtualizingIoCObservableCollection<FrameVm, PmFrameBase>(this.Model.Frames, this.InvestigationWindsorContainer);
            this.L3Conversations = new ViewModelVirtualizingIoCObservableCollection<ConversationVm, L3Conversation>(this.Model.L3Conversations, this.InvestigationWindsorContainer);
            this.L4Conversations = new ViewModelVirtualizingIoCObservableCollection<ConversationVm, L4Conversation>(this.Model.L4Conversations, this.InvestigationWindsorContainer);
            this.L7Conversations = new ViewModelVirtualizingIoCObservableCollection<ConversationVm, L7Conversation>(this.Model.L7Conversations, this.InvestigationWindsorContainer);

            this.UsedSnoopers = new ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper>(this.Model.UsedSnoopers, this.InvestigationWindsorContainer);
            this.UsedSnoopers.CollectionChanged += this.UsedSnoopersOnCollectionChanged;

            Task.Factory.StartNew(() => { Messenger.Default.Register<ConversationMessage>(this, this.ConversationMessageHandler); });
        }

        public IConversationsModel Model { get; }
        public IWindsorContainer InvestigationWindsorContainer { get; private set; } //never change to public!!! .NET BUG!!!

        public FrameVm CurrentFrame
        {
            get { return this._currentFrame; }
            set
            {
                if(this._currentFrame == value) { return; }

                if(value != null && !this.Frames.Contains(value)) { return; }

                this._currentFrame = value;
                this.OnPropertyChanged();
                if(this.CurrentFrame == null) { FrameMessage.SendFrameMessage(this.CurrentFrame, FrameMessage.MessageType.CurrentFrameChanged, false); }
                //if (this._currentConversation != null)
                //{
                //    ConversationMessage.SendConversationMessage(this._currentConversation, ConversationMessage.MessageType.CurrentConversationChanged, false);
                //}
            }
        }

        public RelayCommand CRemoveConvGroupClick
            => new RelayCommand(() => { this.Logger?.Error($"Action is not implemented {Environment.StackTrace}");}); //todo

        public RelayCommandAsync<List<object>> CActualizeExportSet
            => new RelayCommandAsync<List<object>>(async (list, token) => await Task.Run(async () => { await this.ActualizeExportSet(list); }), o => true);

        public ExportService ExportService { get; set; }
        ///no fucking idea why InvestigationWindsorContainer is se to application, but... InvestigationOrAppWindsorContainer is set correctly to investigation

        public virtual ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper> AvailableSnoopers
            => this._availableSnoopers ?? (this._availableSnoopers = this.InvestigationVm?.AvailableSnoopers);

        public InvestigationVm InvestigationVm => this._investigationVm ?? (this._investigationVm = this.InvestigationWindsorContainer.Resolve<InvestigationVm>());

        // TODO: implement
        public RelayCommand CConversationChartDClick => new RelayCommand(() => this.Logger?.Error($"Action is not implemented {Environment.StackTrace}"));

        public RelayCommand CAddCypherKeyFromFile => new RelayCommand(() =>
        {
            var pkFilePath = this.SystemServices.OpenFileDialog("", ".pem", "Private key (*.pem)|*.pem;|All files (*.*)|*.*");

            if(pkFilePath.IsNullOrEmpty()) { return; }

            var pk = File.ReadAllText(pkFilePath);

            foreach(var conversation in this.L7Conversations)
            {
                var l7Conv = conversation.Conversation as L7Conversation;
                if(l7Conv == null)
                {
                    continue;
                }
                l7Conv.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }
            this.Logger?.Info($"Private key has been set to all conversations: {pkFilePath}");
        });

        public RelayCommand CShowVoIPOverView => new RelayCommand(() => new RelayCommand(null,()=>false));

        private NotifyTaskCompletion<ConversationsStatisticsVm> _ConversationsStatisticsVm
        {
            get
            {
                if(this._conversationsStatisticsVm == null)
                {
                    var conversationStatistics = new ConversationsStatisticsVm(this.InvestigationWindsorContainer);
                    this._conversationsStatisticsVm = new NotifyTaskCompletion<ConversationsStatisticsVm>(async () =>
                    {
                        return await Task.Run(async () =>
                        {
                            await conversationStatistics.InitializeAsync(this.Model);
                            return conversationStatistics;
                        });
                    }, () => { this.OnPropertyChanged(nameof(this.ConversationsStatisticsVm)); }, conversationStatistics, false);
                }
                return this._conversationsStatisticsVm;
            }
        }

        public ExportVm[] AllExportResults { get; private set; }

        public RelayCommand CAddConvGroupToExportClick
            => new RelayCommand(() =>{this.Logger?.Error($"Action is not implemented {Environment.StackTrace}");}); //todo

        public RelayCommand<IList> CCreateConversationsGroup => new RelayCommand<IList>(this.CreateConversationGroup);

        public NotifyTaskCompletion<ConversationVm[]> ConversationsByDuration
        {
            get
            {
                if(this._conversationsByDuration == null)
                {
                    this._conversationsByDuration = new NotifyTaskCompletion<ConversationVm[]>(async () =>
                    {
                        return await Task.Run(() =>
                        {
                            var conversationsByDuration = this.GetFilteredL7ConversationsArray();
                            Array.Sort(conversationsByDuration, (t1, t2) => -t1.Conversation.ConversationStats.Duration.CompareTo(t2.Conversation.ConversationStats.Duration));
                            this._conversationsByDuration = null;
                            return conversationsByDuration;
                        });
                    }, () => this.OnPropertyChanged(nameof(this.ConversationsByDuration)), true);
                }
                return this._conversationsByDuration;
            }
        }

        public NotifyTaskCompletion<ConversationVm[]> ConversationsByTraffic
        {
            get
            {
                if(this._conversationsByTraffic == null)
                {
                    this._conversationsByTraffic = new NotifyTaskCompletion<ConversationVm[]>(async () =>
                    {
                        return await Task.Run(() =>
                        {
                            var conversationsByTraffic = this.GetFilteredL7ConversationsArray();
                            Array.Sort(conversationsByTraffic, (t1, t2) => -t1.Conversation.ConversationStats.Bytes.CompareTo(t2.Conversation.ConversationStats.Bytes));
                            return conversationsByTraffic;
                        });
                    }, () => this.OnPropertyChanged(nameof(this.ConversationsByTraffic)), true);
                }
                return this._conversationsByTraffic;
            }
        }

        public ConversationsStatisticsVm ConversationsStatisticsVm => this._ConversationsStatisticsVm; //necessary for NotifyTaskCompletion

        public RelayCommand<ConversationVm> CShowConversationDetail
            =>
                this._cShowConversationDetail
                ?? (this._cShowConversationDetail = new RelayCommand<ConversationVm>(conv => this.NavigationService.Show(typeof(ConversationDetailVm), conv)));

        public RelayCommand<FrameVm> CShowFrameDetail => new RelayCommand<FrameVm>(frame => this.NavigationService.Show(typeof(FrameContentVm), frame));

        public ConversationVm CurrentConversation
        {
            get { return this._currentConversation; }
            set
            {
                if(this._currentConversation == value) { return; }

                if(value != null && !(this.L3Conversations.Contains(value) || this.L4Conversations.Contains(value) || this.L7Conversations.Contains(value))) { return; }

                this._currentConversation = value;
                this.OnPropertyChanged();
                if(this._currentConversation != null && this._canBroadcastSelectedConversationChange) {
                    ConversationMessage.SendConversationMessage(this._currentConversation, ConversationMessage.MessageType.CurrentConversationChanged, false);
                }
            }
        }

        //#region Properties
        //public TType EncapsulatedData { get; set; }

        // public IExportResultsProvider ExportResultsProvider { get; set; }
        public ViewModelVirtualizingIoCObservableCollection<FrameVm, PmFrameBase> Frames
        {
            get
            {
                return this._frames ?? (this._frames = new ViewModelVirtualizingIoCObservableCollection<FrameVm, PmFrameBase>(this.Model.Frames, this.InvestigationWindsorContainer));
            }
            protected set { this._frames = value; }
        }

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                lock(this._isSelectedLock)
                {
                    var previousIsSelected = base.IsSelected;
                    base.IsSelected = value;
                    //if (base.IsSelected && previousIsSelected != base.IsSelected && ++this._isSelectedTimes > 0)
                    //{

                    //    this.L3Conversations.ResumeCollectionChangeNotification();
                    //    this.L4Conversations.ResumeCollectionChangeNotification();
                    //    this.L7Conversations.ResumeCollectionChangeNotification();
                    //    this.L3Conversations.IsSuspended = false;
                    //    this.L4Conversations.IsSuspended = false;
                    //    this.L7Conversations.IsSuspended = false;
                    //}
                    //else if (!base.IsSelected && previousIsSelected != base.IsSelected && --this._isSelectedTimes <= 0)
                    //{
                    //    this.L3Conversations.SuspendCollectionChangeNotification();
                    //    this.L4Conversations.SuspendCollectionChangeNotification();
                    //    this.L7Conversations.SuspendCollectionChangeNotification();

                    //    this.L3Conversations.IsSuspended = true;
                    //    this.L4Conversations.IsSuspended = true;
                    //    this.L7Conversations.IsSuspended = true;
                    //}
                }
            }
        }

        public ViewModelVirtualizingIoCObservableCollection<ConversationVm, L3Conversation> L3Conversations
        {
            get
            {
                return this._l3Conversations
                       ?? (this._l3Conversations =
                           new ViewModelVirtualizingIoCObservableCollection<ConversationVm, L3Conversation>(this.Model.L3Conversations, this.InvestigationWindsorContainer));
            }
            protected set { this._l3Conversations = value; }
        }

        public ViewModelVirtualizingIoCObservableCollection<ConversationVm, L4Conversation> L4Conversations
        {
            get
            {
                return this._l4Conversations
                       ?? (this._l4Conversations =
                           new ViewModelVirtualizingIoCObservableCollection<ConversationVm, L4Conversation>(this.Model.L4Conversations, this.InvestigationWindsorContainer));
            }
            protected set { this._l4Conversations = value; }
        }

        public ViewModelVirtualizingIoCObservableCollection<ConversationVm, L7Conversation> L7Conversations
        {
            get
            {
                return this._l7Conversations
                       ?? (this._l7Conversations =
                           new ViewModelVirtualizingIoCObservableCollection<ConversationVm, L7Conversation>(this.Model.L7Conversations, this.InvestigationWindsorContainer));
            }
            protected set { this._l7Conversations = value; }
        }

        public ConversationVm SetCurrentConversationByFrame(FrameVm frameVm)
        {
            //var newCovnersation = this.Conversations.FirstOrDefault(c => c.Conversation.Frames.Contains(frameVm));
            //if (newCovnersation != null)
            //{
            //    this.CurrentConversation = newCovnersation;
            //    return newCovnersation;
            //}

            return null;
        }

        //#region Current conversation
        public ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper> UsedSnoopers { get; }

        public void CreateConversationGroup(IList conversationsVmList)
        {
            if(conversationsVmList == null)
            {
                this.Logger?.Error($"Create conversation group: Invalid conversation model type");
                return;
            }
            if(!conversationsVmList.Any())
            {
                this.Logger?.Error($"No conversations selected");
                return;
            }
            var conversationGroups = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<ConversationsGroup>>();
            var conversationGroup = new ConversationsGroup();
            foreach(var conversationVm in conversationsVmList)
            {
                var convVm = conversationVm as ConversationVm;
                if(convVm?.Conversation == null)
                {
                    this.Logger?.Error($"Create conversation group: Invalid conversation model type");
                    continue;
                }
                conversationGroup.AddConversation(convVm.Conversation);
            }
            conversationGroups.Add(conversationGroup);
        }

        public ConversationsGroup CreateConversationGroup(IEnumerable<ILxConversation> conversations)
        {
            if(conversations == null)
            {
                this.Logger?.Error($"Create conversation group: Invalid conversation model type");
                return null;
            }
            var lxConversations = conversations as ILxConversation[] ?? conversations.ToArray();
            if(!lxConversations.Any())
            {
                this.Logger?.Error($"No conversations selected");
                return null;
            }
            var conversationGroups = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<ConversationsGroup>>();
            var conversationGroup = new ConversationsGroup();
            foreach(var conversation in lxConversations) { conversationGroup.AddConversation(conversation); }
            conversationGroups.Add(conversationGroup);
            return conversationGroup;
        }

        protected virtual async Task ActualizeExportSet(List<object> parameters)
        {
            var values = parameters;
            var dontUseApplicationTagsForceOnAllConversations = (bool) values[0];
            var showExportedObjectsDuringExportation = (bool) values[1];
            await this.ExportService.Export(this.Model.L7Conversations, this.CurrentInvestigationSelectedExporters(), null, false, showExportedObjectsDuringExportation,
                dontUseApplicationTagsForceOnAllConversations);
        }

        protected List<ISnooper> CurrentInvestigationSelectedExporters()
        {
            return (from exporterVm in this.AvailableSnoopers
                where exporterVm.IsEnabled
                select exporterVm.Snooper).ToList();
        }

        /// <summary>
        ///     http://proveitwithaunittest.wordpress.com/2013/07/19/c-using-parallel-linq-plinq-to-find-the-average/
        ///     http://stackoverflow.com/questions/5075484/property-selector-expressionfunct-how-to-get-set-value-to-selected-property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        private double Average<T>(IEnumerable<T> items, Expression<Func<T, long>> selector)
        {
            var sel = selector.Compile();
            return items.AsParallel().Aggregate(
                // the datetype its starting with this will be used for TSource and TAccumulate
                new AverageStruct(),
                // This section is repeated multiple times on multiple threads.
                // subTotal is passed from call to call on a thread, each call
                // gives the next item from items. So we are merging item into subTotal
                (subTotal, item) =>
                {
                    subTotal.Sum += sel(item);
                    subTotal.Count++;
                    return subTotal;
                },
                // This section happens after a single thread has completed its
                // assigned number of items from items IEnumerable.
                // its job is to take total an AverageStruct and to take thisThread also an AverageStruct
                // and then to merge them together. So the end result is one AverageStruct item after all
                // the threads have completed their processing.
                (total, thisThread) =>
                {
                    total.Sum += thisThread.Sum;
                    total.Count += thisThread.Count;
                    return total;
                },
                // This is the simple average calculation. final is that overal merged AverageStruct
                // which contains all the needed answers to make the final calulation.
                // The casting final.Count to double is needed because other wise
                // since both values and ints they will divide like ints.
                final => final.Sum / (double) final.Count);
        }

        private void ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            var closestDataPoint = e.Context.ClosestDataPoint;
            if(closestDataPoint != null)
            {
                var data = closestDataPoint.DataPoint.DataItem as KeyValue<DateTime, long>;
                //this._trackedFrame = data.Ref as Models.Base.Frame;
                //this.radTimelineTimeInfo.Text = data.Key.ToString();
                //this.radTimelineBytesInfo.Text = data.Value.ToString();
            }
        }

        private void ConversationMessageHandler(ConversationMessage conversationMessage)
        {
            if(conversationMessage.Type != ConversationMessage.MessageType.CurrentConversationChanged) { return; }
            this._canBroadcastSelectedConversationChange = false;
            this.CurrentConversation = conversationMessage.ConversationVm as ConversationVm;
            this._canBroadcastSelectedConversationChange = true;
        }

        private void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Statistics") { this.StatisticsCreated(); }
        } //todo in another fashion

        private ConversationVm[] GetFilteredL7ConversationsArray()
        {
            if(this._filteredL7ConversationsArray == null)
            {
                lock(this._getFilteredL7ConversationsArrayLock)
                {
                    var conversationsBag = new ConcurrentBag<ConversationVm>();
                    var avgDur = (long) this.Average(this.L7Conversations, vm => (long) vm.Conversation.ConversationStats.Duration.TotalMilliseconds);
                    var avgTraff = (long) this.Average(this.L7Conversations, vm => vm.Conversation.ConversationStats.Bytes);
                    Parallel.ForEach(this.L7Conversations, (conversation, state, index) =>
                    {
                        if(avgDur > conversation.Conversation.ConversationStats.Duration.TotalMilliseconds && avgTraff > conversation.Conversation.ConversationStats.Bytes) {
                            return;
                        }
                        conversationsBag.Add(conversation);
                    });
                    this._filteredL7ConversationsArray = conversationsBag.ToArray();
                }
            }
            return this._filteredL7ConversationsArray;
        }

        private void StatisticsCreated()
        {
            this.OnPropertyChanged("TransportProtocolsDistribution");
            this.OnPropertyChanged("LinkProtocolsDistribution");
            this.OnPropertyChanged("AppProtocolsDistribution");
            this.OnPropertyChanged("TrafficErrors");
        }

        private void UsedSnoopersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) { }

        protected enum CachedItems
        {
            AllExportResults,
            TotalPacketsCount,
            AppProtocolsCount,
            UniqueHostsCount,
            Period,
            AppProtocolsDistribution,
            AppProtocolsSummary,
            TransportProtocolsDistribution,
            TransportProtocolsSummary,
            LinkProtocolsDistribution,
            LinkProtocolsSummary,
            TrafficHistory,
            TrafficErrors,
            ConversationsByTraffic,
            ConversationsByDuration,
            HostsTraffic,
            CommunicationNodes
        }

        private struct AverageStruct
        {
            // need Int64 because otherwise you end up with negative numbers when the bit flips
            public Int64 Sum { get; set; }
            public Int64 Count { get; set; }
        }
    }
}