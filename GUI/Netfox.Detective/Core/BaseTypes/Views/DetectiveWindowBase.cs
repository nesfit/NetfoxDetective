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
            if(vm == null) { return; }
            vm.ViewType = this.GetType();
            vm.View = new WeakReference(this);
        }
    }
}