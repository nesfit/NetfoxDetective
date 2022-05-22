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
using Netfox.Core.Collections;
using Netfox.Detective.Models.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Detective.Views.Converters
{
    public class ExportsCollectionContainer : CollectionContainer
    {
        public bool IsGroup { get; set; }
        public bool IsResult { get; set; }
    }

    public class ExportsMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = new CompositeCollection();

            for(var i = 0; i < values.Length; i++)
            {
                if(values[i].GetType() == typeof(ViewModelVirtualizingIoCObservableCollection<ExportGroupVm, ExportGroup>))
                {
                    var nc = new ExportsCollectionContainer
                    {
                        Collection = (ViewModelVirtualizingIoCObservableCollection<ExportGroupVm, ExportGroup>) values[i],
                        IsGroup = true
                    };
                    result.Add(nc);
                }
                else if(values[i].GetType() == typeof(ViewModelVirtualizingIoCObservableCollection<ExportVm, SnooperExportBase>))
                {
                    var nc = new ExportsCollectionContainer
                    {
                        Collection = (ViewModelVirtualizingIoCObservableCollection<ExportVm, SnooperExportBase>) values[i],
                        IsResult = true
                    };
                    result.Add(nc);
                }
            }


            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot perform reverse-conversion");
        }
    }
}