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
using System.Diagnostics;
using System.Windows;
using System.Windows.Interactivity;
using Telerik.Windows.Controls;

namespace Netfox.Detective.Views.Behaviours
{
    public class MultiSelectRadTreeViewBehavior : Behavior<RadTreeView>
    {
        private RadTreeView Grid => this.AssociatedObject as RadTreeView;

        public object SelectedItems
        {
            get { return (object) this.GetValue(SelectedItemsProperty); }
            set { this.SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
                  DependencyProperty.Register("SelectedItems", typeof(object), typeof(MultiSelectRadTreeViewBehavior), new PropertyMetadata(OnSelectedItemsPropertyChanged));


        private static void OnSelectedItemsPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var collection = args.NewValue as INotifyCollectionChanged;
            if (collection != null)
            {
                collection.CollectionChanged += ((MultiSelectRadTreeViewBehavior)target).ContextSelectedItems_CollectionChanged;
            }
        }

        private void ContextSelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach(var itemObjVm in e.NewItems)
                    {
                        FrameworkElement item = null;
                        foreach(var gridItem in this.Grid.Items)
                        {
                            var itemFe = (gridItem as FrameworkElement);
                            if(itemFe?.DataContext == itemObjVm) item = itemFe;
                        }
                        if (item != null && !this.Grid.SelectedItems.Contains(item))
                        this.Grid.SelectedItems.Add(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var itemObjVm in e.OldItems)
                    {
                        FrameworkElement item = null;
                        foreach (var gridItem in this.Grid.Items)
                        {
                            var itemFe = (gridItem as FrameworkElement);
                            if (itemFe?.DataContext == itemObjVm) item = itemFe;
                        }
                        if (item != null && !this.Grid.SelectedItems.Contains(item))
                            this.Grid.SelectedItems.Remove(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                default:
                    Debugger.Break(); //todo implement
                    break;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.Grid.SelectedItems.CollectionChanged += this.GridSelectedItems_CollectionChanged;
        }

        private void GridSelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var selectedItems = (this.SelectedItems as IList);
                    if(selectedItems == null) return;
                    foreach (var itemObj in e.NewItems)
                    {
                        var item = (itemObj as FrameworkElement)?.DataContext;
                        if (item != null && !selectedItems.Contains(item))
                            selectedItems.Add(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    selectedItems = (this.SelectedItems as IList);
                    if (selectedItems == null) return;
                    foreach (var itemObj in e.OldItems)
                    {
                        var item = (itemObj as FrameworkElement)?.DataContext;
                        if (item != null && !selectedItems.Contains(item))
                            selectedItems.Remove(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                default:
                    Debugger.Break(); //todo implement
                    break;
            }
        }
    }
}
