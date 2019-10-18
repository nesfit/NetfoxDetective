// Copyright (c) 2017 Jan Pluskal
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Netfox.Framework.Models.PmLib;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.CaptureProcessor.Captures
{
    /// <remarks>
    ///     Class implementing support for TCPD (Pcap) files
    /// </remarks>
    [Serializable]
    public class PmCaptureProcessorBlockPcap : PmCaptureProcessorBlockBase
    {
        #region LinkTypeEnumeration
        /// <summary>
        ///     Enumeration of TCPDump link types
        /// </summary>
        public enum TcpdLinkType
        {
            Null = 0,
            Ethernet = 1,
            Ieee8025 = 6,
            ArcnetBsd = 7,
            Slip = 8,
            Ppp = 9,
            Fddi = 10,
            PppHdlc = 50,
            PppEthernet = 51,
            AtmRfc1483 = 100,
            Raw = 101,
            CiscoHdlc = 104,
            Ieee80211 = 105,
            FrameRelay = 107,
            Loop = 108,
            LinuxSll = 113,
            Ltalk = 114,
            Pflog = 117,
            Ieee80211Prism = 119,
            IPOverFc = 122,
            SunAtm = 123,
            Ieee80211Radiotap = 127,
            ArcnetLinux = 129,
            AppleIPOverIeee1394 = 138,
            Mtp2WithPhdr = 139,
            Mtp2 = 140,
            Mtp3 = 141,
            Sccp = 142,
            Docsis = 143,
            LinuxIrda = 144,
            Ieee80211Avs = 163,
            BacnetMsTp = 165,
            PppPppd = 166,
            Gprsllc = 169,
            LinuxLapd = 177,
            BluetoothHciH4 = 187,
            UsbLinux = 189,
            Ppi = 192,
            Ieee802154 = 195,
            Sita = 196,
            Erf = 197,
            BluetoothHciH4WithPhdr = 201,
            Ax25Kiss = 202,
            Lapd = 203,
            PppWithDir = 204,
            CiscoHdlcWithDir = 205,
            FrameRelayWithDir = 206,
            IpmbLinux = 209,
            Ieee802154NonaskPhy = 215,
            UsbLinuxMmapped = 220,
            Fc2 = 224,
            Fc2WithFrameDelims = 225,
            Ipnet = 226,
            CanSocketcan = 227,
            Ipv4 = 228,
            Ipv6 = 229,
            Ieee802154Nofcs = 230,
            Dbus = 231,
            DvbCi = 235,
            Mux27010 = 236,
            Stanag5066DPdu = 237,
            Nflog = 239,
            Netanalyzer = 240,
            NetanalyzerTransparent = 241,
            IPOib = 242,
            Mpeg2Ts = 243,
            LinktypeNg40 = 244,
            LinktypeNfcLlcp = 245
        }
        #endregion

        #region Constructors
        /// <summary>
        ///     Load constructor - generaly for loading input from file
        ///     (no need to call OpenFile after using this constructor)
        /// </summary>
        public PmCaptureProcessorBlockPcap(FileInfo fileInfo)
            : base(new PmCapturePcap(fileInfo)) {}
        #endregion

        #region PcapFileVariables
        /// <summary>
        ///     Major version number
        /// </summary>
        private UInt16 _versionMaj;

        /// <summary>
        ///     Minor version number
        /// </summary>
        private UInt16 _versionMin;

        /// <summary>
        ///     LibPcap Link type that is shared by all frames in capture file
        /// </summary>
        private TcpdLinkType _pcapLinkType;

        /// <summary>
        ///     Timezone offset that is used when measuring "real-time"
        /// </summary>
        private Int32 _tcpdTimeZoneOffset;

        /// <summary>
        ///     Flags
        /// </summary>
        private UInt32 _sigfigs;

        /// <summary>
        ///     If truncation of frames occured then what is captured length (firts X bytes of frame)
        /// </summary>
        private UInt32 _snaplen;

        /// <summary>
        ///     Common link type of captured frames which is independent and derived from TCPDLinkType
        /// </summary>
        private PmLinkType _pmLinkType = PmLinkType.Null;
        #endregion

        #region IPMCaptureFileIOMethods
        /// <summary>
        ///     Initialize FrameTables and appropriate FrameVectors from input file
        /// </summary>
        protected override async Task<Boolean> ParseCaptureFile()
        {
            try
            {
                this.ParseTcpdHeader();
                this._pmLinkType = ConvertPcapLayer2ToCommonLayer2(this._pcapLinkType);
                await this.ParseTcpdFrameTable();
            }
            catch(EndOfStreamException ex)
            {
                //todo fix
                //Log.Error("Some packet was malformed and ended prematurely.", ex);
                PmConsolePrinter.PrintError("Error>\n" + ex.Message);
            }
            catch(Exception ex) //todo to general
            {
                //todo fix
                //Log.Error("PmCaptureProcessorPcap General error", ex);
                PmConsolePrinter.PrintError("Error>\n" + ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Store frames contained in Frame Table to PCAP file
        /// </summary>
        public override Boolean WriteCaptureFile(String fileName) => this.CreateTcpdOutput(fileName);

        /// <summary>
        ///     First timestamp for LibPcap is timestamp of first frame
        /// </summary>
        /// <returns>Returns DateTime of first frame</returns>
        public override DateTime? GetFirstTimeStamp()
            => ConvertUnixTimeToDate(ConvertTcpdToUnixTime((Int32) (this.GetTcpdPckTimestampSeconds(24) + this._tcpdTimeZoneOffset), (Int32) this.GetTcpdPckTimestampUseconds(24)));
        #endregion

        #region TCPDtoSupprotedtypesConversionMethods
        /// <summary>
        ///     Converts LibPcap L2 type to appropriate common suppored link type
        /// </summary>
        /// <param name="linkaType">Input enumartion LibPcap LinkType</param>
        /// <returns>Returns output enumarion common link type</returns>
        private static PmLinkType ConvertPcapLayer2ToCommonLayer2(TcpdLinkType linkaType)
        {
            switch(linkaType)
            {
                case TcpdLinkType.Ethernet:
                    return PmLinkType.Ethernet;
                case TcpdLinkType.Fddi:
                    return PmLinkType.Fddi;
                case TcpdLinkType.Raw:
                    return PmLinkType.Raw;
                case TcpdLinkType.Ieee80211:
                    return PmLinkType.Ieee80211;
                case TcpdLinkType.AtmRfc1483:
                    return PmLinkType.AtmRfc1483;
                default:
                    return PmLinkType.Null;
            }
        }

        /// <summary>
        ///     Converts common suppored link type to appropriate LibPcap L2 type
        /// </summary>
        /// <param name="linkaType">Input enumartion LinkType</param>
        /// <returns>Returns output enumarion LibPcap LinkType</returns>
        private static TcpdLinkType ConvertCommonLayer2ToPcapLayer2(PmLinkType linkaType)
        {
            switch(linkaType)
            {
                case PmLinkType.Ethernet:
                    return TcpdLinkType.Ethernet;
                case PmLinkType.Fddi:
                    return TcpdLinkType.Fddi;
                case PmLinkType.Raw:
                    return TcpdLinkType.Raw;
                case PmLinkType.Ieee80211:
                    return TcpdLinkType.Ieee80211;
                case PmLinkType.AtmRfc1483:
                    return TcpdLinkType.AtmRfc1483;
                default:
                    return TcpdLinkType.Null;
            }
        }
        #endregion

        #region TCPD Functions

        #region TCPD misceleanous functions
        /// <summary>
        ///     Converts UnixTime to output variables used in TCPD Packet Header
        /// </summary>
        /// <param name="tspan">Input timestamp representing units since beginning of UNIX Epoch</param>
        /// <param name="seconds">Output number of seconds since 1970-01-01</param>
        /// <param name="microseconds">Output number of microseconds since 1970-01-01</param>
        private static void ConvertUnixTimeToTcpd(TimeSpan tspan, out UInt32 seconds, out UInt32 microseconds)
        {
            //PrintDebug(tspan.Ticks + "-->" + (UInt32)(tspan.Ticks / 10000000) + "." + microseconds);
            seconds = (UInt32) (tspan.Ticks / 10000000);
            microseconds = (UInt32) (tspan.Ticks - ((tspan.Ticks / 10000000) * 10000000)) / 10;
        }

        /// <summary>
        ///     Converts DateTime to amount of ticks since beginning of UNIX Expoch 1970-01-01
        /// </summary>
        /// <param name="current">Input DateTime varibale which measure time since 0001-01-01</param>
        /// <returns>Returns number of ticks since 1970-01-01</returns>
        private static TimeSpan ConvertDateToUnixTime(DateTime current) => new TimeSpan(current.Ticks - new DateTime(1970, 1, 1).Ticks);

        /// <summary>
        ///     Converts TCPD Packet Header time information into TimeSpan variable
        /// </summary>
        /// <param name="seconds">Input seconds</param>
        /// <param name="microseconds">Input microseconds. To get ticks it should be multiplied with 10.</param>
        /// <returns>Returns timestamp representing number of units since 1970-01-01</returns>
        private static TimeSpan ConvertTcpdToUnixTime(Int32 seconds, Int32 microseconds) => new TimeSpan(0, 0, 0, seconds).Add(new TimeSpan(microseconds * 10));

        /// <summary>
        ///     Converts TimeSpan representing units since begeinning of Unix Epoch into real DateTime
        /// </summary>
        /// <param name="tspan">Input timespan</param>
        /// <returns>Returns real DateTime variable</returns>
        private static DateTime ConvertUnixTimeToDate(TimeSpan tspan) => new DateTime(1970, 1, 1).Add(tspan);

        /// <summary>
        ///     Generate TCP Dump LibPCAP format output file
        /// </summary>
        /// <param name="outFile">File name or file path that will be converted to absolute file path</param>
        /// <returns></returns>
        protected Boolean CreateTcpdOutput(String outFile)
        {
            try
            {
                var bw = new BinaryWriter(File.Open(Path.GetFullPath(outFile), FileMode.Create));
                /* TCP Dump format global header variables */
                const UInt32 magicNumber = 0xa1b2c3d4;
                const UInt16 versionMaj = 0x2;
                const UInt16 versionMin = 0x4;
                /* Use first record as refernce TZI offset */
                //    Int32 thisZoneSec = timeZoneOffset; // no conversion needed
                // var linkType = (UInt32)RealFrames.First().PmPcapFile.TCPDLinkType;
                //TODO: Add TZI functionality
                const Int32 thisZoneSec = 0;
                var fileLinkType = TcpdLinkType.Null;
                var referenceLinkType = PmLinkType.Null;

                var frames = this.PmCapture.Frames.ToArray();

                if(frames.Any())
                {
                    referenceLinkType = frames.First().PmLinkType;
                    fileLinkType = ConvertCommonLayer2ToPcapLayer2(referenceLinkType);
                }
                var linkTypeValue = (UInt32) fileLinkType;
                const UInt32 sigFigs = 0;
                const UInt32 snapLen = 0x0000ffff;
                //Console.WriteLine("{0:x}|{1:x}|{2:x}|{3:x}|{4:x}|{5:x}|{6:x}", magicNumber, versionMaj, versionMin, thisZoneSec, sigFigs, snapLen, linkType);
                /* Write Global Header */
                bw.Write(magicNumber);
                bw.Write(versionMaj);
                bw.Write(versionMin);
                bw.Write(thisZoneSec);
                bw.Write(sigFigs);
                bw.Write(snapLen);
                bw.Write(linkTypeValue);
                /* Write frames */
                foreach(var fr in frames.Where(fr => fr.PmLinkType == referenceLinkType))
                {
                    /* Convert DateTime timestamp to TCPD variables used in TCPD Packet Header*/
                    UInt32 sec;
                    UInt32 usec;
                    ConvertUnixTimeToTcpd(ConvertDateToUnixTime(fr.TimeStamp), out sec, out usec);

                    bw.Write(sec);
                    bw.Write(usec);
                    bw.Write(fr.IncludedLength);
                    bw.Write(fr.OriginalLength);
                    // bw.Write(GetTCPDPckFrameData(fr));
                    bw.Write(fr.L2Data());
                }

                bw.Close();
            }
            /* If anything went bad generate exception and return false */
            catch(Exception ex) //todo to general
            {
                //todo fix
                //Log.Error("PmCaptureProcessorPcap General error", ex);
                PmConsolePrinter.PrintError(ex.Message);
                return false;
            }
            /* otherwise return true if everything went good */
            return true;
        }
        #endregion

        #region TCPD Global Header parsing Fuctions
        /// <summary>
        ///     Reads major version value on 4th byte of TCPD Header
        /// </summary>
        /// <returns>Returns major number of version in header of TCPD file</returns>
        private UInt16 GetTcpdHdrVersionMaj()
        {
            //binreader.BaseStream.Position = 4;
            this.BinReader.BaseStream.Seek(4, SeekOrigin.Begin);
            return this.BinReader.ReadUInt16();
        }

        /// <summary>
        ///     Reads minor version value on 6th byte of TCPD Header
        /// </summary>
        /// <returns>Returns minor number of version in header of TCPD file</returns>
        private UInt16 GetTcpdHdrVersionMin()
        {
            //binreader.BaseStream.Position = 6;
            this.BinReader.BaseStream.Seek(6, SeekOrigin.Begin);
            return this.BinReader.ReadUInt16();
        }

        /// <summary>
        ///     Reads UTC to local time correction on 8th byte of TCPD Header
        /// </summary>
        /// <returns>Returns correction time in seconds between UTC and time when capture was taken</returns>
        private Int32 GetTcpdHdrTimezoneOffset()
        {
            //binreader.BaseStream.Position = 8;
            this.BinReader.BaseStream.Seek(8, SeekOrigin.Begin);
            return this.BinReader.ReadInt32();
        }

        /// <summary>
        ///     Reads accuracy of timestamps on 12th byte of TCPD Header. In practice all tools set it to 0.
        /// </summary>
        /// <returns>Returns usually 0, everything else is suspicious</returns>
        private UInt32 GetTcpdHdrSigfigs()
        {
            //binreader.BaseStream.Position = 12;
            this.BinReader.BaseStream.Seek(12, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads snapshot length (internal limiter of frames content) on 16th byte of TCPD Header. In practice all tools set
        ///     it to 0.
        /// </summary>
        /// <returns>Returns snapshot length frame limiter. If unlimited then it returns 0xffff (65535)</returns>
        private UInt32 GetTcpdHdrSnapLength()
        {
            //binreader.BaseStream.Position = 16;
            this.BinReader.BaseStream.Seek(16, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads layer 2 link type on 20th byte of TCPD Header. In practice all tools set it to 0.
        /// </summary>
        /// <returns>Returns TCPDLinkType enumeration of layer 2 media on which capture was taken</returns>
        private TcpdLinkType GetTcpdHdrNetwork()
        {
            //binreader.BaseStream.Position = 20;
            this.BinReader.BaseStream.Seek(20, SeekOrigin.Begin);
            return (TcpdLinkType) this.BinReader.ReadUInt32();
        }
        #endregion

        #region TCPD Packet Header parsing functions
        /// <summary>
        ///     Reads seconds in timestamp information starting on zero B of TCPD Packet Header
        /// </summary>
        /// <param name="offset">Starting offset of target frame in TCPD PCAP file</param>
        /// <returns>Number of seconds since start of UNIX Epoch</returns>
        private UInt32 GetTcpdPckTimestampSeconds(Int64 offset)
        {
            //binreader.BaseStream.Position = offset;
            this.BinReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads seconds in timestamp information starting on 4th of TCPD Packet Header
        /// </summary>
        /// <param name="offset">Starting offset of target frame in TCPD PCAP file</param>
        /// <returns>Number of microseconds since start of UNIX Epoch</returns>
        private Int64 GetTcpdPckTimestampUseconds(Int64 offset)
        {
            //binreader.BaseStream.Position = offset + 4;
            this.BinReader.BaseStream.Seek(offset + 4, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads included frame size starting on 8th B in TCPD Packet Header
        /// </summary>
        /// <param name="offset">Starting offset of target frame in TCPD PCAP file</param>
        /// <returns>Included frame length, that is lesser-equal to potential snapshot length.</returns>
        private UInt32 GetTcpdPckIncludedLength(Int64 offset)
        {
            //binreader.BaseStream.Position = offset + 8;
            this.BinReader.BaseStream.Seek(offset + 8, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads original frame size as it was received by network interface card starting on 12th B in TCPD Packet Header
        /// </summary>
        /// <param name="offset">Starting offset of target frame in TCPD PCAP file</param>
        /// <returns>Original frame length as it was presented on the wire.</returns>
        private UInt32 GetTcpdPckOriginalLength(Int64 offset)
        {
            //binreader.BaseStream.Position = offset + 12;
            this.BinReader.BaseStream.Seek(offset + 12, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /* /// <summary>
        /// Reads frame data starting on 16B in TCPD Packet Header
        /// </summary>
        /// <param name="binreader">Binary reader with opened PCAP file</param>
        /// <param name="offset">Starting offset of target frame in TCPD PCAP file</param>
        /// <param name="frameLength">Frame size representing number of bytes to read</param>
        /// <returns>Byte array filled with binary frame data</returns>
        private byte[] GetTCPDPckFrameData(PmFrame fr)
        {
            //binreader.BaseStream.Position = offset + 16;
            BinReader.BaseStream.Seek(fr.FrameOffset + 16, SeekOrigin.Begin);
            return BinReader.ReadBytes((int)fr.IncludedLength);
        }*/

        /// <summary>
        ///     Fills output with appropriate variables from TCPD Packet Header
        /// </summary>
        /// <param name="fr">Target frame</param>
        /// <param name="tsSec">Number of units since beginning of UNIX epoch in seconds</param>
        /// <param name="tsUsec">Number of units since beginning of UNIX epoch in microseconds</param>
        /// <param name="inclLen">Frame length of frame data presented in TCP PCAP File</param>
        /// <param name="origLen">Original frame length as it was received by computer's NIC</param>
        /// <param name="frameData">Binary frame data of inclLen size</param>
        private void GetTcpdPckPacketHeaderRecord(PmFrameBase fr, out Int64 tsSec, out Int64 tsUsec, out Int64 inclLen, out Int64 origLen, out Byte[] frameData)
        {
            tsSec = this.GetTcpdPckTimestampSeconds(fr.FrameOffset);
            tsUsec = this.GetTcpdPckTimestampUseconds(fr.FrameOffset);
            inclLen = this.GetTcpdPckIncludedLength(fr.FrameOffset);
            origLen = this.GetTcpdPckOriginalLength(fr.FrameOffset);
            // frameData = GetTCPDPckFrameData(fr);
            frameData = fr.L2Data();
        }
        #endregion

        #region TCPD printing functions
        /// <summary>
        ///     Prints all information in TCPD Global Header
        /// </summary>
        internal void PrintTcpdGlobalHeader()
        {
            PmConsolePrinter.PrintDebug("GLOBAL HEADER INFORMATION FOR " + this.PmCapture.FileInfo.FullName);
            "Version> {0}.{1}".PrintInfoEol(this._versionMaj, this._versionMin);
            "Correction for UTC> {0}".PrintInfoEol(this._tcpdTimeZoneOffset);
            "Sigfigs> {0}".PrintInfoEol(this._sigfigs);
            "Snapshot Length> {0}".PrintInfoEol(this._snaplen);
            "Data link type> {0}".PrintInfoEol(this._pcapLinkType);
        }

        /// <summary>
        ///     Prints whole TCPD Packet Header inforamtion of one frame record
        /// </summary>
        /// <param name="fr">Target frame</param>
        internal void PrintTcpdPacketHeaderRecord(PmFrameBase fr)
        {
            Int64 tsSec;
            Int64 tsUsec;
            Int64 inclLen;
            Int64 origLen;
            Byte[] frameData;

            this.GetTcpdPckPacketHeaderRecord(fr, out tsSec, out tsUsec, out inclLen, out origLen, out frameData);

            "Time span in seconds.useconds> {0}.{1}".PrintInfoEol(tsSec, tsUsec);
            "Converted time> {0}".PrintInfoEol(ConvertUnixTimeToDate(ConvertTcpdToUnixTime((Int32) tsSec, (Int32) tsUsec)).ToString("dd/MM/yyyy HH:mm:ss.FFFFFFF"));

            "Included frame length> ".PrintInfoEol();
            if(inclLen > this._snaplen) {
                PmConsolePrinter.PrintError(inclLen.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                Console.WriteLine("{0}", inclLen);
            }

            "Original frame length> {0}".PrintInfoEol(origLen);
            "Frame data> {0}".PrintInfoEol(frameData);
        }

        /// <summary>
        ///     Prints whole frame table from TCPD PCAP file
        /// </summary>
        internal void PrintTcpdFrameTable()
        {
            foreach(var fr in this.PmCapture.Frames)
            {
                PmConsolePrinter.PrintDebug("FRAME " + fr.FrameIndex + " OFFSET " + fr.FrameOffset);
                this.PrintTcpdPacketHeaderRecord(fr);
            }
        }
        #endregion

        #region TCPD parsing function
        /// <summary>
        ///     Parses MNM CaptureProcessor File Header and initialize all basic MNM properties of Sniff File Specification
        /// </summary>
        private void ParseTcpdHeader()
        {
            this._versionMaj = this.GetTcpdHdrVersionMaj();
            this._versionMin = this.GetTcpdHdrVersionMin();
            this._tcpdTimeZoneOffset = this.GetTcpdHdrTimezoneOffset();
            this._sigfigs = this.GetTcpdHdrSigfigs();
            this._snaplen = this.GetTcpdHdrSnapLength();
            this._pcapLinkType = this.GetTcpdHdrNetwork();
            //PrintTCPDGlobalHeader(sfs);            
        }

        /// <summary>
        ///     Generate next frame offset via equation = offset + 16B + included frame length
        /// </summary>
        /// <param name="offset">Starting offset of target frame in TCPD PCAP file</param>
        /// <param name="length"></param>
        /// <returns>Returns offset of next frame</returns>
        private Int64 CountTcpdNextFrameOffset(Int64 offset, Int64 length) => offset + 16 + length;

/*
        /// <summary>
        /// Generate next frame offset via equation = offset + 16B + included frame length, this variant includes binary reading operation
        /// </summary>
        /// <param name="offset">Starting offset of target frame in TCPD PCAP file</param>
        /// <returns></returns>
        private UInt32 CountTCPDNextFrameOffset(UInt32 offset)
        {
            return offset + 16 + GetTCPDPckIncludedLength(offset);
        }
*/

        /// <summary>
        ///     Parses frame table of TCPD PCAP file and more importantly it initialize TCPFrameTable variable where are stored all
        ///     frame offsets
        /// </summary>
        private async Task ParseTcpdFrameTable()
        {
            if(!PmSupportedTypes.IsSupportedLinkType(this._pmLinkType)) { return; }

            //sfs.RealFrames.Clear();
            /* First frame starts always on 24th byte of TCPD PCAP file. Just after Globap CaptureProcessor Header */
            //sfs.TCPDFrameTable = new List<uint> {24};
            Int64 last = 24;
            var velikost = this.BinReader.BaseStream.Length;
            Int64 index = 0;
            do
            {
                var len = this.GetTcpdPckIncludedLength(last);
                await this.PmMetaFramesBufferBlock.SendAsync(new PmFramePcap(this.PmCapture, index, last, this._pmLinkType,
                    ConvertUnixTimeToDate(ConvertTcpdToUnixTime((int) (this.GetTcpdPckTimestampSeconds(last) + this._tcpdTimeZoneOffset),
                        (int) this.GetTcpdPckTimestampUseconds(last))), this.GetTcpdPckOriginalLength(last), len));
                last = this.CountTcpdNextFrameOffset(last, len);
                index++;
            } while(last != velikost);

            /* Remove last record because it is unparsable EOF */
            //sfs.TCPDFrameTable.Remove(sfs.TCPDFrameTable.Last());
            //PrintError(sfs.TCPDFrameTable.Count.ToString());
            //PrintError("xxx"+sfs.RealFrames.Count.ToString());
        }
        #endregion

        #endregion
    }
}