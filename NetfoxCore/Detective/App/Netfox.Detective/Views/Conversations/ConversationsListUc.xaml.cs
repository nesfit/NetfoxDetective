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

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.BaseTypes.Views;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Telerik.Windows;

namespace Netfox.Detective.Views.Conversations
{
    /// <summary>
    ///     Interaction logic for ConversationsListUc.xaml
    /// </summary>
    public partial class ConversationsListUc : CollectionUserControlBase
    {
        public static readonly DependencyProperty CCreateConversationsGroupProperty = DependencyProperty.Register(nameof(CCreateConversationsGroup), typeof(RelayCommand<IList>),
            typeof(ConversationsListUc));

        public static readonly DependencyProperty CDoubleClickedConversationProperty = DependencyProperty.Register(nameof(CDoubleClickedConversation),
            typeof(RelayCommand<ConversationVm>), typeof(ConversationsListUc));

        public ConversationsListUc()
        {
            this.InitializeComponent();
            this.ConversationsLisDataGrid.SelectionChanged += this.ConversationsLisDataGridOnSelectionChanged;
        }

        public RelayCommand<IList> CCreateConversationsGroup
        {
            get { return (RelayCommand<IList>) this.GetValue(CCreateConversationsGroupProperty); }
            set { this.SetValue(CCreateConversationsGroupProperty, value); }
        }

        public RelayCommand<ConversationVm> CDoubleClickedConversation
        {
            get { return (RelayCommand<ConversationVm>) this.GetValue(CDoubleClickedConversationProperty); }
            set { this.SetValue(CDoubleClickedConversationProperty, value); }
        }

        private void ConversationsLisDataGridOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var selectedItem = this.ConversationsLisDataGrid.SelectedItem;
            if(selectedItem != null) { this.ConversationsLisDataGrid.ScrollIntoView(selectedItem); }
        }

        private void RadMenuItem_OnClick(object sender, RadRoutedEventArgs e) { this.CCreateConversationsGroup?.Execute(this.ConversationsLisDataGrid.SelectedItems); }
    }
}