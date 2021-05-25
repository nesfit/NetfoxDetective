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
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Netfox.Detective.Views.Exports.Converters
{
    public class ImageUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = (string) value;
            if(File.Exists(path))
            {
                try
                {
                    var source = new Uri(Path.GetFullPath(path));
                    return BitmapFrame.Create(source);
                }
                catch(Exception) {
                    return new BitmapImage();
                }

                /*   Uri source;

                

                if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                    source= new Uri(path,UriKind.Absolute);
                else if (Uri.IsWellFormedUriString(path, UriKind.Relative))
                    source = new Uri(path, UriKind.Relative);
                else
                    return new BitmapImage();*/
            }
            return new BitmapImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}