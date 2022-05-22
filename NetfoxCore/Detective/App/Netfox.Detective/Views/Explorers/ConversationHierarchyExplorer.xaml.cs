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

using System.Windows;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.ViewModels.Conversations;

namespace Netfox.Detective.Views.Explorers
{
    /// <summary>
    ///     Interaction logic for ConversationHierarchyExplorer.xaml
    /// </summary>
    public partial class ConversationHierarchyExplorer : DetectiveApplicationPaneViewBase, IConversationHierarchyExplorer
    {
        public ConversationHierarchyExplorer()
        {
            this.InitializeComponent();
            //this.ConversationExplorerRadTreeView.AutoScrollToSelectedItem = true;
            this.ConversationExplorerRadTreeView.SelectionChanged += (sender, args) =>
            {
                //this.ConversationExplorerRadTreeView.ExpandAll();
                this.ConversationExplorerRadTreeView.BringPathIntoView((this.DataContext as ConversationHierarchyExplorerVm)?.SelectedConversationToRootPath);
            };
        }

        public void ConversationExplorerRadTreeViewCollapseAll(object sender, RoutedEventArgs routedEventArgs) { this.ConversationExplorerRadTreeView.CollapseAll(); }

        public void ConversationExplorerRadTreeViewExpandAll(object sender, RoutedEventArgs routedEventArgs) { this.ConversationExplorerRadTreeView.ExpandAll(); }
    }
}