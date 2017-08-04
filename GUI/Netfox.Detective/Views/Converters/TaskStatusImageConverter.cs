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
using System.Windows.Media.Imaging;
using Netfox.Core.Interfaces;

namespace Netfox.Detective.Views.Converters
{
    public class TaskStatusImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null) { return new BitmapImage(); }

            var taskState = value as TaskState? ?? TaskState.Ready;

            var iconFileName = "running.png";

            if(taskState == TaskState.Running) { iconFileName = "running.png"; }
            if(taskState == TaskState.DoneOk) { iconFileName = "ok.png"; }
            if(taskState == TaskState.DoneError) { iconFileName = "error.png"; }

            return new BitmapImage(new Uri(@"pack://application:,,,/Netfox.Detective;component/Views/Resources/Tasks/" + iconFileName));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}