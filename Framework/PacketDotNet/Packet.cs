using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using PacketDotNet.Ieee80211.Ieee80211;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    ///     Base class for all packet types.
    ///     Defines helper methods and accessors for the architecture that underlies how
    ///     packets interact and store their data.
    /// </summary>
    public abstract class Packet
    {
#if DEBUG
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
#else
    // NOTE: No need to warn about lack of use, the compiler won't
    //       put any calls to 'log' here but we need 'log' to exist to compile
#pragma warning disable 0169, 0649
        private static readonly ILogInactive Log;
#pragma warning restore 0169, 0649
#endif

        /// <summary>
        ///     Used internally when building new packet dissectors
        /// </summary>
        protected ByteArraySegment header;

        /// <summary>
        ///     Used internally when building new packet dissectors
        /// </summary>
        protected PacketOrByteArraySegment payloadPacketOrData = new PacketOrByteArraySegment();

        /// <summary>
        ///     The parent packet. Accessible via the 'ParentPacket' property
        /// </summary>
        private Packet _parentPacket;

        /// <summary>
        ///     Gets the total length of the packet.
        ///     Recursively finds the length of this packet and all of the packets
        ///     encapsulated by this packet
        /// </summary>
        /// <value>
        ///     The total length of the packet.
        /// </value>
        protected int TotalPacketLength
        {
            get
            {
                var totalLength = 0;
                totalLength += this.header.Length;

                if(this.payloadPacketOrData.Type == PayloadType.Bytes) {
                    totalLength += this.payloadPacketOrData.TheByteArraySegment.Length;
                }
                else if(this.payloadPacketOrData.Type == PayloadType.Packet) { totalLength += this.payloadPacketOrData.ThePacket.TotalPacketLength; }

                return totalLength;
            }
        }

        /// <value>
        ///     Returns true if we already have a contiguous byte[] in either
        ///     of these conditions:
        ///     - This packet's header byte[] and payload byte[] are the same instance
        ///     or
        ///     - This packet's header byte[] and this packet's payload packet
        ///     are the same instance and the offsets indicate that the bytes
        ///     are contiguous
        /// </value>
        protected bool SharesMemoryWithSubPackets
        {
            get
            {
                Log.Debug("");

                switch(this.payloadPacketOrData.Type)
                {
                    case PayloadType.Bytes:
                        // is the byte array payload the same byte[] and does the offset indicate
                        // that the bytes are contiguous?
                        if((this.header.Bytes == this.payloadPacketOrData.TheByteArraySegment.Bytes)
                           && ((this.header.Offset + this.header.Length) == this.payloadPacketOrData.TheByteArraySegment.Offset))
                        {
                            Log.Debug("PayloadType.Bytes returning true");
                            return true;
                        }
                        Log.Debug("PayloadType.Bytes returning false");
                        return false;
                    case PayloadType.Packet:
                        // is the byte array payload the same as the payload packet header and does
                        // the offset indicate that the bytes are contiguous?
                        if((this.header.Bytes == this.payloadPacketOrData.ThePacket.header.Bytes)
                           && ((this.header.Offset + this.header.Length) == this.payloadPacketOrData.ThePacket.header.Offset))
                        {
                            // and does the sub packet share memory with its sub packets?
                            var retval = this.payloadPacketOrData.ThePacket.SharesMemoryWithSubPackets;
                            Log.DebugFormat("PayloadType.Packet retval {0}", retval);
                            return retval;
                        }
                        Log.Debug("PayloadType.Packet returning false");
                        return false;
                    case PayloadType.None:
                        // no payload data or packet thus we must share memory with
                        // our non-existent sub packets
                        Log.Debug("PayloadType.None, returning true");
                        return true;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        ///     The packet that is carrying this one
        /// </summary>
        public virtual Packet ParentPacket
        {
            get { return this._parentPacket; }
            set { this._parentPacket = value; }
        }

        /// <value>
        ///     Returns a
        /// </value>
        public virtual byte[] Header
        {
            get { return this.header.ActualBytes(); }
        }

        /// <summary>
        ///     Packet that this packet carries if one is present.
        ///     Note that the packet MAY have a null PayloadPacket but
        ///     a non-null PayloadData
        /// </summary>
        public virtual Packet PayloadPacket
        {
            get { return this.payloadPacketOrData.ThePacket; }
            set
            {
                // TODO BUG: Check that packet cannot have any its parent as its payload.
                if(this == value) { throw new InvalidOperationException("A packet cannot have itself as its payload."); }

                this.payloadPacketOrData.ThePacket = value;
                this.payloadPacketOrData.ThePacket.ParentPacket = this;
            }
        }

        /// <summary>
        ///     Payload byte[] if one is present.
        ///     Note that the packet MAY have a null PayloadData but a
        ///     non-null PayloadPacket
        /// </summary>
        public byte[] PayloadData
        {
            get
            {
                if(this.payloadPacketOrData.TheByteArraySegment == null)
                {
                    Log.Debug("returning null");
                    return null;
                }
                var retval = this.payloadPacketOrData.TheByteArraySegment.ActualBytes();
                Log.DebugFormat("retval.Length: {0}", retval.Length);
                return retval;
            }

            set
            {
                Log.DebugFormat("value.Length {0}", value.Length);

                this.payloadPacketOrData.TheByteArraySegment = new ByteArraySegment(value, 0, value.Length);
            }
        }

        /// <summary>
        ///     byte[] containing this packet and its payload
        ///     NOTE: Use 'public virtual ByteArraySegment BytesHighPerformance' for highest performance
        /// </summary>
        public virtual byte[] Bytes
        {
            get
            {
                Log.Debug("");

                // Retrieve the byte array container
                var ba = this.BytesHighPerformance;

                // ActualBytes() will copy bytes if necessary but will avoid a copy in the
                // case where our offset is zero and the byte[] length matches the
                // encapsulated Length
                return ba.ActualBytes();
            }
        }

        /// <value>
        ///     The option to return a ByteArraySegment means that this method
        ///     is higher performance as the data can start at an offset other than
        ///     the first byte.
        /// </value>
        public virtual ByteArraySegment BytesHighPerformance
        {
            get
            {
                Log.Debug("");

                // ensure calculated values are properly updated
                this.RecursivelyUpdateCalculatedValues();

                // if we share memory with all of our sub packets we can take a
                // higher performance path to retrieve the bytes
                if(this.SharesMemoryWithSubPackets)
                {
                    // The high performance path that is often taken because it is called on
                    // packets that have not had their header, or any of their sub packets, resized
                    var newByteArraySegment = new ByteArraySegment(this.header.Bytes, this.header.Offset, this.header.BytesLength - this.header.Offset);
                    Log.DebugFormat("SharesMemoryWithSubPackets, returning byte array {0}", newByteArraySegment);
                    return newByteArraySegment;
                }
                Log.Debug("rebuilding the byte array");

                var ms = new MemoryStream();

                // TODO: not sure if this is a performance gain or if
                //       the compiler is smart enough to not call the get accessor for Header
                //       twice, once when retrieving the header and again when retrieving the Length
                var theHeader = this.Header;
                ms.Write(theHeader, 0, theHeader.Length);

                this.payloadPacketOrData.AppendToMemoryStream(ms);

                var newBytes = ms.ToArray();

                return new ByteArraySegment(newBytes, 0, newBytes.Length);
            }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
// ReSharper disable EmptyConstructor
        public Packet()
// ReSharper restore EmptyConstructor
        {}

        private static object Lock = new object();

        /// <summary>
        ///     Parse bytes into a packet
        /// </summary>
        /// <param name="linkLayer">
        ///     A <see cref="LinkLayers" />
        /// </param>
        /// <param name="packetData">
        ///     A <see cref="System.Byte" />
        /// </param>
        /// <returns>
        ///     A <see cref="Packet" />
        /// </returns>
        public static Packet ParsePacket(LinkLayers linkLayer, byte[] packetData)
        {
            Packet p;
            var bas = new ByteArraySegment(packetData);

            Log.DebugFormat("LinkLayer {0}", linkLayer);

            switch(linkLayer)
            {
                case LinkLayers.Ethernet:
                    p = new EthernetPacket(bas);
                    break;
                case LinkLayers.LinuxSLL:
                    p = new LinuxSLLPacket(bas);
                    break;
                case LinkLayers.Ppp:
                    p = new PPPPacket(bas);
                    break;
                case LinkLayers.Ieee80211:
                    p = MacFrame.ParsePacket(bas);
                    break;
                case LinkLayers.Ieee80211_Radio:
                    p = new RadioPacket(bas);
                    break;
                case LinkLayers.PerPacketInformation:
                    p = new PpiPacket(bas);
                    break;
                //http://sourceforge.net/p/packetnet/patches/1/
                case LinkLayers.Raw:
                    var ipVer = (packetData[0]&0xf0) >> 4;
                    p = (ipVer == 4)? new IPv4Packet(bas) : new IPv6Packet(bas) as Packet;
                    break;
                default:
                    throw new NotImplementedException("LinkLayer of " + linkLayer + " is not implemented");
            }

            return p;
        }

        /// <summary>
        ///     Used to ensure that values like checksums and lengths are
        ///     properly updated
        /// </summary>
        protected void RecursivelyUpdateCalculatedValues()
        {
            // call the possibly overridden method
            this.UpdateCalculatedValues();

            // if the packet contains another packet, call its
            if(this.payloadPacketOrData.Type == PayloadType.Packet) { this.payloadPacketOrData.ThePacket.RecursivelyUpdateCalculatedValues(); }
        }

        /// <summary>
        ///     Called to ensure that calculated values are updated before
        ///     the packet bytes are retrieved
        ///     Classes should override this method to update things like
        ///     checksums and lengths that take too much time or are too complex
        ///     to update for each packet parameter change
        /// </summary>
        public virtual void UpdateCalculatedValues() {}

        /// <summary>Output this packet as a readable string</summary>
        public override String ToString()
        {
            return this.ToString(StringOutputType.Normal);
        }

        /// <summary cref="Packet.ToString()">
        ///     Output the packet information in the specified format
        ///     Normal - outputs the packet info to a single line
        ///     Colored - outputs the packet info to a single line with coloring
        ///     Verbose - outputs detailed info about the packet
        ///     VerboseColored - outputs detailed info about the packet with coloring
        /// </summary>
        /// <param name="outputFormat">
        ///     <see cref="StringOutputType" />
        /// </param>
        public virtual string ToString(StringOutputType outputFormat)
        {
            return this.payloadPacketOrData.Type == PayloadType.Packet? this.payloadPacketOrData.ThePacket.ToString(outputFormat) : String.Empty;
        }

        /// <summary>
        ///     Prints the Packet PayloadData in Hex format
        ///     With the 16-byte segment number, raw bytes, and parsed ascii output
        ///     Ex:
        ///     0010  00 18 82 6c 7c 7f 00 c0  9f 77 a3 b0 88 64 11 00   ...1|... .w...d..
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" />
        /// </returns>
        public string PrintHex()
        {
            var data = this.BytesHighPerformance.Bytes;
            var buffer = new StringBuilder();
// ReSharper disable TooWideLocalVariableScope
            var segmentNumber = "";
// ReSharper restore TooWideLocalVariableScope
            var bytes = "";
            var ascii = "";

            buffer.AppendLine("Data:  ******* Raw Hex Output - length=" + data.Length + " bytes");
            buffer.AppendLine("Data: Segment:                   Bytes:                              Ascii:");
            buffer.AppendLine("Data: --------------------------------------------------------------------------");

            // parse the raw data
            for(var i = 1; i <= data.Length; i++)
            {
                // add the current byte to the bytes hex string
                bytes += (data[i - 1].ToString("x")).PadLeft(2, '0') + " ";

                // add the current byte to the asciiBytes array for later processing
                if(data[i - 1] < 0x21 || data[i - 1] > 0x7e) {
                    ascii += ".";
                }
                else
                {
                    ascii += Encoding.ASCII.GetString(new[]
                    {
                        data[i - 1]
                    });
                }

                // add an additional space to split the bytes into
                //  two groups of 8 bytes
                if(i%16 != 0 && i%8 == 0)
                {
                    bytes += " ";
                    ascii += " ";
                }

                // append the output string
                if(i%16 == 0)
                {
                    // add the 16 byte segment number
                    segmentNumber = ((((i - 16)/16)*10).ToString(CultureInfo.InvariantCulture)).PadLeft(4, '0');

                    // build the line
                    buffer.AppendLine("Data: " + segmentNumber + "  " + bytes + "  " + ascii);

                    // reset for the next line
                    bytes = "";
                    ascii = "";

                    continue;
                }

                // handle the last pass
                if(i == data.Length)
                {
                    // add the 16 byte segment number
                    segmentNumber = (((((i - 16)/16) + 1)*10).ToString(CultureInfo.InvariantCulture)).PadLeft(4, '0');

                    // build the line
                    buffer.AppendLine("Data: " + (segmentNumber).PadLeft(4, '0') + "  " + bytes.PadRight(49, ' ') + "  " + ascii);
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        ///     Extract a packet of a specific type or null if a packet of the given type isn't found
        ///     NOTE: a 'dynamic' return type is possible here but costs ~7.8% in performance
        /// </summary>
        /// <param name='type'>
        ///     Type.
        /// </param>
        public Packet Extract(Type type)
        {
            var p = this;

            // search for a packet type that matches the given one
            do
            {
                if(type.IsInstanceOfType(p)) { return p; }

                // move to the PayloadPacket
                p = p.PayloadPacket;
            } while(p != null);

            return null;
        }

        /// <value>
        ///     Color used when generating the text description of a packet
        /// </value>
        public virtual String Color
        {
            get { return AnsiEscapeSequences.Black; }
        }
    }
}