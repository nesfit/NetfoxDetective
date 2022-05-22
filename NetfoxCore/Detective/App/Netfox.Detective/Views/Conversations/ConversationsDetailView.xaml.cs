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

using Netfox.Core.Interfaces.Views;

namespace Netfox.Detective.Views.Conversations
{
    /// <summary>
    ///     Interaction logic for ConversationsDetailView.xaml
    /// </summary>
    public partial class ConversationsDetailView : DetectiveDataEntityPaneViewBase, IConversationsDetailView
    {
        //private ConversationsGroupVm _context;

        public ConversationsDetailView()
        {
            this.InitializeComponent();
            //Task.Factory.StartNew(() => Messenger.Default.Register<ConversationsGroupMessage>(this, this.ConversationsGroupMessageHandler));
        }

        //private void ConversationsGroupMessageHandler(ConversationsGroupMessage message)
        //{
        //    if(message.Type == ConversationsGroupMessage.MessageType.GroupVmSelected)
        //    {
        //        this._context = message.ConversationsGroupVm as ConversationsGroupVm;
        //        this.DataContext = this._context;
        //        this.Visibility = (this._context != null? Visibility.Visible : Visibility.Hidden);
        //        this.ControlVisible = (this._context != null? Visibility.Visible : Visibility.Collapsed);
        //        this.ConversationsDetail.ConversationsVm = this._context;
        //    }
        //}
    }
}