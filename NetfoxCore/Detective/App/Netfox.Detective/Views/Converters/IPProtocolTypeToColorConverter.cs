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
using PacketDotNet;

namespace Netfox.Detective.Views.Converters
{
    public class IPProtocolTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var protocolType = value as IPProtocolType? ?? IPProtocolType.NONE;
            
            switch(protocolType)
            {
                case IPProtocolType.IP:
                    return new SolidColorBrush(Colors.Teal);
                case IPProtocolType.TCP:
                    return new SolidColorBrush(Colors.DarkGreen);
                case IPProtocolType.UDP:
                    return new SolidColorBrush(Colors.DarkBlue);
                case IPProtocolType.IPV6:
                    return new SolidColorBrush(Colors.GreenYellow);
                case IPProtocolType.NONE:
                    return new SolidColorBrush(Colors.Black);
                case IPProtocolType.ICMP:
                case IPProtocolType.IGMP:
                case IPProtocolType.IPIP:
                case IPProtocolType.EGP:
                case IPProtocolType.PUP:
                case IPProtocolType.IDP:
                case IPProtocolType.TP:
                case IPProtocolType.ROUTING:
                case IPProtocolType.FRAGMENT:
                case IPProtocolType.RSVP:
                case IPProtocolType.GRE:
                case IPProtocolType.ESP:
                case IPProtocolType.AH:
                case IPProtocolType.ICMPV6:
                case IPProtocolType.DSTOPTS:
                case IPProtocolType.MTP:
                case IPProtocolType.ENCAP:
                case IPProtocolType.PIM:
                case IPProtocolType.COMP:
                case IPProtocolType.RAW:
                default:
                    return new SolidColorBrush(Colors.Red);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}