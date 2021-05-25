using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.ViewModels;
using Telerik.Windows.Controls;

namespace Netfox.Detective.Core.BaseTypes.Views
{
    /// <summary>
    ///     DetectiveWindowBase class all windows in application must be derivate from this
    /// </summary>
    public abstract class DetectiveWindowBase : RadWindow, IDetectiveView
    {
        protected DetectiveWindowBase()
        {
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            this.DataContextChanged += this.DetectiveWindowBase_DataContextChanged;
        }

        private void DetectiveWindowBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = this.DataContext as DetectiveWindowViewModelBase;
            if (vm == null)
            {
                return;
            }

            vm.ViewType = this.GetType();
            vm.View = new WeakReference(this);
        }
    }
}