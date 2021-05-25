

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(this.SetDataContext));
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