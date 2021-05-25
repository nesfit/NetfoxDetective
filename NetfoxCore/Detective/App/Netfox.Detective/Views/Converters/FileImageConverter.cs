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

namespace Netfox.Detective.Views.Converters
{
    public class FileImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null) { return new BitmapImage(); }

            var fileName = value as string;
            string iconFileName;

            var ext = Path.GetExtension(fileName);

            switch(ext)
            {
                case ".doc":
                case ".docx":
                    iconFileName = "doc.png";
                    break;
                case ".gif":
                    iconFileName = "gif.png";
                    break;
                case ".jpeg":
                case ".jpg":
                    iconFileName = "jpg.png";
                    break;
                case ".pdf":
                    iconFileName = "pdf.png";
                    break;
                case ".png":
                    iconFileName = "png.png";
                    break;
                case ".rar":
                    iconFileName = "rar.png";
                    break;
                case ".rtf":
                    iconFileName = "rtf.png";
                    break;
                case ".riff":
                    iconFileName = "tiff.png";
                    break;
                case ".txt":
                    iconFileName = "txt.png";
                    break;
                case ".zip":
                    iconFileName = "zip.png";
                    break;
                default:
                    iconFileName = "_blank.png";
                    break;
            }

            return new BitmapImage(new Uri(@"pack://application:,,,/Netfox.Detective;component/Views/Resources/Files/" + iconFileName));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}