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
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Netfox.Detective.Views.Converters
{
    public class BoolAndBoolToBlueAndRedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values.Count() != 2) { return new SolidColorBrush(Colors.Black); }
            var primaryVal = values[0] is bool && (bool) values[0];
            var secondaryVal = values[1] is bool && (bool) values[1];

            if(primaryVal && secondaryVal) { return new SolidColorBrush(Colors.Blue); }
            if(primaryVal && !secondaryVal) { return new SolidColorBrush(Colors.Blue); }
            if(!primaryVal && secondaryVal) { return new SolidColorBrush(Colors.Red); }
            if(!primaryVal && !secondaryVal) { return new SolidColorBrush(Colors.Black); }

            //if (primaryVal)
            //    return new SolidColorBrush(Colors.Blue);
            //else if (secondaryVal)
            //    return new SolidColorBrush(Colors.Black);
            //else
            //    return new SolidColorBrush(Colors.Red);
            return new SolidColorBrush(Colors.Black);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) { return null; }
    }
}