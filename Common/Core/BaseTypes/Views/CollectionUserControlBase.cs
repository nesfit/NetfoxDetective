// Copyright (c) 2017 Jan Pluskal
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
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;

namespace Netfox.Core.BaseTypes.Views
{
    public class CollectionUserControlBase : UserControl
    {
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CollectionUserControlBase), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = false,
            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(CollectionUserControlBase), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        public static readonly DependencyProperty CSelectItemProperty = DependencyProperty.Register("CSelectedItemItem", typeof(RelayCommand<object>), typeof(CollectionUserControlBase));
        public static readonly DependencyProperty CSelectDoubleClikedItemProperty = DependencyProperty.Register("CSelectedDoubleCliked", typeof(RelayCommand<object>), typeof(CollectionUserControlBase));

        public CollectionUserControlBase()
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(this.SetDataContext));
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable) this.GetValue(ItemSourceProperty); }
            set { this.SetValue(ItemSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }

        public RelayCommand<object> CSelectItem
        {
            get { return (RelayCommand<object>)this.GetValue(CSelectItemProperty); }
            set { this.SetValue(CSelectItemProperty, value); }
        }

        public RelayCommand<object> CSelectDoubleClikedItem
        {
            get { return (RelayCommand<object>)this.GetValue(CSelectDoubleClikedItemProperty); }
            set { this.SetValue(CSelectDoubleClikedItemProperty, value); }
        }

        public void SetDataContext()
        {
            var frameworkElement = this.Content as FrameworkElement;
            if(frameworkElement != null) { frameworkElement.DataContext = this; }
        }
    }
}