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

using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;

namespace Netfox.Detective.Views.Windows
{
    public class DockingSaveLoadLayoutHelper : DefaultSaveLoadLayoutHelper
    {
        protected override void ElementLoadedOverride(string serializationTag, DependencyObject element)
        {
            base.ElementLoadedOverride(serializationTag, element);
            var document = element as RadDocumentPane;
            if(document != null)
            {
                // Restore the content for the loaded document.
            }
        }
    }
}