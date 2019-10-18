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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Netfox.Core.Database;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.SupportedTypes;
using PacketDotNet;
using PostSharp.Patterns.Model;

namespace Netfox.Framework.Models.PmLib.Frames
{
    [Serializable]
    [KnownType(typeof(PmFramePcap))]
    [KnownType(typeof(PmFrameMnm))]
    [KnownType(typeof(PmFramePcapNg))]
    [KnownType(typeof(PmFrameVirtual))]
    [DataContract]
    [Persistent]
    public abstract class PmFrameBase : IOrdereableEntity
    {
        [NonSerialized] public static readonly IPEndPoint NullEndPoint = new IPEndPoint(0, 0);

        /// <summary>
        ///     Constructor when adding new frame that has no L2 and above information filles
        /// </summary>
        protected PmFrameBase(PmCaptureBase pmCapture, PmLinkType pmLinkType, DateTime timeStamp, Int64 incLength, PmFrameType frameType, long frameIndex, long originalLength)
        {
            this.PmCapture = pmCapture;
            this.PmLinkType = pmLinkType;
            this.TimeStamp = timeStamp;
            this.IncludedLength = incLength;
            this.PmFrameType = frameType;
            this.FrameIndex = frameIndex;
            this.OriginalLength = originalLength;

            this.PmCapture.Frames.Add(this);
        }

        protected PmFrameBase() { }

        /// <summary>
        ///     Fill L3, L4, L7 offsets and other useful information related to IP fragmentation or TCP reassembling
        ///     BEAWARE Runns in Parallel
        /// </summary>
        /// <returns>Everything went fine ? true : false</returns>
        public virtual async Task IndexFrame(IPmCaptureProcessorBlockBase captureProcessorBlockBase)
        {
            try
            {
                this.PmCaptureRefId = this.PmCapture.Id;
                //TODO: After Packet.Extract refactor, what is current efficiency of this method?
                var packet = this.PmPacket;
                this.L3Offset = this.L2Offset + packet.PacketHeaderOffset;
                this.L4Offset = this.L2Offset + packet.SegmentHeaderOffset;
                //Verify that L4Payload has some data, if not then leave L7Offset uninitialized
                //TODO: Vesely - What about frames with L4Payload + padding? For them IncludedLength shoud be recounted!
                this.L7Offset = packet.SegmentPayloadLength != 0 ? this.L2Offset + packet.SegmentPayloadOffset : -1;

                this.IpProtocol = packet.ProtocolIdentifier;

                //Create virtual frame from tunel protocol payload
                switch (packet.ProtocolIdentifier)
                {
                    case IPProtocolType.IP:
                    case IPProtocolType.TCP:
                    case IPProtocolType.UDP:
                    case IPProtocolType.ICMP:
                    case IPProtocolType.ICMPV6:
                        this.SrcAddress = packet.SourceAddress;
                        this.DstAddress = packet.DestinationAddress;
                        break;
                    case IPProtocolType.GRE:
                        this.SrcAddress = packet.SourceAddress;
                        this.DstAddress = packet.DestinationAddress;
                        await captureProcessorBlockBase.CreateAndAddToMetaFramesVirtualFrame(this, packet);
                        break;

                    //Possible 6in4
                    case IPProtocolType.IPV6:
                        this.SrcAddress = packet.SourceAddress;
                        this.DstAddress = packet.DestinationAddress;
                        if (packet.IpVersion == IpVersion.IPv4) {await captureProcessorBlockBase.CreateAndAddToMetaFramesVirtualFrame(this, packet); }
                        break;

                        //Possible Teredo
                        // TODO: Looooot of exceptions
                        //case IPProtocolType.UDP:
                        //	if(packet.IpVersion == IpVersion.IPv4) { this.CreateAndAddToMetaFramesVirtualFrame(this, packet); }

                        //	break;
                }
                this.SrcPort = packet.SourceTransportPort;
                this.DstPort = packet.DestinationTransportPort;

                this.Ipv4FIdentification = packet.Ipv4Identification;
                this.Ipv4FMf = packet.Ipv4MFbit;
                this.Ipv4FragmentOffset = packet.Ipv4FragmentOffset;

                this.TcpFAck = packet.TcpFAck;
                this.TcpFCwr = packet.TcpFCwr;
                this.TcpFEce = packet.TcpFEce;
                this.TcpFFin = packet.TcpFFin;
                // TODO: PacketDotNet can't read NS
                this.TcpFNs = false; // packet.TcpFNs;
                this.TcpFPsh = packet.TcpFPsh;
                this.TcpFRst = packet.TcpFRst;
                this.TcpFSyn = packet.TcpFSyn;
                this.TcpFUrg = packet.TcpFUrg;
                this.TcpSequenceNumber = packet.TcpSequenceNumber;
                this.TcpAcknowledgementNumber = packet.TcpAcknowledgmentNumber;

                this.L7PayloadLength = packet.SegmentPayloadLength;
            }
            catch (Exception ex) //TODO To general, it would be better to catch more and specific exceptions
            {
                this.IsMalformed = true;
                //Debugger.Break();
                //todo fix
                //Log.Error(
                //"PmCaptureProcessorBase Cannot parse frame: " + (this.FrameIndex) + " in file: " + this.GetFileName() +
                //    ". It is marked as malformed!", ex);
                PmConsolePrinter.PrintError("Cannot parse frame: " + (this.FrameIndex) + " in file: " + this.PmCapture.FileInfo.Name + ". It is marked as malformed!\n" + ex.Message);
            }
        }
        //TODO: Mares - neni to moc pekne
        [Obsolete("Vesely - Navrhuji s PacketInfo pracovat mimo kontext PmFrameBase")]
        
        [NotMapped]
        public Packet PacketDotNet => this.PmPacket?.PacketInfo;
        
        [NotMapped]
        public IPEndPoint DestinationEndPoint => this._destinationEndPoint ?? (this.DstAddress == null? NullEndPoint : (this._destinationEndPoint = new IPEndPoint(this.DstAddress, this.DstPort)));

        /// <summary>
        ///     Transport destination port
        /// </summary>
        public Int32 DstPort { get; set; } = RESERVED;

        /// <summary>
        ///     Frame index of particular frame inside PCAP file
        /// </summary>
        [DataMember]
        public Int64 FrameIndex { get; set; }

        /// <summary>
        ///     Frame offset of particular frame inside PCAP file
        /// </summary>
        [DataMember]
        public Int64 FrameOffset { get; set; }

        /// <summary>
        ///     Length of frame in bytes that is actually included, might be lesser than OriginalLength when snapping have been used during capture
        /// </summary>
        [DataMember]
        public Int64 IncludedLength { get; set; }

        /// <summary>
        ///     IP Protocol number
        /// </summary>
        public IPProtocolType IpProtocol { get; set; } = IPProtocolType.NONE;

        /// <summary>
        ///     Pragma value whether target frame is malformed
        /// </summary>
        /// <remarks>
        ///     IsMalformed is usually because Ethernet checksum does not match. This happens on tranfer bit error or when
        ///     less than expected number of bytes is present.
        /// </remarks>
                    //By default is frame considered to be OK (false value). But when it is processed by PacketDotNet it could be marked as malformed upon any exception during parsing.
        [DataMember]
        public Boolean IsMalformed { get; set; } = false;

        public bool IsL4Frame => (this.IpProtocol == IPProtocolType.TCP || this.IpProtocol == IPProtocolType.UDP);

        /// <summary>
        ///     Starting offset of L2 header
        /// </summary>
        /// <returns>Starting offset of DataPart</returns>
        [DataMember]
        public Int64 L2Offset { get; set; } = UNINITIALIZED;

        /// <summary>
        ///     Starting offset of L3 header
        /// </summary>
        //[DataMember]
        public Int64 L3Offset { get; set; } = UNINITIALIZED;

        /// <summary>
        ///     Starting offset of L4 header
        /// </summary>
        //[DataMember]
        public Int64 L4Offset { get; set; } = UNINITIALIZED;

        /// <summary>
        ///     Starting offset of L7 header
        /// </summary>
        //[DataMember]
        public Int64 L7Offset { get; set; } = UNINITIALIZED;
        
        /// <summary>
        ///     Starting offset of ApplicationMessage
        /// </summary>
        
        [NotMapped]
        public Int64 MessageOffset => (this.L7Offset != -1)? this.L7Offset : this.L4Offset;

        public int L2HeaderSize => (int)(this.L3Offset - this.L2Offset);
        public int L2L3HeaderSize => (int)(this.L4Offset - this.L2Offset);

        public int L2L3L4HeaderSize => (this.L7Offset != -1) ? (int)(this.L7Offset - this.L2Offset) : (int) this.OriginalLength;

        /// <summary>
        ///     Original length of frame in bytes
        /// </summary>
        [DataMember]
        public Int64 OriginalLength { get; set; }

        /// <summary>
        ///     Original length of frame in bytes without padding
        /// </summary>

        public Int64 OriginalLengthWithoutPadding {
            get
            {
                if(this.PmFrameType == PmFrameType.VirtualBlank || this.PmFrameType == PmFrameType.VirutalNoise)
                    return this.IncludedLength;
                var len = 0;
                switch(this.PmPacket.EthernetType)
                {
                    case EthernetPacketType.IpV4:
                    case EthernetPacketType.IpV6:
                        len += this.PmPacket.Ethernet.Header.Length;
                        len += this.PmPacket.IP.TotalLength;
                        break;
                    default: return this.IncludedLength;
                }
                return len;
            } }

        /// <summary>
        ///     Frame type according to relevant PCAP format
        /// </summary>
        [DataMember]
        public PmFrameType PmFrameType { get; set; }

        /// <summary>
        ///     Data-link type of original frame
        /// </summary>
        [DataMember]
        public PmLinkType PmLinkType { get; set; }

        [NotMapped]
        [DataMember]
        public Byte[] L2DataEncapsulated { get; set; }

        /// <summary>
        ///     Method parses frame itself using PacketDotNet and returns new instance of PmPacket.
        /// </summary>
        [IgnoreAutoChangeNotification]
        [NotMapped]
        public PmPacket PmPacket
        {
            get
            {
                PmPacket pmPacket = null;
                if (this._pmPacket == null || !this._pmPacket.TryGetTarget(out pmPacket))
                {
                    pmPacket = new PmPacket(this.PmLinkType, this.L2Data() ?? this.L2DataEncapsulated);
                    this._pmPacket = new WeakReference<PmPacket>(pmPacket);
                }
                return pmPacket;
            }
        }

        [NotMapped]
        public IPEndPoint SourceEndPoint => this._sourceEndPoint ?? (this.SrcAddress == null? NullEndPoint : (this._sourceEndPoint = new IPEndPoint(this.SrcAddress, this.SrcPort)));

       
        /// <summary>
        ///     Transport source port
        /// </summary>
        //[DataMember]
        public Int32 SrcPort { get; set; } = RESERVED;

        /// <summary>
        ///     Frame capture time stamp
        /// </summary>
        [DataMember]
        public DateTime TimeStamp
        {
            get { return this.FirstSeen; }
            set { this.FirstSeen = value; }
        }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime FirstSeen { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int OrderKey { get; set; }

        /// <summary>
        ///     Searches raw frame for occurence of a single byte array pattern
        /// </summary>
        /// <param name="haystack">Where to look</param>
        /// <param name="needle">For what to look</param>
        /// <param name="start">Where to start looking in a haystack</param>
        /// <returns>Returns index of found pattern, otherwise returns -1</returns>
        public Int32 MatchPattern(Byte[] haystack, Byte[] needle, Int32 start)
        {
            var found = -1;
            //only look at this if we have a populated search array and search bytes with a sensible start
            if(haystack.Length <= 0 || needle.Length <= 0 || start > (haystack.Length - needle.Length) || haystack.Length < needle.Length) { return -1; }
            //iterate through the array to be searched
            for(var i = start; i <= haystack.Length - needle.Length; i++)
            {
                //if the start bytes match we will start comparing all other bytes, otherwise continue
                if(haystack[i] != needle[0]) { continue; }
                if(haystack.Length > 1)
                {
                    //multiple bytes to be searched we have to compare byte by byte
                    var matched = true;
                    for(var y = 1; y <= needle.Length - 1; y++)
                    {
                        //if there is a match then continue
                        if(haystack[i + y] == needle[y]) { continue; }
                        //match broken
                        matched = false;
                        break;
                    }
                    if(!matched) { continue; }
                    //everything matched up
                    found = i;
                    break;
                }
                //search byte is only one bit nothing else to do
                found = i;
                break; //stop the loop
            }
            return found;
        }

        /// <summary>
        ///     Searches raw frame content for occurence of any pattern
        /// </summary>
        /// <param name="patterns"></param>
        /// <returns>Returns true if at least one pattern was found. Otherwise, returns false.</returns>
        public Boolean MatchPatterns(IEnumerable<Byte[]> patterns)
        {
            var data = this.L2Data();
            return patterns.Any(pat => this.MatchPattern(data, pat, 0) != -1);
        }


        #region Debugging
        /// <summary>
        ///     Converts Frame ADT to human-readable string
        /// </summary>
        /// <returns>String representation of Frame content</returns>
        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("\nFrame num> ")
                .AppendLine(this.FrameIndex.ToString())
                .Append("IP version> ")
                .AppendLine(this.IpProtocol.ToString())
                .Append("Src> ")
                .Append(this.PmPacket.SourceAddress)
                .Append(" ")
                .AppendLine(this.PmPacket.SourceTransportPort.ToString())
                .Append("Dst> ")
                .Append(this.PmPacket.DestinationAddress)
                .Append(" ")
                .Append(this.PmPacket.DestinationTransportPort.ToString())
                .Append("Seq> ")
                .Append(this.PmPacket?.SequenceNumber)
                .Append("L7Size> ")
                .Append(this.PmPacket?.SegmentPayloadLength);
            return sb.ToString();
        }

        public void PrintFrameInfo()
        {
            ("----------- this " + this.FrameIndex + " ------------").PrintInfoEol();
            ("thisOffset> " + this.FrameOffset).PrintInfoEol();
            ("thisType> " + this.PmFrameType).PrintInfoEol();
            ("LinkType> " + this.PmLinkType).PrintInfoEol();
            ("OriginalLength> " + this.OriginalLength).PrintInfoEol();
            ("IncludedLength> " + this.IncludedLength).PrintInfoEol();
            ("Malformed> " + this.IsMalformed).PrintInfoEol();
            ("L2 offset> " + this.L2Offset).PrintInfoEol();
            ("L3 offset> " + this.L3Offset).PrintInfoEol();
            ("L4 offset> " + this.L4Offset).PrintInfoEol();
            ("L7 offset> " + this.L7Offset).PrintInfoEol();
            ("SrcEndPoint> " + this.SourceEndPoint).PrintInfoEol();
            ("DstEndPoint> " + this.DestinationEndPoint).PrintInfoEol();
            ("IPProtocol> " + this.IpProtocol).PrintInfoEol();
            //PmConsolePrinter.PrintInfoEol("TCP flags>" + this.TcpFlags);
            ("TCP Sequence Number>" + this.TcpSequenceNumber).PrintInfoEol();
            ("TCP Acknowledgement Number>" + this.TcpAcknowledgementNumber).PrintInfoEol();
        }
        #endregion

        // ReSharper disable InconsistentNaming
        [NonSerialized]
        private const Int16 UNINITIALIZED = -1;
        [NonSerialized]
        private const Byte RESERVED = 0;
// ReSharper restore InconsistentNaming

        #region IP defragmentation related fields
        /// <summary>
        ///     IPv4 fragment identifier
        /// </summary>
        public Int32 Ipv4FIdentification { get; set; } = UNINITIALIZED;

        /// <summary>
        ///     IPv4 fragment flag MF
        /// </summary>
        public Boolean Ipv4FMf { get; set; } = false;

        /// <summary>
        ///     Fragment part number from IPv4 header - in range from 1 to 2^13-1
        /// </summary>
        public Int32 Ipv4FragmentOffset { get; set; } = UNINITIALIZED;
        #endregion

        #region TCP related fields
        /// <summary>
        ///     TCP flag ACK
        /// </summary>
        [NotMapped]
        public Boolean TcpFAck { get; set; } = false;

        /// <summary>
        ///     TCP flag CWR
        /// </summary>
        [NotMapped]
        public Boolean TcpFCwr { get; set; } = false;

        /// <summary>
        ///     TCP flag ECE
        /// </summary>
        [NotMapped]
        public Boolean TcpFEce { get; set; } = false;

        /// <summary>
        ///     TCP flag FIN
        /// </summary>
        [NotMapped]
        public Boolean TcpFFin { get; set; } = false;

        /// <summary>
        ///     TCP flag NS
        /// </summary>
        [NotMapped]
        public Boolean TcpFNs { get; set; } = false;

        /// <summary>
        ///     TCP flag PSH
        /// </summary>
        [NotMapped]
        public Boolean TcpFPsh { get; set; } = false;

        /// <summary>
        ///     TCP flag RST
        /// </summary>
        [NotMapped]
        public Boolean TcpFRst { get; set; } = false;

        /// <summary>
        ///     TCP flag SYN
        /// </summary>
        [NotMapped]
        public Boolean TcpFSyn { get; set; } = false;

        /// <summary>
        ///     TCP flag URG
        /// </summary>
        [NotMapped]
        public Boolean TcpFUrg { get; set; } = false;

        /// <summary>
        ///     TCP sequence number to assure in order delivery
        /// </summary>
        [DataMember]
        public Int64 TcpSequenceNumber { get; set; } = UNINITIALIZED;

        /// <summary>
        ///     TCP acknowledgement number to assure in order delivery
        /// </summary>
        [DataMember]
        public Int64 TcpAcknowledgementNumber { get; set; } = UNINITIALIZED;

        /// <summary>
        ///     TCP window size
        /// </summary>
        [NotMapped]
        [DataMember]
        public Int32 WindowSize { get; set; } = 65535;

        [NotMapped]
        [DataMember]
        public Int32 WindowSizeScale { get; set; }

        /// <summary>
        ///     Real calculated windows size, realWindowSize = WindowSize << WindowSizeScale
        /// </summary>

        [NotMapped]
        public UInt64 RealWindowSize => (UInt64) this.WindowSize << this.WindowSizeScale;

        private IPAddress _dstAddress;
        private IPAddress _srcAddress;
        private WeakReference<PmPacket> _pmPacket;
        private IPEndPoint _sourceEndPoint;
        private IPEndPoint _destinationEndPoint;
        private L7Conversation _l7Conversation;
        private L7PDU _l7Pdu;
        private L3Conversation _l3Conversation;
        private PmCaptureBase _pmCapture;
        private L4Conversation _l4Conversation;
        private Guid? _l7ConversationRefId;
        #endregion

        #region Other useful calculated properties
        /// <summary>
        ///     Returns length of L7 Payload ergo application message length or -1 if no L7Payload is present
        /// </summary>
        
        //[DataMember]
        //public virtual long L7PayloadLength => (this.L7Offset <= this.L2Offset)? -1 : this.IncludedLength - (this.L7Offset - this.L2Offset);
        public Int64 L7PayloadLength { get; set; }

        /// <summary>
        ///     Returns length of application message length
        /// </summary>
        /// <summary>
        ///     Function gets raw L2 frame
        /// </summary>
        /// <returns>Returns data in form of byte field</returns>
        public virtual Byte[] L2Data()
        {
            return this.GetData(this.L2Offset,0);
        }

     /// <summary>
        ///     Function gets raw L3 packet
        /// </summary>
        /// <returns>Returns data in form of byte field</returns>
        public virtual Byte[] L3Data()
        {
            return this.GetData(this.L3Offset, this.L3Offset - this.L2Offset);
        }

        /// <summary>
        ///     Function gets raw L4 segment/datagram
        /// </summary>
        /// <returns>Returns data in form of byte field</returns>
        public virtual Byte[] L4Data()
        {
            return this.GetData(this.L4Offset, this.L4Offset - this.L2Offset);
        }
        
        /// <summary>
        ///     Function gets raw application data (L4 payload)
        /// </summary>
        /// <returns>Returns data in form of byte field or null if data are not present</returns>
        public virtual Byte[] L7Data()
        {
            return this.GetDataBySize(this.L7Offset, (Int32)this.L7PayloadLength);
        }
        private byte[] GetData(long offsetInCaptureFile, long subOfIncluded)
        {
            if (offsetInCaptureFile < this.L2Offset || this.L2Offset == -1) { return null; }
            if (this.PmCapture != null)
            {
                BinaryReader reader = null;
                try
                {
                    reader = this.PmCapture.BinaryReadersPool.GetReader();
                    reader.BaseStream.Seek(offsetInCaptureFile, SeekOrigin.Begin);
                    return reader.ReadBytes((Int32)(this.IncludedLength - subOfIncluded));
                }
                finally
                {
                    if (reader != null) this.PmCapture.BinaryReadersPool.PutReader(reader);
                }
            }
            if (this.L2DataEncapsulated != null)
            {
                var includedLength = (Int32)(this.IncludedLength - subOfIncluded);
                var data = new byte[includedLength];
                Array.Copy(this.L2DataEncapsulated, subOfIncluded, data, 0, includedLength);
                return data;
            }
            return null;
        }

        private byte[] GetDataBySize(long offsetInCaptureFile, Int32 length)
        {
            if (offsetInCaptureFile < this.L2Offset || this.L2Offset == -1) { return null; }
            if (this.PmCapture != null)
            {
                BinaryReader reader = null;
                try
                {
                    reader = this.PmCapture.BinaryReadersPool.GetReader();
                    reader.BaseStream.Seek(offsetInCaptureFile, SeekOrigin.Begin);
                    return reader.ReadBytes(length);
                }
                finally
                {
                    if (reader != null) this.PmCapture.BinaryReadersPool.PutReader(reader);
                }
            }
            if (this.L2DataEncapsulated != null)
            {
                var sourceIndex = (Int32) (this.IncludedLength - length);
                var data = new byte[length];
                Array.Copy(this.L2DataEncapsulated, sourceIndex, data, 0, length);
                return data;
            }
            return null;
        }


        /// <summary>
        ///     Function gets raw application message data
        /// </summary>
        /// <returns>Returns data in form of byte field or null if data are not present</returns>
        /// <summary>
        ///     Compares timestamps of two IPmFrames and returns the one which has earlier timestamp
        /// </summary>
        /// <param name="other">Another PmFrameBase instance</param>
        /// <returns>Returns PmFrameBase with earlier time stamps or tie in case there is noone to compare</returns>
        public Int32 CompareTo(PmFrameBase other) => other != null? this.TimeStamp.CompareTo(other.TimeStamp) : 0;
        #endregion

        #region EF database
        public Guid L3ConversationRefId { get; private set; } = Guid.Empty;

        [ForeignKey(nameof(L3ConversationRefId))]
        public virtual L3Conversation L3Conversation
        {
            get { return this._l3Conversation; }
            set
            {
                this._l3Conversation = value;
                this.L3ConversationRefId = value?.Id ?? Guid.Empty;
            }
        }

        [DataMember]
        public Guid PmCaptureRefId { get; private set; } = Guid.Empty;

        [ForeignKey(nameof(PmCaptureRefId))]
        public virtual PmCaptureBase PmCapture
        {
            get { return this._pmCapture; }
            set
            {
                this._pmCapture = value;
                this.PmCaptureRefId = value?.Id ?? Guid.Empty;
            }
        }

        public Guid L4ConversationRefId { get; private set; } = Guid.Empty;

        [ForeignKey(nameof(L4ConversationRefId))]
        public virtual L4Conversation L4Conversation
        {
            get { return this._l4Conversation; }
            set
            {
                this._l4Conversation = value;
                this.L4ConversationRefId = value?.Id ?? Guid.Empty;
            }
        }

        public Guid L7PduRefId { get; set; } = Guid.Empty;

        [ForeignKey((nameof(L7PduRefId)))]
        public virtual L7PDU L7Pdu
        {
            get { return this._l7Pdu; }
            set
            {
                this._l7Pdu = value;
                this.L7PduRefId = value?.Id ?? Guid.Empty;
            }
        }

        public Guid L7ConversationRefId
        {
            get
            {
                if(this._l7ConversationRefId.HasValue) return this._l7ConversationRefId.Value;
                if(this.L7Pdu?.L7ConversationRefId.HasValue ?? false) return this.L7Pdu.L7ConversationRefId.Value;
                return Guid.Empty;
            }
            set { this._l7ConversationRefId = value; }
        }

        [ForeignKey(nameof(L7ConversationRefId))]
        public virtual L7Conversation L7Conversation
        {
            get { return this._l7Conversation; }
            set
            {
                this._l7Conversation = value;
                this.L7ConversationRefId = value?.Id??Guid.Empty;
            }
        }

        [MaxLength(16)]
        public byte[] DstAddressData { get; private set; } = {0,0,0,0};

        //[DataMember]
        [NotMapped]
        public IPAddress DstAddress
        {
            get { return this._dstAddress ?? (this._dstAddress = new IPAddress(this.DstAddressData)); }
            set
            {
                this._dstAddress = value;
                this.DstAddressData = value.GetAddressBytes();
            }
        }
   
        [MaxLength(16)] public byte[] SrcAddressData { get; private set; } = { 0, 0, 0, 0 };

        [NotMapped]
        public IPAddress SrcAddress
        {
            get { return this._srcAddress ?? (this._srcAddress = new IPAddress(this.SrcAddressData)); }
            set
            {
                this._srcAddress = value;
                this.SrcAddressData = value.GetAddressBytes();
            }
        }

        /// <summary>
        ///     Starting offset of L2 header capture l4
        /// </summary>
        /// <returns>Starting offset of DataPart</returns>
        public Int64 L2OffsetCaptureL4 { get; set; }

        /// <summary>
        ///     Starting offset of L3 header capture l4
        /// </summary>
        public Int64 L3OffsetCaptureL4 { get; set; }

        /// <summary>
        ///     Starting offset of L4 header capture l4
        /// </summary>
        public Int64 L4OffsetCaptureL4 { get; set; }

        /// <summary>
        ///     Starting offset of L7 header capture l4
        /// </summary>
        public Int64 L7OffsetCaptureL4 { get; set; }

        [NotMapped]
        public bool IsDoneUpdateOffsetsForCaptureL4 { get; set; } = false;
        #endregion

        #region Overrides of Object
        public override bool Equals(object obj)
        {
            var frame = obj as PmFrameBase;
            if (frame == null){return false;}
            var e2 = this.TimeStamp == frame.TimeStamp;
            var e3 = this.FrameIndex == frame.FrameIndex;
            var e4 = this.SrcAddressData.SequenceEqual(frame.SrcAddressData);
            var e5 = this.DstAddressData.SequenceEqual(frame.DstAddressData);
            return e2 && e3 && e4 && e5;

        }

        public override int GetHashCode() {
            return this.TimeStamp.GetHashCode()^this.FrameIndex.GetHashCode()^this.SrcAddressData.GetHashCode()^this.DstAddressData.GetHashCode();
            
        }

        #region Implementation of IOrdereableEntity
        public long OrderingKey { get; set; }
        #endregion

        #endregion

        /// <summary>References to frames, which were decapsulated from this frame or from a PDU that this frame partly carries.</summary>
        public List<PmFrameBase> EncapsulatedFrames = new List<PmFrameBase>();

        /// <summary>References to frames, from which this frame was decapsulated.</summary>
        public List<PmFrameBase> DecapsulatedFromFrames = new List<PmFrameBase>();
    }
}