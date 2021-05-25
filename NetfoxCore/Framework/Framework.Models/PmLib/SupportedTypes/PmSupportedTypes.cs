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
            switch (typ)
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
            switch (pmLinkType)
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