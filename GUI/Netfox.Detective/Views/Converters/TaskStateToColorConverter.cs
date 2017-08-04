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
using Netfox.Core.Interfaces;

namespace Netfox.Detective.Views.Converters
{
    public class TaskStateToColorConverter : IValueConverter
    {
        public static Brush unknownBrush = new SolidColorBrush(Colors.Gainsboro);
        public static Brush activeBrush = new SolidColorBrush(Colors.Black);
        public static Brush doneBrush = new SolidColorBrush(Colors.Gray);
        public static Brush doneErrorBrush = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var taskState = value as TaskState? ?? TaskState.Ready;

            if(taskState == TaskState.Running) { return activeBrush; }
            if(taskState == TaskState.DoneOk) { return doneBrush; }
            if(taskState == TaskState.DoneError) { return doneErrorBrush; }

            return unknownBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}