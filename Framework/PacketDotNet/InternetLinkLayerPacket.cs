using System.Reflection;
using log4net;

namespace PacketDotNet
{
    /// <summary>
    ///     Internet Link layer packet
    ///     See http://en.wikipedia.org/wiki/Link_Layer
    /// </summary>
    public class InternetLinkLayerPacket : Packet
    {
#if DEBUG
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
#else
    // NOTE: No need to warn about lack of use, the compiler won't
    //       put any calls to 'log' here but we need 'log' to exist to compile
#pragma warning disable 0169, 0649
        private static readonly ILogInactive log;
#pragma warning restore 0169, 0649
#endif

        /// <summary>
        ///     Look for the innermost payload. This method is useful because
        ///     while some packets are LinuxSSL->IpPacket or
        ///     EthernetPacket->IpPacket, there are some packets that are
        ///     EthernetPacket->PPPoEPacket->PPPPacket->IpPacket, and for these cases
        ///     we really want to get to the IpPacket
        /// </summary>
        /// <returns>
        ///     A <see cref="Packet" />
        /// </returns>
        public static Packet GetInnerPayload(InternetLinkLayerPacket packet)
        {
            // is this an ethernet packet?
            if(packet is EthernetPacket)
            {
                log.Debug("packet is EthernetPacket");

                var thePayload = packet.PayloadPacket;

                // is this packets payload a PPPoEPacket? If so,
                // the PPPoEPacket payload should be a PPPPacket and we want
                // the payload of the PPPPpacket
                if(thePayload is PPPoEPacket)
                {
                    log.Debug("thePayload is PPPoEPacket");
                    return thePayload.PayloadPacket.PayloadPacket;
                }

                return thePayload;
            }
            log.Debug("else");
            return packet.PayloadPacket;
        }
    }
}