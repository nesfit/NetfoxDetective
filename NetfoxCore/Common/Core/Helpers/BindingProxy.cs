﻿using System.Windows;

namespace Netfox.Core.Helpers
{
    public class BindingProxy : Freezable
    {
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        public object Data
        {
            get { return this.GetValue(DataProperty); }
            set { this.SetValue(DataProperty, value); }
        }

        #region Overrides of Freezable
        protected override Freezable CreateInstanceCore() { return new BindingProxy(); }
        #endregion
    }
}