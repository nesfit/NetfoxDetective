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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Linq;
using Netfox.Core.Helpers;

namespace Netfox.Detective.Views.Converters
{
    public class ObjectToStringDictionaryPropertiesMap : MarkupExtension, IValueConverter
    {
        /// <summary>
        ///     http://stackoverflow.com/questions/15003827/async-implementation-of-ivalueconverter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _dict = new Dictionary<string, string>();
            if(value == null) { return _dict; }
            try
            {
                var task = Task.Run(() =>
                {
                    var props = value.GetType().GetProperties();
                    foreach(var prop in props)
                    {
                        var propName = prop.Name;
                        object propVal = string.Empty;
                        try
                        {
                            propVal = prop.GetValue(value);
                            if(!(propVal is string) && propVal is IEnumerable<object> collection)
                            {
                                propVal = string.Join($", {Environment.NewLine}", collection.Select(i => i.ToString()));
                            }
                        }
                        catch(Exception) {
                            propVal = string.Empty;
                        }
                        finally { _dict.Add(propName, propVal?.ToString()); }
                    }
                    return _dict;
                });
                return new NotifyTaskCompletion<Dictionary<string, string>>(task);
            }
            catch(Exception) {}

            return _dict;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }

        public override object ProvideValue(IServiceProvider serviceProvider) { return this; }
    }
}