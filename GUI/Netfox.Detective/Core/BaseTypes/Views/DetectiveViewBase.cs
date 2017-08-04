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
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Netfox.Core.Interfaces.Views;

namespace Netfox.Detective.Core.BaseTypes.Views
{
    public enum DetectivePaneDockingPosition
    {
        None,
        Floating,
        Main,
        Explorer,
        Output,
        Entity,
        Plugin
    }

    [Obsolete]
    public abstract class DetectiveViewBase : UserControl, IDetectiveView
    {
        private const DetectivePaneDockingPosition _defaultDockingPositionStatic = DetectivePaneDockingPosition.None;
        private Visibility _controlVisible = Visibility.Collapsed;
        private bool _isHidden;
        private string _viewName;
        protected DetectiveViewBase() { this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag); }

        public string ViewName
        {
            get { return this._viewName; }
            set
            {
                this._viewName = value;
                this.OnPropertyChanged();
            }
        }

        [Obsolete]
        public Visibility ControlVisible
        {
            get { return this._controlVisible; }
            set
            {
                this._controlVisible = value;
                this.OnPropertyChanged();
            }
        }

        [Obsolete]
        public bool IsHidden
        {
            get { return this._isHidden; }
            set
            {
                this._isHidden = value;
                this.OnPropertyChanged();
            }
        }

        public virtual DetectivePaneDockingPosition DefaultDockingPosition
        {
            get { return DefaultDockingPositionStatic; }
        }

        public static DetectivePaneDockingPosition DefaultDockingPositionStatic
        {
            get { return _defaultDockingPositionStatic; }
        }

        public bool ApplicationContextNeeded { get; protected set; }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if(handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }
        #endregion
    }
}