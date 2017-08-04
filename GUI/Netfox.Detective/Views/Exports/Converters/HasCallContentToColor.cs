using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Netfox.Detective.Views.Exports.Converters
{
    public class HasCallContentToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null || !value.GetType().IsAssignableFrom(typeof(VoIpExport)))
            {
                //return new SolidColorBrush(Colors.Red);
                return new SolidColorBrush(Colors.Black);
            }
            var export = value as VoIpExport;
            if(export == null || export.Call == null) { return new SolidColorBrush(Colors.Black); }

            //var _data = _export.Call;
            return (export.Call.CallContents.Length > 0)? new SolidColorBrush(Colors.DarkOliveGreen) : new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}