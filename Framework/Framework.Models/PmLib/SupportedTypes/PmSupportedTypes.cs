/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
 * Author(s):
 * Vladimir Vesely (mailto:ivesely@fit.vutbr.cz)
 * Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
 * Jan Plusal (mailto:xplusk03@stud.fit.vutbr.cz)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using PacketDotNet;

namespace Netfox.Framework.Models.PmLib.SupportedTypes
{
    /// <summary> Class providing definition of supported file and link layer types.</summary>
    public static class PmSupportedTypes
    {
        /// <summary> Convert link type to packetdot net.</summary>
        /// <param name="typ"> The typ. </param>
        /// <returns> The link converted type to packetdot net.</returns>
        public static LinkLayers ConvertLinkTypeToPacketdotNet(PmLinkType typ)
        {
            switch(typ)
            {
                case PmLinkType.Ieee80211:
                    return LinkLayers.Ieee80211;
                case PmLinkType.Raw:
                    return LinkLayers.Raw;
                case PmLinkType.Fddi:
                    return LinkLayers.Fddi;
                case PmLinkType.AtmRfc1483:
                    return LinkLayers.AtmRfc1483;
// ReSharper disable RedundantCaseLabel
                case PmLinkType.Ethernet:
// ReSharper restore RedundantCaseLabel
                default:
                    return LinkLayers.Ethernet;
            }
        }

        /// <summary> Pragma function reeturning if file type is supproted by this app.</summary>
        /// <param name="pmLinkType"> CaptureProcessor file type. </param>
        /// <returns> If type is supported then returns true otherwise false.</returns>
        public static Boolean IsSupportedLinkType(PmLinkType pmLinkType)
        {
            switch(pmLinkType)
            {
                case PmLinkType.Ethernet:
                case PmLinkType.Fddi:
                case PmLinkType.Raw:
                case PmLinkType.Ieee80211:
                case PmLinkType.AtmRfc1483:
                    return true;
                default:
                    return false;
            }
        }
    }
}