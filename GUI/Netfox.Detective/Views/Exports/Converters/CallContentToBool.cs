using System;
using System.Globalization;
using System.Windows.Data;

namespace Netfox.Detective.Views.Exports.Converters
{
    public class CallContentToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (!value.GetType().IsAssignableFrom(typeof(Array)))

            if(value == null || !value.GetType().IsAssignableFrom(typeof(VoIpExport))) { return false; }

            var export = value as VoIpExport;
            if(export == null) { return false; }

            // for RTP
            if(export.Protocol == VoIpExport.VoIPProto.RTPRTPC && export.CallStream != null)
            {
                // something was exported
                if(export.CallStream.WavPath != string.Empty) { return true; }
            }

            if(export.Call == null) { return false; }

            if(export.Call.CallContents == null) { return false; }
            return (export.Call.CallContents.Length > 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}