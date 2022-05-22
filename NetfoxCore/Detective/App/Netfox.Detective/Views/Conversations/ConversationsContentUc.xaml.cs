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

using System.Windows.Controls;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;

namespace Netfox.Detective.Views.Conversations
{
    /// <summary>
    ///     Interaction logic for ConversationsContentView.xaml
    /// </summary>
    public partial class ConversationsContentUc : UserControl
    {
        public ConversationsContentUc()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        ///// <summary>
        ///// Identified the ConversationsVm dependency property
        ///// </summary>
        //public static readonly DependencyProperty ConversationsVmProperty =
        //    DependencyProperty.Register("ConversationsVm", typeof(IConversationsVm),typeof(ConversationsContentUc));

        public IConversationsVm ConversationsVm
        {
            get { return (IConversationsVm) this.DataContext; }
            set { this.DataContext = value; }
        }
    }
}