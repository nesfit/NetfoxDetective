﻿// Copyright (c) 2017 Jan Pluskal
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

using Netfox.AnalyzerAppIdent.Interfaces;
using Netfox.Detective.Views;

namespace Netfox.AnalyzerAppIdent.Views
{
    /// <summary>
    /// Interaction logic for AppIdentMainView.xaml
    /// </summary>
    public partial class AppIdentMainView : DetectiveDataEntityPaneViewBase, IAppIdentMainView
    {
        public AppIdentMainView()
        {
            this.InitializeComponent();
        }
    }
}
