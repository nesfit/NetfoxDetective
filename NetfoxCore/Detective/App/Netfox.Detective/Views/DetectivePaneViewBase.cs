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

using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Netfox.Detective.Views
{
    /// <summary>
    ///     Base class of all PaneViews, only to be derived by another base classes - ApplicationPanes, ExportPanes, etc.
    ///     Derived from UserControl insted of RadPane because this is a RadPane content!.
    /// </summary>
    public abstract class DetectivePaneViewBase : UserControl
    {
        protected DetectivePaneViewBase() { this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag); }
    }
}