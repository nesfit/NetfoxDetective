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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using AlphaChiTech.VirtualizingCollection.Interfaces;
using Castle.Windsor;
using Netfox.Core.Database;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Models;
using Netfox.Detective.Interfaces.Models.Base;
using Netfox.Detective.Models.Conversations;
using Netfox.Detective.Models.Exports;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;
using Netfox.Persistence;
using PostSharp.Patterns.Model;
using Component = Castle.MicroKernel.Registration.Component;

namespace Netfox.Detective.Models.Base
{
    /// <summary>
    ///     ConversationsVm of investigation.
    /// </summary>
    [NotifyPropertyChanged]
    public class Investigation : IInvestigation
    {
        private IObservableCollection<PmFrameBase> _allFrames;
        private IObservableCollection<PmCaptureBase> _captures;
        private IObservableCollection<ConversationsGroup> _conversationsGroups;
        private IObservableCollection<ExportGroup> _exportsGroups;
        private IObservableCollection<PmFrameBase> _frames;
        private IObservableCollection<L3Conversation> _l3Conversations;
        private IObservableCollection<L4Conversation> _l4Conversations;
        private IObservableCollection<L7Conversation> _l7Conversations;

        private ConcurrentIObservableVirtualizingObservableCollection<OperationLog> _operationLogs;
        private IObservableCollection<SnooperExportBase> _snooperExports;
        private IObservableCollection<SourceLog.SourceLog> _sourceLogs;

        public Investigation(IInvestigationInfo investigationInfo)
        {
            this.InvestigationInfo = investigationInfo;
            this.InvestigationInfo.LastRecentlyUsed = DateTime.UtcNow;
            this.OperationLogs = new ConcurrentIObservableVirtualizingObservableCollection<OperationLog>();
        }

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IObservableCollection<PmCaptureBase> Captures
            => this._captures ?? (this._captures = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmCaptureBase>>());

        [DataMember]
        public IInvestigationInfo InvestigationInfo { get; }

        [NotMapped]
        public ConcurrentIObservableVirtualizingObservableCollection<OperationLog> OperationLogs
        {
            get { return this._operationLogs; }
            set
            {
                this._operationLogs = value;
                this.OnPropertyChanged();
            }
        }

        [IgnoreAutoChangeNotification]
        [NotMapped]
        public IObservableCollection<ConversationsGroup> ConversationsGroups
            =>
                this._conversationsGroups
                ?? (this._conversationsGroups = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<ConversationsGroup>>());

        [IgnoreAutoChangeNotification]
        [NotMapped]
        public IObservableCollection<ExportGroup> ExportsGroups
            => this._exportsGroups ?? (this._exportsGroups = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<ExportGroup>>());

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IObservableCollection<PmFrameBase> AllFrames
            => this._allFrames ?? (this._allFrames = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>());

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IObservableCollection<SourceLog.SourceLog> SourceLogs
            => this._sourceLogs ?? (this._sourceLogs = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SourceLog.SourceLog>>());

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IObservableCollection<PmFrameBase> Frames
            => this._frames ?? (this._frames = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>());

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IObservableCollection<L3Conversation> L3Conversations
            => this._l3Conversations ?? (this._l3Conversations = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L3Conversation>>(new
            {
                eagerLoadProperties = new[]
                {
                    nameof(L3Conversation.ConversationFlowStatistics)
                }
            }));

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IObservableCollection<L4Conversation> L4Conversations
            => this._l4Conversations ?? (this._l4Conversations = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L4Conversation>>(new
            {
                eagerLoadProperties = new[]
                {
                    nameof(L4Conversation.ConversationFlowStatistics)
                }
            }));

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IObservableCollection<L7Conversation> L7Conversations
            => this._l7Conversations ?? (this._l7Conversations = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L7Conversation>>(new
            {
                eagerLoadProperties = new[]
                {
                    nameof(L7Conversation.UnorderedL7PDUs),
                    nameof(L7Conversation.ConversationFlowStatistics),
                    nameof(L7Conversation.L4Conversation),
                    $"{nameof(L7Conversation.UnorderedL7PDUs)}.{nameof(L7PDU.UnorderedFrameList)}"
                }
            }));

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IObservableCollection<SnooperExportBase> SnooperExports
            => this._snooperExports ?? (this._snooperExports = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>());

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IObservableCollection<ISnooper> UsedSnoopers { get; } = new ConcurrentIObservableVirtualizingObservableCollection<ISnooper>();

        public async Task Initialize()
        {
            await Task.Run(() =>
            {
                this.InitializeDatabase();
                this._captures = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmCaptureBase>>();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotMapped]
        public IWindsorContainer InvestigationWindsorContainer { get; set; }

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitializeDatabase()
        {
            this.InvestigationWindsorContainer.Register(Component.For<SqlConnectionStringBuilder>().Instance(this.InvestigationInfo.SqlConnectionStringBuilder).LifestyleSingleton());
            if(this.InvestigationInfo.IsInMemory)
            {
                this.InvestigationWindsorContainer.Register(Component.For<NetfoxDbContext, NetfoxDbContextInMemory>().LifestyleSingleton());
                this.InvestigationWindsorContainer.Register(
                    Component.For<IObservableNetfoxDBContext>().ImplementedBy<NetfoxDbContextInMemory>().Named(nameof(IObservableNetfoxDBContext)).LifestyleSingleton());
            }
            else
            {
                this.InvestigationWindsorContainer.Register(Component.For<NetfoxDbContext>().ImplementedBy<NetfoxDbContext>().LifestyleTransient());
                this.InvestigationWindsorContainer.Register(
                    Component.For<IObservableNetfoxDBContext>().ImplementedBy<NetfoxDbContext>().Named(nameof(IObservableNetfoxDBContext)).LifestyleSingleton());
            }
            var iObservableNetfoxDBContext = this.InvestigationWindsorContainer.Resolve<IObservableNetfoxDBContext>();
            iObservableNetfoxDBContext.RegisterDbSetTypes();
            iObservableNetfoxDBContext.RegisterVirtualizingObservableDBSetPagedCollections();
            using(var dbx = this.InvestigationWindsorContainer.Resolve<NetfoxDbContext>()) { dbx.CheckCreateDatabase(); }
        }
    }
}