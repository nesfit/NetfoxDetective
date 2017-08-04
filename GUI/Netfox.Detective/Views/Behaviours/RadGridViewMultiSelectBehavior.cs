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
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Interactivity;
using Telerik.Windows.Controls;

namespace Netfox.Detective.Views.Behaviours
{
    public class RadGridViewMultiSelectBehavior : Behavior<RadGridView>
    {
        private RadGridView Grid => this.AssociatedObject as RadGridView;

        public INotifyCollectionChanged SelectedItems
        {
            get { return (INotifyCollectionChanged) this.GetValue(SelectedItemsProperty); }
            set { this.SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItemsProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(INotifyCollectionChanged), typeof(RadGridViewMultiSelectBehavior), new PropertyMetadata(OnSelectedItemsPropertyChanged));


        private static void OnSelectedItemsPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var collection = args.NewValue as INotifyCollectionChanged;
            if (collection != null)
            {
                collection.CollectionChanged += ((RadGridViewMultiSelectBehavior)target).ContextSelectedItems_CollectionChanged;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.Grid.SelectedItems.CollectionChanged += this.GridSelectedItems_CollectionChanged;
        }

        void ContextSelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UnsubscribeFromEvents();

            Transfer(this.SelectedItems as IList, this.Grid.SelectedItems);

            this.SubscribeToEvents();
        }

        void GridSelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UnsubscribeFromEvents();

            Transfer(this.Grid.SelectedItems, this.SelectedItems as IList);

            this.SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            this.Grid.SelectedItems.CollectionChanged += this.GridSelectedItems_CollectionChanged;

            if (this.SelectedItems != null)
            {
                this.SelectedItems.CollectionChanged += this.ContextSelectedItems_CollectionChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            this.Grid.SelectedItems.CollectionChanged -= this.GridSelectedItems_CollectionChanged;

            if (this.SelectedItems != null)
            {
                this.SelectedItems.CollectionChanged -= this.ContextSelectedItems_CollectionChanged;
            }
        }

        public static void Transfer(IList source, IList target)
        {
            if (source == null || target == null)
                return;

            target.Clear();

            foreach (var o in source)
            {
                target.Add(o);
            }
        }
    }
}
