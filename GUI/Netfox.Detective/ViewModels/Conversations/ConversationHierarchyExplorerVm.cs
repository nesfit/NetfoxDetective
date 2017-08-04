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

using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Messages.Base;
using Netfox.Detective.Core;
using Netfox.Detective.Interfaces;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Conversations
{
    public class ConversationHierarchyExplorerVm : DetectiveApplicationPaneViewModelBase
    {
        public ICrossContainerHierarchyResolver CrossContainerHierarchyResolver { get; }
        private bool _canBroadcastSelectedConversationChange;
        private RelayCommand _CClearConversationsList;
        private RelayCommand _CRemoveConversation;
        private RelayCommand _CShowConversationDetail;
        private ILxConversation _selectedConversation;

        public ConversationHierarchyExplorerVm(IWindsorContainer applicationWindsorContainer, ICrossContainerHierarchyResolver crossContainerHierarchyResolver) : base(applicationWindsorContainer)
        {
            this.CrossContainerHierarchyResolver = crossContainerHierarchyResolver;
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IConversationHierarchyExplorer>());
            this.DockPositionPosition = DetectiveDockPosition.DockedRight;
            this.IsHidden = false;
            this.IsSelected = true;
            Task.Factory.StartNew(() => { Messenger.Default.Register<ConversationMessage>(this, this.ConversationMessageHandler); });
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "Conversation explorer";
        #endregion

        public ILxConversation SelectedConversation
        {
            get => this._selectedConversation;
            set
            {
                if(this._selectedConversation == value || value == null) { return; }
                this._selectedConversation = value;
                this.SelectedConversationVm = this.CrossContainerHierarchyResolver.Resolve<ConversationVm>(value);
                this.BroadcastSelectedConversationChange();
            }
        }

        [IgnoreAutoChangeNotification]
        public string SelectedConversationToRootPath
        {
            get
            {
                var sb = new StringBuilder();
                if(this.SelectedConversation is L3Conversation) {
                    sb.Append((this.SelectedConversation as L3Conversation).Name);
                }
                else if(this.SelectedConversation is L4Conversation)
                {
                    sb.Append((this.SelectedConversation as L4Conversation).L3Conversation.Name);
                    sb.Append(@"\");
                    sb.Append((this.SelectedConversation as L4Conversation).Name);
                }
                else if(this.SelectedConversation is L7Conversation)
                {
                    sb.Append(@"\");
                    sb.Append((this.SelectedConversation as L7Conversation).L4Conversation.Name);
                    sb.Append(@"\");
                    sb.Append((this.SelectedConversation as L7Conversation).Name);
                }
                return sb.ToString();
            }
        }

        public ObservableCollection<L3Conversation> RootConversations { get; } = new ObservableCollection<L3Conversation>();
        public ConversationVm SelectedConversationVm { get; set; }

        [IgnoreAutoChangeNotification]
        public RelayCommand CRemoveConversation
        {
            get
            {
                return this._CRemoveConversation ?? (this._CRemoveConversation = new RelayCommand(() =>
                {
                    this.RootConversations.Remove(this.GetRootConversation(this.SelectedConversation));
                    this.SelectedConversation = null;
                    this.SelectedConversationVm = null;
                    this.NotifyChanges();
                }, () => this.SelectedConversationVm != null));
            }
        }

        [IgnoreAutoChangeNotification]
        public RelayCommand CClearConversationsList
        {
            get
            {
                return this._CClearConversationsList ?? (this._CClearConversationsList = new RelayCommand(() =>
                {
                    this.RootConversations.Clear();
                    this.SelectedConversation = null;
                    this.SelectedConversationVm = null;
                    this.NotifyChanges();
                }, () => this.RootConversations.Any()));
            }
        }

        [IgnoreAutoChangeNotification]
        public RelayCommand CShowConversationDetail
        {
            get
            {
                return this._CShowConversationDetail
                       ?? (this._CShowConversationDetail =
                           new RelayCommand(() => this.NavigationService.Show(typeof(ConversationDetailVm), this.SelectedConversationVm, true),
                               () => this.SelectedConversation != null));
            }
        }

        private void BroadcastSelectedConversationChange()
        {
            if(!this._canBroadcastSelectedConversationChange) { return; }
            ConversationMessage.SendConversationMessage(this.SelectedConversationVm, ConversationMessage.MessageType.CurrentConversationChanged, false);
        }

        private void ChangeRootConversation(ConversationVm conversation)
        {
            var lxConvModel = conversation?.Conversation;
            var rootConversationModel = this.GetRootConversation(lxConvModel);
            if(rootConversationModel == null) { return; }

            if(this.SelectedConversation == lxConvModel || this.RootConversations.Contains(rootConversationModel)) { return; }
            this.RootConversations.Add(rootConversationModel);
        }

        private void ConversationMessageHandler(ConversationMessage conversationMessage)
        {
            if(conversationMessage.Type != ConversationMessage.MessageType.CurrentConversationChanged) { return; }
            this._canBroadcastSelectedConversationChange = false;

            var selectedConversationVm = conversationMessage.ConversationVm as ConversationVm;
            this.ChangeRootConversation(selectedConversationVm);
            this.SelectedConversationVm = selectedConversationVm;
            this.SelectedConversation = selectedConversationVm?.Conversation;

            this._canBroadcastSelectedConversationChange = true;

            this.NotifyChanges();
        }

        private L3Conversation GetRootConversation(ILxConversation lxConvModel)
        {
            L3Conversation rootConversationModel = null;
            if(lxConvModel is L3Conversation) {
                rootConversationModel = lxConvModel as L3Conversation;
            }
            else if(lxConvModel is L4Conversation) {
                rootConversationModel = (lxConvModel as L4Conversation)?.L3Conversation;
            }
            else if(lxConvModel is L7Conversation) { rootConversationModel = (lxConvModel as L7Conversation)?.L4Conversation?.L3Conversation; }
            return rootConversationModel;
        }

        private void NotifyChanges()
        {
            this.CClearConversationsList.RaiseCanExecuteChanged();
            this.CRemoveConversation.RaiseCanExecuteChanged();
            this.CShowConversationDetail.RaiseCanExecuteChanged();
        }
    }
}