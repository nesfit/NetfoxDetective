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
using System.Windows.Controls;
using System.Windows.Interactivity;
using Telerik.Windows.Controls;

namespace Netfox.Detective.Views.Behaviours
{
    //<behaviours:ScrollIntoViewBehavior />
    //</i:Interaction.Behaviors>-->
    //</telerik:RadListBox>
    /// <summary>
    ///     <telerik:RadListBox ....
    ///         <!--<i:Interaction.Behaviors>
    /// </summary>
    public sealed class ScrollIntoViewBehavior : Behavior<RadListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.LayoutUpdated += this.AssociatedObjectOnLayoutUpdated;
            this.AssociatedObject.SelectionChanged += this.ScrollIntoView;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.SelectionChanged -= this.ScrollIntoView;
            base.OnDetaching();
        }

        private void AssociatedObjectOnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            if(sender == null) { return; }
            this.AssociatedObject.ScrollIntoView(this.AssociatedObject.Items.Count - 1);
        }

        private void ScrollIntoView(object o, SelectionChangedEventArgs e)
        {
            var b = (RadListBox) o;
            if(b == null) { return; }
            if(b.SelectedItem == null) { return; }

            var item = (RadListBoxItem) ((RadListBox) o).ItemContainerGenerator.ContainerFromItem(((RadListBox) o).SelectedItem);
            if(item != null) { item.BringIntoView(); }
        }
    }
}