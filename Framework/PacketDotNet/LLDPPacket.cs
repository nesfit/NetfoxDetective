using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using PacketDotNet.LLDP;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    ///     A LLDP packet.
    ///     As specified in IEEE Std 802.1AB
    /// </summary>
    /// <remarks>
    ///     See http://en.wikipedia.org/wiki/Link_Layer_Discovery_Protocol for general info
    ///     See IETF 802.1AB for the full specification
    /// </remarks>
    public class LLDPPacket : InternetLinkLayerPacket, IEnumerable
    {
        #region Preprocessor Directives
#if DEBUG
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
#else
    // NOTE: No need to warn about lack of use, the compiler won't
    //       put any calls to 'log' here but we need 'log' to exist to compile
#pragma warning disable 0169, 0649
        private static readonly ILogInactive log;
#pragma warning restore 0169, 0649
#endif
        #endregion

        #region Constructors
        /// <summary>
        ///     Create an empty LLDPPacket
        /// </summary>
        public LLDPPacket()
        {
            log.Debug("");

            // all lldp packets end with an EndOfLLDPDU tlv so add one
            // by default
            this.TlvCollection.Add(new EndOfLLDPDU());
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public LLDPPacket(ByteArraySegment bas)
        {
            log.Debug("");

            this.header = new ByteArraySegment(bas);

            // Initiate the TLV list from the existing data
            this.ParseByteArrayIntoTlvs(this.header.Bytes, this.header.Offset);
        }
        #endregion

        #region Properties
        /// <value>
        ///     The current length of the LLDPDU
        /// </value>
        public int Length { get; set; }

        /// <summary>
        ///     LLDPPacket specific implementation of BytesHighPerformance
        ///     Necessary because each TLV in the collection may have a
        ///     byte[] that is not shared by other TLVs
        ///     NOTE: There is potential for the same performance improvement that
        ///     the Packet class uses where we check to see if each TLVs uses the
        ///     same byte[] and that there are no gaps.
        /// </summary>
        public override ByteArraySegment BytesHighPerformance
        {
            get
            {
                var ms = new MemoryStream();
                foreach(var tlv in this.TlvCollection)
                {
                    var tlvBytes = tlv.Bytes;
                    ms.Write(tlvBytes, 0, tlvBytes.Length);
                }

                var offset = 0;
                var msArray = ms.ToArray();
                return new ByteArraySegment(msArray, offset, msArray.Length);
            }
        }

        /// <summary>
        ///     Allows access of the TlvCollection by index
        /// </summary>
        /// <param name="index">The index of the item being set/retrieved in the collection</param>
        /// <returns>The requested TLV</returns>
        public TLV this[int index]
        {
            get { return this.TlvCollection[index]; }
            set { this.TlvCollection[index] = value; }
        }

        /// <summary>
        ///     Enables foreach functionality for this class
        /// </summary>
        /// <returns>The next item in the list</returns>
        public IEnumerator GetEnumerator() { return this.TlvCollection.GetEnumerator(); }
        #endregion

        #region Methods
        /// <summary>
        ///     Parse byte[] into TLVs
        /// </summary>
        public void ParseByteArrayIntoTlvs(byte[] bytes, int offset)
        {
            log.DebugFormat("bytes.Length {0}, offset {1}", bytes.Length, offset);

            var position = 0;

            this.TlvCollection.Clear();

            while(position < bytes.Length)
            {
                // The payload type
                var byteArraySegment = new ByteArraySegment(bytes, offset + position, TLVTypeLength.TypeLengthLength);
                var typeLength = new TLVTypeLength(byteArraySegment);

                // create a TLV based on the type and
                // add it to the collection
                var currentTlv = TLVFactory(bytes, offset + position, typeLength.Type);
                if(currentTlv == null)
                {
                    log.Debug("currentTlv == null");
                    break;
                }

                log.DebugFormat("Adding tlv {0}, Type {1}", currentTlv.GetType(), currentTlv.Type);
                this.TlvCollection.Add(currentTlv);

                // stop at the first end tlv we run into
                if(currentTlv is EndOfLLDPDU) { break; }

                // Increment the position to seek the next TLV
                position += (currentTlv.TotalLength);
            }

            log.DebugFormat("Done, position {0}", position);
        }

        /// <summary>
        /// </summary>
        /// <param name="Bytes">
        ///     A <see cref="System.Byte[]" />
        /// </param>
        /// <param name="offset">
        ///     A <see cref="System.Int32" />
        /// </param>
        /// <param name="type">
        ///     A <see cref="TLVTypes" />
        /// </param>
        /// <returns>
        ///     A <see cref="TLV" />
        /// </returns>
        private static TLV TLVFactory(byte[] Bytes, int offset, TLVTypes type)
        {
            switch(type)
            {
                case TLVTypes.ChassisID:
                    return new ChassisID(Bytes, offset);
                case TLVTypes.PortID:
                    return new PortID(Bytes, offset);
                case TLVTypes.TimeToLive:
                    return new TimeToLive(Bytes, offset);
                case TLVTypes.PortDescription:
                    return new PortDescription(Bytes, offset);
                case TLVTypes.SystemName:
                    return new SystemName(Bytes, offset);
                case TLVTypes.SystemDescription:
                    return new SystemDescription(Bytes, offset);
                case TLVTypes.SystemCapabilities:
                    return new SystemCapabilities(Bytes, offset);
                case TLVTypes.ManagementAddress:
                    return new ManagementAddress(Bytes, offset);
                case TLVTypes.OrganizationSpecific:
                    return new OrganizationSpecific(Bytes, offset);
                case TLVTypes.EndOfLLDPU:
                    return new EndOfLLDPDU(Bytes, offset);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Returns the LLDP inside of the Packet p or null if
        ///     there is no encapsulated packet
        /// </summary>
        /// <param name="p">
        ///     A <see cref="Packet" />
        /// </param>
        /// <returns>
        ///     A <see cref="IpPacket" />
        /// </returns>
        [Obsolete("Use Packet.Extract() instead")]
        public static LLDPPacket GetEncapsulated(Packet p)
        {
            log.Debug("");

            if(p is InternetLinkLayerPacket)
            {
                var payload = GetInnerPayload((InternetLinkLayerPacket) p);
                if(payload is LLDPPacket) { return (LLDPPacket) payload; }
            }

            return null;
        }

        /// <summary>
        ///     Create a randomized LLDP packet with some basic TLVs
        /// </summary>
        /// <returns>
        ///     A <see cref="Packet" />
        /// </returns>
        public static LLDPPacket RandomPacket()
        {
            var rnd = new Random();

            var lldpPacket = new LLDPPacket();

            var physicalAddressBytes = new byte[EthernetFields.MacAddressLength];
            rnd.NextBytes(physicalAddressBytes);
            var physicalAddress = new PhysicalAddress(physicalAddressBytes);
            lldpPacket.TlvCollection.Add(new ChassisID(physicalAddress));

            var networkAddress = new byte[IPv4Fields.AddressLength];
            rnd.NextBytes(networkAddress);
            lldpPacket.TlvCollection.Add(new PortID(new NetworkAddress(new IPAddress(networkAddress))));

            var seconds = (ushort) rnd.Next(0, 120);
            lldpPacket.TlvCollection.Add(new TimeToLive(seconds));

            lldpPacket.TlvCollection.Add(new EndOfLLDPDU());

            return lldpPacket;
        }

        /// <summary cref="Packet.ToString(StringOutputType)" />
        public override string ToString(StringOutputType outputFormat)
        {
            var buffer = new StringBuilder();
            var color = "";
            var colorEscape = "";

            if(outputFormat == StringOutputType.Colored || outputFormat == StringOutputType.VerboseColored)
            {
                color = this.Color;
                colorEscape = AnsiEscapeSequences.Reset;
            }

            if(outputFormat == StringOutputType.Normal || outputFormat == StringOutputType.Colored)
            {
                // build the string of tlvs
                var tlvs = "{";
                var r = new Regex(@"[^(\.)]([^\.]*)$");
                foreach(var tlv in this.TlvCollection)
                {
                    // regex trim the parent namespaces from the class type
                    //   (ex. "PacketDotNet.LLDP.TimeToLive" becomes "TimeToLive")
                    var m = r.Match(tlv.GetType().ToString());
                    tlvs += m.Groups[0].Value + "|";
                }
                tlvs = tlvs.TrimEnd('|');
                tlvs += "}";

                // build the output string
                buffer.AppendFormat("{0}[LLDPPacket: TLVs={2}]{1}", color, colorEscape, tlvs);
            }

            if(outputFormat == StringOutputType.Verbose || outputFormat == StringOutputType.VerboseColored)
            {
                // build the output string
                buffer.AppendLine("LLDP:  ******* LLDP - \"Link Layer Discovery Protocol\" - offset=? length=" + this.TotalPacketLength);
                buffer.AppendLine("LLDP:");
                foreach(var tlv in this.TlvCollection) { buffer.AppendLine("LLDP:" + tlv); }
                buffer.AppendLine("LLDP:");
            }

            // append the base string output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }
        #endregion

        #region Members
        /// <summary>
        ///     Contains the TLV's in the LLDPDU
        /// </summary>
        public TLVCollection TlvCollection = new TLVCollection();
        #endregion
    }
}