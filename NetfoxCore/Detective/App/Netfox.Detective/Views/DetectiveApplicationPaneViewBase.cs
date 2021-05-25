﻿// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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

using System.ComponentModel;

namespace Netfox.Detective.Views
{
    /// <summary>
    ///     All Applications Panes should be derived from this class.
    /// </summary>
    public abstract class DetectiveApplicationPaneViewBase : DetectivePaneViewBase
    {
        public DetectiveApplicationPaneViewBase()
        {
            // this.DataContextChanged += (_, e) =>
            // {
            //     if (e.OldValue is INotifyPropertyChanged old)
            //         old.PropertyChanged -= RefreshView;
            //     if (e.NewValue is INotifyPropertyChanged vm)
            //         vm.PropertyChanged += RefreshView;
            // };
        }

        protected virtual void RefreshView(object s, PropertyChangedEventArgs e){}
    }
}