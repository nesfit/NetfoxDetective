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
using Netfox.Core.Models.Exports;

namespace Netfox.Detective.Views.Exports.Converters
{
    public class ExportReportLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is ExportReport.ReportLevel)) { return new SolidColorBrush(Colors.Black); }
            ;

            var reportLevel = (ExportReport.ReportLevel) value;

            switch(reportLevel)
            {
                case ExportReport.ReportLevel.Debug:
                    return new SolidColorBrush(Colors.Gray);
                case ExportReport.ReportLevel.Info:
                    return new SolidColorBrush(Colors.Black);
                case ExportReport.ReportLevel.Warn:
                    return new SolidColorBrush(Colors.DarkOrange);
                case ExportReport.ReportLevel.Error:
                    return new SolidColorBrush(Colors.Red);
                case ExportReport.ReportLevel.Fatal:
                    return new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(Colors.Black);
            ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}