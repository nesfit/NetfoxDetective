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

using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Core;
using Netfox.Detective.ViewModelsDataEntity;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;
using PostSharp.Patterns.Model;

//using System.Windows.Forms.VisualStyles.VisualStyleElement.MenuBand;

namespace Netfox.Detective.ViewModels.Conversations
{
    [NotifyPropertyChanged]
    public class ConversationsOverviewVm : DetectiveIvestigationDataEntityPaneViewModelBase
    {
        private IConversationsVm _conversationsVm;

        public ConversationsOverviewVm(WindsorContainer applicationWindsorContainer, IConversationsVm model) : base(applicationWindsorContainer, model)
        {
            this.ConversationsVm = model;
            this.NavigationService.Show(typeof(ConversationsDetailVm), model, false); //hack to open pane in background while opening overview ... temporary todo
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IConversationsOverviewView>());
            //Binder.Bind(this, "IsSelected", this.ConversationsVm, "IsSelected", BindingDirection.OneWay);
            //Task.Factory.StartNew(() => Messenger.Default.Register<CaptureMessage>(this, this.CaptureActionHandler));
            //this.PropertyObserver = new PropertyObserver<IConversationsVm>(this.ConversationsVm);
            //this.PropertyObserver.RegisterHandler(n => n.CurrentConversation,
            //    n => ConversationMessage.SendConversationMessage(n.CurrentConversation, ConversationMessage.MessageType.CurrentConversationChanged, false));
        }

        public override string HeaderText => "Conversations overview";

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

        public PropertyObserver<IConversationsVm> PropertyObserver { get; set; }

        public IConversationsVm ConversationsVm
        {
            get { return this._conversationsVm; }
            set
            {
                this._conversationsVm = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("IsCaptureVm");
                this.OnPropertyChanged("IsConversationGroupVm");
            }
        }

        [SafeForDependencyAnalysis]
        public bool IsCaptureVm => this.ConversationsVm?.GetType() == typeof(CaptureVm);

        [SafeForDependencyAnalysis]
        public bool IsConversationGroupVm => this.ConversationsVm?.GetType() == typeof(ConversationsGroupVm);
    }
}