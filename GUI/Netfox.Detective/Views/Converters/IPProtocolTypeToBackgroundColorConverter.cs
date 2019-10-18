using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PacketDotNet;

namespace Netfox.Detective.Views.Converters
{
    /// <inheritdoc />
    /// <summary>
    ///     Convertor from <see cref="T:PacketDotNet.IPProtocolType" /> to
    ///     <see cref="T:System.Windows.Media.SolidColorBrush" /> for coloring frame's background.
    /// </summary>
    /// <remarks>Uses few default Wireshark's coloring rules.</remarks>
    public class IPProtocolTypeToBackgroundColorConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if(value == null) return new SolidColorBrush();

            var protocolType = (IPProtocolType) value;

            // icmp||icmpv6
            if(protocolType == IPProtocolType.ICMP || protocolType == IPProtocolType.ICMPV6) return new SolidColorBrush(Color.FromRgb(252, 224, 255));
            // tcp
            if(protocolType == IPProtocolType.TCP) return new SolidColorBrush(Color.FromRgb(231, 230, 255));
            // udp
            if(protocolType == IPProtocolType.UDP) return new SolidColorBrush(Color.FromRgb(218, 238, 255));

            return new SolidColorBrush();
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) { return null; }
    }
}