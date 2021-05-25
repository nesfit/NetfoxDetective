// Copyright (c) 2017 Martin Vondracek
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
using Netfox.Framework.Models.PmLib.Frames;
using PacketDotNet;

namespace Netfox.Detective.Views.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Convertor from <see cref="T:Netfox.Framework.Models.PmLib.Frames.PmFrameBase" /> to <see cref="T:System.Windows.Media.SolidColorBrush" /> for coloring frame's background.
    /// </summary>
    /// <remarks>Uses few default Wireshark's coloring rules.</remarks>
    public class PmFrameBaseToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var frame = value as PmFrameBase;
            if(frame == null) { return new SolidColorBrush(); }
            
            // arp
            if(frame.PmPacket.PacketInfo.Extract(typeof(ARPPacket)) != null){return new SolidColorBrush(Color.FromRgb(250,240,215));}
            // icmp||icmpv6
            if(   frame.PmPacket.PacketInfo.Extract(typeof(ICMPv4Packet)) != null
               || frame.PmPacket.PacketInfo.Extract(typeof(ICMPv6Packet)) != null){return new SolidColorBrush(Color.FromRgb(252,224,255));}
            // tcp
            if(frame.PmPacket.PacketInfo.Extract(typeof(TcpPacket)) != null){return new SolidColorBrush(Color.FromRgb(231,230,255));}
            // udp
            if(frame.PmPacket.PacketInfo.Extract(typeof(UdpPacket)) != null){return new SolidColorBrush(Color.FromRgb(218,238,255));}

            return new SolidColorBrush();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}