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
using System.Windows.Media;

namespace Netfox.Detective.Views.Converters
{
    public class ExporterTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var exporterTypeName = value as string;
            if(exporterTypeName == null) { return new SolidColorBrush(Colors.Gray); }

            switch(exporterTypeName.ToUpper())
            {
                case "HTTPSLEUTH":
                    return new SolidColorBrush(Colors.Firebrick);
                case "SMTPSLEUTH":
                    return new SolidColorBrush(Colors.DodgerBlue);
                case "IMAPSLEUTH":
                    return new SolidColorBrush(Colors.DodgerBlue);
                case "POP3SLEUTH":
                    return new SolidColorBrush(Colors.DodgerBlue);
                case "XAMPSLEUTH":
                    return new SolidColorBrush(Colors.Green);
                case "YMSGSLEUTH":
                    return new SolidColorBrush(Colors.Green);
                case "MSNSLEUTH":
                    return new SolidColorBrush(Colors.Green);
                case "ICQSLEUTH":
                    return new SolidColorBrush(Colors.Green);


                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}