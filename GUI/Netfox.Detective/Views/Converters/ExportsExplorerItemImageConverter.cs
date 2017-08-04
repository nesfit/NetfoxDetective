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
using System.Windows.Data;
using Netfox.Detective.ViewModels.Exports;

namespace Netfox.Detective.Views.Converters
{
    public class ExportsExplorerItemImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!value.GetType().IsAssignableFrom(typeof(ExportsExplorerVm.ExplorerItem.ItemType))) { return null; }

            var type = value is ExportsExplorerVm.ExplorerItem.ItemType? (ExportsExplorerVm.ExplorerItem.ItemType) value : ExportsExplorerVm.ExplorerItem.ItemType.Investigation;


            switch(type)
            {
                case ExportsExplorerVm.ExplorerItem.ItemType.Investigation:
                    return "pack://application:,,,/Netfox.Detective;component/Views/Resources/Icons/db.png";
                case ExportsExplorerVm.ExplorerItem.ItemType.Group:
                    return "pack://application:,,,/Netfox.Detective;component/Views/Resources/Icons/folder.png";
                case ExportsExplorerVm.ExplorerItem.ItemType.Result:
                    return "pack://application:,,,/Netfox.Detective;component/Views/Resources/Icons/notepad_2.png";
                case ExportsExplorerVm.ExplorerItem.ItemType.Data:
                    return "pack://application:,,,/Netfox.Detective;component/Views/Resources/Icons/doc_lines_stright.png";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}