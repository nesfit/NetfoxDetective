using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Netfox.Framework.Models.PmLib;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.PmLib.SupportedTypes;
using Numl.Utils;

namespace Netfox.Framework.CaptureProcessor.Captures
{
    /// <remarks>
    ///     Class implementing support for Microsoft Netmon files
    /// </remarks>
    [Serializable]
    internal class PmCaptureProcessorBlockMnm : PmCaptureProcessorBlockBase
    {
        #region LinkTypeEnumeration

        /// <summary>
        ///     Enumeration of Microsoft Network Monitor media types (including non-L2 carriers of information)
        /// </summary>
        public enum MnmMediaType
        {
            Null = 0,
            Ethernet = 1,
            Tokenring = 2,
            Fddi = 3,
            Atm = 4,
            Ieee1394 = 5,
            WiFi = 6,
            TunnelingInterfaces = 7,
            WirelessWan = 8,
            RawIpFrames = 9,
            UnsupportedPcapLayer2Type = 0xE000,
            LinuxCookedMode = 0xE071,
            NetEvent = 0xFFE0,
            NetmonNetworkInfoEx = 0xFFFB,
            NetmonPayloadHeader = 0xFFFC,
            NetmonNetworkInfo = 0xFFFD,
            NetmonDnsCache = 0xFFFE,
            NetmonFilter = 0xFFFF
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Load constructor - constructor for existing PCAP file
        /// </summary>
        /// <param name="fileInfo">File path to PCAP</param>
        /// <param name="controllerCaptureProcessorWindsorContainer"></param>
        public PmCaptureProcessorBlockMnm(FileInfo fileInfo)
            : base(new PmCaptureMnm(fileInfo))
        {
        }

        #endregion

        #region MNMFileVariables

        // MNM variables

        /// <summary>
        ///     Major version number
        /// </summary>
        private Byte _mnmVersionMaj;

        /// <summary>
        ///     Minor version number
        /// </summary>
        private Byte _mnmVersionMin;

        /// <summary>
        ///     Media type of captured frames which is MNM specific
        /// </summary>
        private MnmMediaType _mediaType;

        /// <summary>
        ///     Starting time of capture
        /// </summary>
        private DateTime _mnmHeaderTimeStamp;

        /// <summary>
        ///     Frame Table offset
        /// </summary>
        private UInt32 _mnmFtOffset;

        /// <summary>
        ///     Frame Table length
        /// </summary>
        private UInt32 _mnmFtLength;

        /// <summary>
        ///     ProcessInfo offset
        /// </summary>
        private UInt32 _mnmPiOffset;

        /// <summary>
        ///     Number of ProcessInfo records
        /// </summary>
        private UInt32 _mnmPiCount;

        /// <summary>
        ///     ExtendedInfo offset
        /// </summary>
        private UInt32 _mnmEiOffset;

        /// <summary>
        ///     ExtendedInfo length
        /// </summary>
        private UInt32 _mnmEiLength;

        /// <summary>
        ///     FileTime timestamp from ExtenedInfo
        /// </summary>
        private DateTime _mnmEiFileTimeStamp;

        /// <summary>
        ///     Number of timezone records in ExtendedInfo
        /// </summary>
        private UInt32 _mnmEiTziCount;

        /// <summary>
        ///     Number of captured frames
        /// </summary>
        private UInt32 _mnmNumberOfFrames;

        /// <summary>
        ///     List of ProcessInfo records
        /// </summary>
        private List<UInt32> _mnmPiRecords;

        /// <summary>
        ///     List of TimezoneInfo records
        /// </summary>
        private List<UInt32> _mnmEiTziRecords;

        /// <summary>
        ///     Common link type of captured frames which is independent and derived from MNM MediaType
        /// </summary>
        private PmLinkType _pmLinkType = PmLinkType.Null;

        /// <summary>
        ///     Timezone offset/bias that is applied when calculating "real-time"
        /// </summary>
        private Int32 _timeZoneOffset;

        #endregion

        #region IPMCaptureFileIOMethods

        /// <summary>
        ///     First timestamp for LibPcap is timestamp of first frame
        /// </summary>
        /// <returns>Returns DateTime of first frame</returns>
        public override DateTime? GetFirstTimeStamp()
            =>
                this._mnmFtOffset != 0
                    ? ConvertMnmFrameTimeOffsetToRealTime(this._mnmHeaderTimeStamp.AddSeconds(this._timeZoneOffset),
                            this.GetMnmFlTimeOffsetLocal(this._mnmFtOffset))
                        .ToUniversalTime()
                    : (DateTime?) null;

        /// <summary>
        ///     Parses MNM PCAP file and generate RealFrames ADT
        /// </summary>
        /// <returns>Everything went fine ? true : false</returns>
        protected override async Task<Boolean> ParseCaptureFile()
        {
            try
            {
                this.ParseMnmHeader();
                this.ParseMnmProcessInfoTable();
                this.ParseMnmExtendedInfo();
                this._pmLinkType = ConvertMnmLayer2ToCommonLayer2(this._mediaType);
                await this.ParseMnmFrameTable();
                //this.PrintMNMProcessInfoTable();
            }
            catch (Exception ex) //todo to general
            {
                //todo fix
                //Log.Error("PmCaptureProcessorMnm General error", ex);
                PmConsolePrinter.PrintError("Error>\n" + ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Generate output PCAP file from current content of RealFrames ADT
        /// </summary>
        /// <param name="fileName">Name of file where frames would be exported</param>
        /// <returns></returns>
        public override Boolean WriteCaptureFile(String fileName) => this.CreateMnmOutput(fileName);

        #endregion

        #region TCPDtoSupprotedtypesConversionMethods

        /// <summary>
        ///     Converts MNM L2 type to appropriate coommon suppored type
        /// </summary>
        /// <param name="mediaType">Input enumeration MNMMediaType</param>
        /// <returns>Returns converted coomon link type</returns>
        private static PmLinkType ConvertMnmLayer2ToCommonLayer2(MnmMediaType mediaType)
        {
            switch (mediaType)
            {
                case MnmMediaType.Ethernet:
                    return PmLinkType.Ethernet;
                case MnmMediaType.WiFi:
                    return PmLinkType.Ieee80211;
                case MnmMediaType.Atm:
                    return PmLinkType.AtmRfc1483;
                case MnmMediaType.RawIpFrames:
                    return PmLinkType.Raw;
                case MnmMediaType.Fddi:
                    return PmLinkType.Fddi;
                default:
                    return PmLinkType.Null;
            }
        }

        /// <summary>
        ///     Converts common suppored type to appropriate MNM L2 type
        /// </summary>
        /// <param name="mediaType">Input enumartion LinkType</param>
        /// <returns>Returns output enumeration MNM MediaType</returns>
        private static MnmMediaType ConvertCommonLayer2ToMnmLayer2(PmLinkType mediaType)
        {
            switch (mediaType)
            {
                case PmLinkType.Ethernet:
                    return MnmMediaType.Ethernet;
                case PmLinkType.Ieee80211:
                    return MnmMediaType.WiFi;
                case PmLinkType.AtmRfc1483:
                    return MnmMediaType.Atm;
                case PmLinkType.Raw:
                    return MnmMediaType.RawIpFrames;
                case PmLinkType.Fddi:
                    return MnmMediaType.Fddi;
                default:
                    return MnmMediaType.Null;
            }
        }

        #endregion

        #region MNM Functions

        #region MNM CaptureProcessor File Header parsing functions

        /// <summary>
        ///     Function returns minor version value currently located on 4th byte from start
        /// </summary>
        /// <returns>Returns minor version value from MNM PCAP</returns>
        private Byte GetMnmCfhVersionMin()
        {
            //binreader.BaseStream.Position = 4;
            this.BinReader.BaseStream.Seek(4, SeekOrigin.Begin);
            return this.BinReader.ReadByte();
        }

        /// <summary>
        ///     Function returns major version value currently located on 5th byte from start
        /// </summary>
        /// <returns>Returns major version value from MNM PCAP</returns>
        private Byte GetMnmCfhVersionMaj()
        {
            //binreader.BaseStream.Position = 5;
            this.BinReader.BaseStream.Seek(5, SeekOrigin.Begin);
            return this.BinReader.ReadByte();
        }

        /// <summary>
        ///     Function returns MNM media type currently located on 6th byte from start
        /// </summary>
        /// <returns>Returns media type from MNMMediaType enum from CaptureProcessor File Header of MNM PCAP</returns>
        private MnmMediaType GetMnmCfhMediaType()
        {
            //binreader.BaseStream.Position = 6;
            this.BinReader.BaseStream.Seek(6, SeekOrigin.Begin);
            return (MnmMediaType) this.BinReader.ReadUInt16();
        }

        /// <summary>
        ///     Function reads 16B long time information starting on 8th byte and converting it to appropriate C# DateTime variable
        /// </summary>
        /// <returns>Returns the time stamp from CaptureProcessor File Header of MNM PCAP</returns>
        private DateTime GetMnmCfhDateTime()
        {
            //binreader.BaseStream.Position = 8;
            this.BinReader.BaseStream.Seek(8, SeekOrigin.Begin);
            return ConvertByteArrToDateTime(this.BinReader.ReadBytes(16));
        }

        /// <summary>
        ///     Function gets starting offset of Frame Table currently located on 24th byte from start
        /// </summary>
        /// <returns>Returns starting offset of Frame Table from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhFrameTableOffset()
        {
            //binreader.BaseStream.Position = 24;
            this.BinReader.BaseStream.Seek(24, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets length of Frame Table currently located on 28th byte from start
        /// </summary>
        /// <returns>Returns length of Frame Table from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhFrameTableLength()
        {
            //binreader.BaseStream.Position = 28;
            this.BinReader.BaseStream.Seek(28, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets starting offset of User Data currently located on 32nd byte from start
        /// </summary>
        /// <returns>Returns starting offset of User Data from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhUserDataOffset()
        {
            //binreader.BaseStream.Position = 32;
            this.BinReader.BaseStream.Seek(32, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets length of User Data currently located on 36th byte from start
        /// </summary>
        /// <returns>Returns length of User Data from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhUserDataLength()
        {
            //binreader.BaseStream.Position = 36;
            this.BinReader.BaseStream.Seek(36, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets starting offset of Comment Info currently located on 40th byte from start
        /// </summary>
        /// <returns>Returns starting offset of Comment Info from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhCommentInfoOffset()
        {
            //binreader.BaseStream.Position = 40;
            this.BinReader.BaseStream.Seek(40, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets length of Comment Info currently located on 44th byte from start
        /// </summary>
        /// <returns>Returns length of Comment Info from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhCommentInfoLength()
        {
            //binreader.BaseStream.Position = 44;
            this.BinReader.BaseStream.Seek(44, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets starting offset of Process Info Table currently located on 48th byte from start
        /// </summary>
        /// <returns>Returns starting offset of Process Info Table from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhProcessInfoTableOffset()
        {
            //binreader.BaseStream.Position = 48;
            this.BinReader.BaseStream.Seek(48, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets number of records in Process Info Table currently located on 52nd byte from start
        /// </summary>
        /// <returns>Returns number of records in Process Info Table</returns>
        private UInt32 GetMnmCfhProcessInfoTableCount()
        {
            //binreader.BaseStream.Position = 52;
            this.BinReader.BaseStream.Seek(52, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets starting offset of Extended Info currently located on 56th byte from start
        /// </summary>
        /// <returns>Returns starting offset of Extended Info from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhExtendedInfoOffset()
        {
            //binreader.BaseStream.Position = 56;
            this.BinReader.BaseStream.Seek(56, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets number of records in Process Info Table currently located on 60th byte from start
        /// </summary>
        /// <returns>Returns length of Process Info Table from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhExtendedInfoLength()
        {
            //binreader.BaseStream.Position = 60;
            this.BinReader.BaseStream.Seek(60, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets starting offset of BidirectionalFlow Stats currently located on 64th byte from start
        /// </summary>
        /// <returns>Returns starting offset of BidirectionalFlow Stats from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhConversationStatsOffset()
        {
            //binreader.BaseStream.Position = 64;
            this.BinReader.BaseStream.Seek(64, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Function gets length of BidirectionalFlow Stats currently located on 68th byte from start
        /// </summary>
        /// <returns>Returns length of BidirectionalFlow Stats from CaptureProcessor File Header in MNM PCAP file</returns>
        private UInt32 GetMnmCfhConversationStatsLength()
        {
            //binreader.BaseStream.Position = 68;
            this.BinReader.BaseStream.Seek(68, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        #endregion

        #region MNM Process Info Table parsing functions

        /// <summary>
        ///     Function gets record from MNM Process Info Table on current offset and fill return variables with appropriate
        ///     values
        /// </summary>
        /// <param name="offset">Offset to starting position of record in Process Info Table</param>
        /// <param name="pathSize">Return value for path size of application path</param>
        /// <param name="applicationPath">Return value for application path</param>
        /// <param name="iconSize">Return value for icon size of application</param>
        /// <param name="iconData">Return value for icon data of application</param>
        /// <param name="pid">Return value for windows Process IDentified</param>
        /// <param name="localPort">Return value for local port of socket used by application</param>
        /// <param name="remotePort">Return value for remote port of socket used by application</param>
        /// <param name="isipv6">Return value for bool pragma whether used addresses are IPv6 or not</param>
        /// <param name="localIP">Return value for local IP address of socket used by application</param>
        /// <param name="remoteIP">Return value for local IP address of socket used by application</param>
        /// <returns>
        ///     Return variables are pathSize, applicationPath, iconSize, iconData, pid, localPort, remotePort, isipv6, localIP
        ///     and remoteIP
        /// </returns>
        private void GetMnmProcessInfoTableRecord(
            UInt32 offset,
            out UInt32 pathSize,
            out String applicationPath,
            out UInt32 iconSize,
            out Byte[] iconData,
            out UInt32 pid,
            out UInt16 localPort,
            out UInt16 remotePort,
            out UInt32 isipv6,
            out IPAddress localIP,
            out IPAddress remoteIP)
        {
            pathSize = this.GetMnmPitPathSize(offset);
            applicationPath = this.GetMnmPitPathToApp(offset);
            iconSize = this.GetMnmPitIconSize(offset);
            iconData = this.GetMnmPitIconData(offset);
            pid = this.GetMnmPitPid(offset);
            localPort = this.GetMnmPitLocalPort(offset);
            remotePort = this.GetMnmPitRemotePort(offset);
            isipv6 = this.GetMnmPitIsipv6(offset);
            localIP = this.GetMnmPitLocalAddress(offset);
            remoteIP = this.GetMnmPitRemoteAddress(offset);
        }

        /// <summary>
        ///     Function is able to count length of MNM Process Info Length with given starting index
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns length of MNM Process Info Table record starting on target offset</returns>
        private UInt32 CountMnmPiRecordLength(UInt32 offset) =>
            offset + 4 + this.GetMnmPitPathSize(offset) + 4 + this.GetMnmPitIconSize(offset) + 48;

        /// <summary>
        ///     Reads MNM Process Info Table version for parsing purpouses, current version is 2
        /// </summary>
        /// <returns>Returns version of MNM Process Info Table</returns>
        private UInt16 GetMnmPitVersion()
        {
            //sfs.BinReader.BaseStream.Position = sfs.MNMPiOffset;
            this.BinReader.BaseStream.Seek(this._mnmPiOffset, SeekOrigin.Begin);
            return this.BinReader.ReadUInt16();
        }

        /// <summary>
        ///     Reads 4B long application path size in given Process Info Table record starting on target offset
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns application path size of Process Info Table record</returns>
        private UInt32 GetMnmPitPathSize(UInt32 offset)
        {
            //binreader.BaseStream.Position = offset;
            this.BinReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads n-bytes long application path in given Process Info Table record starting on target offset
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns application path of Process Info Table record. Returns Empty string if path is not available.</returns>
        private String GetMnmPitPathToApp(UInt32 offset)
        {
            var pathSize = this.GetMnmPitPathSize(offset);
            if (pathSize == 0)
            {
                return String.Empty;
            }

            //binreader.BaseStream.Position = offset + 4;
            this.BinReader.BaseStream.Seek(offset + 4, SeekOrigin.Begin);
            try
            {
                var path = Encoding.Unicode.GetString(this.BinReader.ReadBytes((Int32) pathSize));
                return path.Substring(0, path.LastIndexOf("\0", StringComparison.Ordinal))
                    .ToLower(); //bacause of shits like "Unavailable\0몀"
            }
            catch (Exception)
            {
                Debugger.Break();
                return null;
            }
        }

        /// <summary>
        ///     Reads 4B long application icon size in given Process Info Table record starting on target offset
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns application icon size of Process Info Table record</returns>
        private UInt32 GetMnmPitIconSize(UInt32 offset)
        {
            var delka = this.GetMnmPitPathSize(offset);
            //binreader.BaseStream.Position = offset + 4 + delka;
            this.BinReader.BaseStream.Seek(offset + 4 + delka, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads n-byte long application icon data in given Process Info Table record starting on target offset
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns application icon as byte array. Returns null if icon data are not available.</returns>
        private Byte[] GetMnmPitIconData(UInt32 offset)
        {
            var delka1 = this.GetMnmPitPathSize(offset);
            var delka2 = this.GetMnmPitIconSize(offset);
            if (delka2 == 0)
            {
                return null;
            }

            //binreader.BaseStream.Position = offset + 4 + delka1 + 4;
            this.BinReader.BaseStream.Seek(offset + delka1 + 8, SeekOrigin.Begin);
            return this.BinReader.ReadBytes((Int32) delka2);
        }

        /// <summary>
        ///     Reads 4B long application PID in given Process Info Table record starting on target offset
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns unique Windows application Process IDentifier of Process Info Table record</returns>
        private UInt32 GetMnmPitPid(UInt32 offset)
        {
            var delka1 = this.GetMnmPitPathSize(offset);
            var delka2 = this.GetMnmPitIconSize(offset);
            //binreader.BaseStream.Position = offset + 4 + delka1 + 4 + delka2;
            this.BinReader.BaseStream.Seek(offset + delka1 + 8 + delka2, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads 2B long local port used by application in given Process Info Table record starting on target offset
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns local (source) port used by application in Process Info Table record</returns>
        private UInt16 GetMnmPitLocalPort(UInt32 offset)
        {
            var delka1 = this.GetMnmPitPathSize(offset);
            var delka2 = this.GetMnmPitIconSize(offset);
            //this.BinReader.BaseStream.Position = offset + 4 + delka1 + 4 + delka2 + 4;
            this.BinReader.BaseStream.Seek(offset + delka1 + 12 + delka2, SeekOrigin.Begin);
            return this.BinReader.ReadUInt16();
        }

        /// <summary>
        ///     Reads 2B long remote port used by application in given Process Info Table record starting on target offset
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns remote (destination) port used by application in Process Info Table record</returns>
        private UInt16 GetMnmPitRemotePort(UInt32 offset)
        {
            var delka1 = this.GetMnmPitPathSize(offset);
            var delka2 = this.GetMnmPitIconSize(offset);
            //this.BinReader.BaseStream.Position = offset + 4 + delka1 + 4 + delka2 + 4 + 2 + 2;
            this.BinReader.BaseStream.Seek(offset + delka1 + 16 + delka2, SeekOrigin.Begin);
            return this.BinReader.ReadUInt16();
        }

        /// <summary>
        ///     Reads 4B long pragma value whether IP addresses used in given Process Info Table are IPv6 or not
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Return value different than 0 means IPv6 address.</returns>
        private UInt32 GetMnmPitIsipv6(UInt32 offset)
        {
            var delka1 = this.GetMnmPitPathSize(offset);
            var delka2 = this.GetMnmPitIconSize(offset);
            //binreader.BaseStream.Position = offset + 4 + delka1 + 4 + delka2 + 4 + 2 + 2 + 2 + 2;
            this.BinReader.BaseStream.Seek(offset + delka1 + 20 + delka2, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads either 4B long IPv4 or 16B long IPv6 local (source) address of application in given Process Info Table record
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns local (source) IP address used by application in Process Info Table record</returns>
        private IPAddress GetMnmPitLocalAddress(UInt32 offset)
        {
            var delka1 = this.GetMnmPitPathSize(offset);
            var delka2 = this.GetMnmPitIconSize(offset);
            var isipv6 = this.GetMnmPitIsipv6(offset) == 0;
            //binreader.BaseStream.Position = offset + 4 + delka1 + 4 + delka2 + 4 + 2 + 2 + 2 + 2 + 4;
            this.BinReader.BaseStream.Seek(offset + delka1 + 24 + delka2, SeekOrigin.Begin);
            //return isipv6? new IPAddress(this.BinReader.ReadBytes(4)) : new IPAddress(this.BinReader.ReadBytes(16));
            return isipv6
                ? new IPAddress(this.BinReader.ReadBytes(4).Reverse().ToArray())
                : new IPAddress(this.BinReader.ReadBytes(16).Reverse().ToArray());
        }

        /// <summary>
        ///     Reads either 4B long IPv4 or 16B long IPv6 remote (destination) address of application in given Process Info Table
        ///     record
        /// </summary>
        /// <param name="offset">Offset pointing to start of MNM Process Info Table record</param>
        /// <returns>Returns remote (destination) port used by application in Process Info Table record</returns>
        private IPAddress GetMnmPitRemoteAddress(UInt32 offset)
        {
            var delka1 = this.GetMnmPitPathSize(offset);
            var delka2 = this.GetMnmPitIconSize(offset);
            var isipv6 = this.GetMnmPitIsipv6(offset) == 0;
            //binreader.BaseStream.Position = offset + 4 + delka1 + 4 + delka2 + 4 + 2 + 2 + 2 + 2 + 4 + 16;
            this.BinReader.BaseStream.Seek(offset + delka1 + 40 + delka2, SeekOrigin.Begin);
            //return isipv6 ? new IPAddress(this.BinReader.ReadBytes(4)) : new IPAddress(this.BinReader.ReadBytes(16));
            return isipv6
                ? new IPAddress(this.BinReader.ReadBytes(4).Reverse().ToArray())
                : new IPAddress(this.BinReader.ReadBytes(16).Reverse().ToArray());
        }

        #endregion

        #region MNM Extended Info parsing functions

        /// <summary>
        ///     Counts length of TZI record as result of = offset + 172B
        /// </summary>
        /// <param name="offset">Startting offset</param>
        /// <returns>In fact it returns starting offset of next TZI record</returns>
        private static UInt32 CountEiTziRecordLength(UInt32 offset) => offset + 172;

        /// <summary>
        ///     Reads 2B long version value of MNM Extended Info, use for parsing purpouses
        /// </summary>
        /// <returns>Returns value of version in MNM Extended Info</returns>
        private UInt16 GetMnmEiVersion()
        {
            //binreader.BaseStream.Position = GetMNMCfhExtendedInfoOffset(binreader);
            this.BinReader.BaseStream.Seek(this.GetMnmCfhExtendedInfoOffset(), SeekOrigin.Begin);
            return this.BinReader.ReadUInt16();
        }

        /// <summary>
        ///     Reads 2B long FileTime value in MNM Extended Info and converts it to DateTime
        /// </summary>
        /// <returns>Returns DateTime value of FileTime converted to UTC time</returns>
        private DateTime GetMnmEiFileTime()
        {
            //binreader.BaseStream.Position = GetMNMCfhExtendedInfoOffset(binreader) + 2;
            this.BinReader.BaseStream.Seek(this.GetMnmCfhExtendedInfoOffset() + 2, SeekOrigin.Begin);
            return DateTime.FromFileTimeUtc(this.BinReader.ReadInt64());
        }

        /// <summary>
        ///     Reads 1B long count of TZI records in MNM Extended Info
        /// </summary>
        /// <returns>Return number of TZI records</returns>
        private Byte GetMnmEiCountTzi()
        {
            //binreader.BaseStream.Position = GetMNMCfhExtendedInfoOffset(binreader) + 2 + 8;
            this.BinReader.BaseStream.Seek(this.GetMnmCfhExtendedInfoOffset() + 10, SeekOrigin.Begin);
            return this.BinReader.ReadByte();
        }

        /// <summary>
        ///     Retrieve timezone infromation on given offset
        /// </summary>
        /// <param name="ofset">Input parameter is offset from where information should be retrieved</param>
        /// <param name="bias">Bias from GMT in seconds</param>
        /// <param name="standardName">Winter timezone name(GMT, CMT,...)</param>
        /// <param name="standardDate">Winter starting date</param>
        /// <param name="standardBias">Winter bias</param>
        /// <param name="daylightName">Summer timezone name</param>
        /// <param name="daylightDate">Summer starting date</param>
        /// <param name="daylightBias">Summer bias</param>
        private void GetMnmEiTziRecord(
            UInt32 ofset,
            out Int32 bias,
            out String standardName,
            out DateTime standardDate,
            out Int32 standardBias,
            out String daylightName,
            out DateTime daylightDate,
            out Int32 daylightBias)
        {
            //binreader.BaseStream.Position = ofset;
            this.BinReader.BaseStream.Seek(ofset, SeekOrigin.Begin);
            bias = this.BinReader.ReadInt32();
            standardName = new String(this.BinReader.ReadChars(64));
            standardDate = ConvertByteArrToDateTime(this.BinReader.ReadBytes(16));
            standardBias = this.BinReader.ReadInt32();
            daylightName = new String(this.BinReader.ReadChars(64));
            daylightDate = ConvertByteArrToDateTime(this.BinReader.ReadBytes(16));
            daylightBias = this.BinReader.ReadInt32();
        }

        /// <summary>
        ///     Reads 4B long bias (difference in minutes between UTC in local TZ) value in MNM Extended Info record starting on
        ///     given offset
        /// </summary>
        /// <param name="binreader">Binary reader of opened PCAP file</param>
        /// <param name="offset">Starting offset of TZI record</param>
        /// <returns>Returns bias value from target TZI record</returns>
        private static Int32 GetMnmTziRecordBias(BinaryReader binreader, UInt32 offset)
        {
            //binreader.BaseStream.Position = offset;
            binreader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return binreader.ReadInt32();
        }

        #endregion

        #region MNM Frame Table parsing functions

        /// <summary>
        ///     Gets 8B long time offset from Frame Layout which represents delta offset from base t time in CaptureProcessor File
        ///     Header.
        ///     Actual timestamp is acquired as = t + delta.
        /// </summary>
        /// <param name="offset">Starting offset of frame layout in MNM frame table</param>
        /// <returns>Returns time offset value since beginning of capture</returns>
        private UInt64 GetMnmFlTimeOffsetLocal(Int64 offset)
        {
            //binreader.BaseStream.Position = offset;
            this.BinReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return this.BinReader.ReadUInt64();
        }

        /// <summary>
        ///     Reads 4B long value starting on 8th byte of Frame Layout which represents actual length/size of frame as it was
        ///     received by network adapter
        /// </summary>
        /// <param name="offset">Starting offset of frame layout in MNM frame table</param>
        /// <returns>Returns actual length of frame in bytes as captured by network interface card</returns>
        private Int32 GetMnmFlFrameLengthWire(Int64 offset)
        {
            //binreader.BaseStream.Position = offset + 8;
            this.BinReader.BaseStream.Seek(offset + 8, SeekOrigin.Begin);
            return (Int32) this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads 4B long value starting on 12th byte of Frame Layout which represents length/size of frame data stored in this
        ///     PCAP file
        /// </summary>
        /// <param name="offset">Starting offset of frame layout in MNM frame table</param>
        /// <returns>Returns stored length of frame in bytes in this PCAP file. It could by smaller than previous wire-length.</returns>
        private Int32 GetMnmFlFrameLength(Int64 offset)
        {
            //binreader.BaseStream.Position = offset + 8 + 4;
            this.BinReader.BaseStream.Seek(offset + 12, SeekOrigin.Begin);
            return (Int32) this.BinReader.ReadUInt32();
        }

        /*  /// <summary>
        /// Reads n-bytes starting on 16th byte of Frame Layout  of frame data present in PCAP file
        /// </summary>
        /// <param name="binreader">Binary reader with opened PCAP file</param>
        /// <param name="offset">Starting offset of frame layout in MNM frame table</param>
        /// <param name="length"></param>
        /// <returns>Returns byte array of raw frame data</returns>
        private byte[] GetMNMFlFrameData(UInt32 offset, UInt32 length)
        {
            //binreader.BaseStream.Position = offset + 8 + 4 + 4;
            BinReader.BaseStream.Seek(offset + 16, SeekOrigin.Begin);
            return BinReader.ReadBytes((int)length);
        }
        */

        /// <summary>
        ///     Reads 2B long value starting on zero byte after frame data of Frame Layout which represents of Layer 2 type of
        ///     media on which frame was captured
        /// </summary>
        /// <param name="offset">Starting offset of frame layout in MNM frame table</param>
        /// <returns>
        ///     Returns MNMMediaType enumeration as representation of return value. If version of MNM Pcap file is 2.0 which
        ///     misses per frame information it returns Media FrameType in CaptureProcessor Header.
        /// </returns>
        private MnmMediaType GetMnmFlMediaType(Int64 offset)
        {
            /* MNM version 2.0 issue */
            if (this._mnmVersionMaj == 2 && this._mnmVersionMin == 0)
            {
                return this._mediaType;
            }

            var len = this.GetMnmFlFrameLength(offset);
            //sfs.BinReader.BaseStream.Position = offset + 8 + 4 + 4 + len;
            this.BinReader.BaseStream.Seek(offset + 16 + len, SeekOrigin.Begin);
            return (MnmMediaType) this.BinReader.ReadUInt16();
        }

        /// <summary>
        ///     Reads 4B long value starting 2nd byte after frame data of Frame Layout which which represents index to MNM Process
        ///     Info Table where more detail information about which application sent/received this network frame
        /// </summary>
        /// <param name="offset">Starting offset of frame layout in MNM frame table</param>
        /// <returns>Returns index to MNM Process Info Table or returns 0, if no Process Info Table is present.</returns>
        private Int64 GetMnmFlProcessInfoIndex(Int64 offset)
        {
            if (this._mnmPiOffset == 0)
            {
                return 0;
            }

            var len = this.GetMnmFlFrameLength(offset);
            //sfs.BinReader.BaseStream.Position = offset + 8 + 4 + 4 + len + 2;
            this.BinReader.BaseStream.Seek(offset + 18 + len, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        /// <summary>
        ///     Reads 8B long value starting 6th byte after frame data of Frame Layout which which represents FILETIME when frame
        ///     was received by OS
        /// </summary>
        /// <param name="offset">Starting offset of frame layout in MNM frame table</param>
        /// <returns>
        ///     Returns UTC representation of FILETIME converted according to local timezone settings. Returns 0001-01-01 in
        ///     case of error.
        /// </returns>
        private DateTime GetMnmFlFileTimeStamp(Int64 offset)
        {
            /* If no Extended info is present in PCAP then return minimal DateTime 0001-01-01 */
            if (this._mnmEiOffset == 0)
            {
                return DateTime.MinValue;
            }

            var len = this.GetMnmFlFrameLength(offset);
            //sfs.BinReader.BaseStream.Position = offset + 8 + 4 + 4 + len + 2 + 4;
            this.BinReader.BaseStream.Seek(offset + 22 + len, SeekOrigin.Begin);
            //PrintError("--- " + aaa + " --- " + DateTime.FromFileTimeUtc(aaa).ToString("dd/MM/yyyy HH:mm:ss.FFFFFFF"));
            return DateTime.FromFileTimeUtc(this.BinReader.ReadInt64());
        }

        /// <summary>
        ///     Reads 4B long value starting 14th byte after frame data of Frame Layout which which represents index to MNM Process
        ///     Info Table where more detail information about which application sent/received this network frame
        /// </summary>
        /// <param name="offset">Starting offset of frame layout in MNM frame table</param>
        /// <returns>Returns index to TZI table in MNM Extended Info. Returns 255 in case of error.</returns>
        private Byte GetMnmFlTzi(Int64 offset)
        {
            if (this._mnmEiOffset == 0)
            {
                return Byte.MaxValue;
            }

            var len = this.GetMnmFlFrameLength(offset);
            //sfs.BinReader.BaseStream.Position = offset + 8 + 4 + 4 + len + 2 + 4 + 8;
            this.BinReader.BaseStream.Seek(offset + 30 + len, SeekOrigin.Begin);
            return this.BinReader.ReadByte();
        }

        /// <summary>
        ///     Function returns variables filled with values belonging to one frame in MNM Frame Layout
        /// </summary>
        /// <param name="frame">Target frame</param>
        /// <param name="timeOffsetLocal">Time offset from base time</param>
        /// <param name="frameLengthWire">Real size of frame just as it was processed by capturing engine</param>
        /// <param name="frameLength">Actual size of frame stored in PCAP file</param>
        /// <param name="frameData">Binary frame data</param>
        /// <param name="frameMediaType">Layer 2 type of media on which intercept was done</param>
        /// <param name="framePiIndex">Index to MNM Process Info Table of PCAP file</param>
        /// <param name="frameFileTime">DateTime variable with converted FILETIME time stamp</param>
        /// <param name="frameTziIndex">Index to TZI Table in Extended Info of PCAP file</param>
        private void GetMnmFlFrameRecord(
            PmFrameMnm frame,
            out UInt64 timeOffsetLocal,
            out Int64 frameLengthWire,
            out Int64 frameLength,
            out Byte[] frameData,
            out MnmMediaType frameMediaType,
            out Int64 framePiIndex,
            out DateTime frameFileTime,
            out Int64 frameTziIndex)
        {
            timeOffsetLocal = this.GetMnmFlTimeOffsetLocal(frame.FrameOffset);
            frameLengthWire = this.GetMnmFlFrameLengthWire(frame.FrameOffset);
            frameLength = this.GetMnmFlFrameLength(frame.FrameOffset);
            // frameData = GetMNMFlFrameData(offset, frameLength);
            frameData = frame.L2Data();
            frameMediaType = this.GetMnmFlMediaType(frame.FrameOffset);
            framePiIndex = this.GetMnmFlProcessInfoIndex(frame.FrameOffset);
            frameFileTime = this.GetMnmFlFileTimeStamp(frame.FrameOffset);
            frameTziIndex = this.GetMnmFlTzi(frame.FrameOffset);
        }

        #endregion

        #region MNM printing functions

        /// <summary>
        ///     Print all information stored in MNM CaptureProcessor File Header to Console output
        /// </summary>
        private void PrintMnmCaptureHeader()
        {
            PmConsolePrinter.PrintDebug("\n\nCAPTURE FILE HEADER INFORMATION FOR " + this.PmCapture.FileInfo.FullName);
            "Version> {0}.{1}\tMediaType> {2}".PrintInfoEol(this._mnmVersionMaj, this._mnmVersionMin, this._mediaType);
            "Timestamp> {0}".PrintInfoEol(this._mnmHeaderTimeStamp.ToString("dd/MM/yyyy HH:mm:ss.FFFFFFF"));
            "Frame Table Offset> {0}".PrintInfoEol(this._mnmFtOffset);
            "Frame Table Length> {0}".PrintInfoEol(this._mnmFtLength);
            "User Data Offset> {0}".PrintInfoEol(this.GetMnmCfhUserDataOffset());
            "User Data Length> {0}".PrintInfoEol(this.GetMnmCfhUserDataLength());
            "Comment Info Offset> {0}".PrintInfoEol(this.GetMnmCfhCommentInfoOffset());
            "Comment Info Length> {0}".PrintInfoEol(this.GetMnmCfhCommentInfoLength());
            "Process Info Table Offset> {0}".PrintInfoEol(this._mnmPiOffset);
            "Process Info Table Count> {0}".PrintInfoEol(this._mnmPiCount);
            "Extended Info Offset> {0}".PrintInfoEol(this._mnmEiOffset);
            "Extended Info Length> {0}".PrintInfoEol(this._mnmEiLength);
            "BidirectionalFlow Stats Offset> {0}".PrintInfoEol(this.GetMnmCfhConversationStatsOffset());
            "BidirectionalFlow Stats Length> {0}".PrintInfoEol(this.GetMnmCfhConversationStatsLength());
            "Number of frames> {0}".PrintInfoEol(this._mnmNumberOfFrames);
        }

        /// <summary>
        ///     Print all information stored in MNM Process Infor Table to Console output
        /// </summary>
        private void PrintMnmProcessInfoTable()
        {
            PmConsolePrinter.PrintDebug("\n\nPROCESS INFO TABLE INFORMATION FOR " + this.PmCapture.FileInfo.FullName);
            "Starting offset> {0}".PrintInfoEol(this._mnmPiOffset);
            "Number of records> {0}".PrintInfoEol(this._mnmPiRecords.Count);
            "Version> {0}".PrintInfoEol(this.GetMnmPitVersion());
            /* Print */
            foreach (var ofs in this._mnmPiRecords)
            {
                Console.WriteLine("\nRecord: {0}, offset {1}:", this._mnmPiRecords.IndexOf(ofs), ofs);
                this.PrintMnmPiRecord(ofs);
            }
        }

        /// <summary>
        ///     Print all information in one Process Info record starting on given offset
        /// </summary>
        /// <param name="offset">Starting offset of given Process Info record</param>
        internal void PrintMnmPiRecord(UInt32 offset)
        {
            UInt32 pathSize;
            String applicationPath;
            UInt32 iconSize;
            Byte[] iconData;
            UInt32 pid;
            UInt16 localPort;
            UInt16 remotePort;
            UInt32 isipv6;
            IPAddress localIP;
            IPAddress remoteIP;
            this.GetMnmProcessInfoTableRecord(offset, out pathSize, out applicationPath, out iconSize, out iconData,
                out pid, out localPort, out remotePort, out isipv6, out localIP,
                out remoteIP);
            "\nPath size> {0}".PrintInfoEol(pathSize);
            "Application path> {0}".PrintInfoEol(applicationPath);
            "Icon Size> {0}".PrintInfoEol(iconSize);
            "Icon Data> {0}".PrintInfoEol(iconData);
            "PID> {0}".PrintInfoEol(pid);
            "Local port> {0}".PrintInfoEol(localPort);
            "Remote port> {0}".PrintInfoEol(remotePort);
            "Is IPv6 address> {0}".PrintInfoEol(isipv6 != 0 ? "YES" : "NO");
            "Local IP address> {0}".PrintInfoEol(localIP);
            "Remote IP address> {0}".PrintInfoEol(remoteIP);
        }

        /// <summary>
        ///     Print all information stored in MNM Extended Information to Console output
        /// </summary>
        internal void PrintMnmExtendedInfo()
        {
            PmConsolePrinter.PrintDebug("\n\nEXTENDED INFO INFORMATION FOR " + this.PmCapture.FileInfo.FullName);
            "Version> {0}".PrintInfoEol(this.GetMnmEiVersion());
            "Start time UTC> {0}".PrintInfoEol(this._mnmEiFileTimeStamp.ToString("dd/MM/yyyy HH:mm:ss.FFFFFFF"));
            "Number of country TZIs> {0}".PrintInfoEol(this._mnmEiTziCount);
            foreach (var ofs in this._mnmEiTziRecords)
            {
                Console.WriteLine("\nTZI {0}, offset {1}:", this._mnmEiTziRecords.IndexOf(ofs), ofs);
                this.PrintMnmEiTziRecord(ofs);
            }
        }

        /// <summary>
        ///     Print all information of given TZI record
        /// </summary>
        /// <param name="offset">Starting offset of given TZI record</param>
        internal void PrintMnmEiTziRecord(UInt32 offset)
        {
            Int32 bias;
            String standardName;
            DateTime standardDate;
            Int32 standardBias;
            String daylightName;
            DateTime daylightDate;
            Int32 daylightBias;
            this.GetMnmEiTziRecord(offset, out bias, out standardName, out standardDate, out standardBias,
                out daylightName, out daylightDate, out daylightBias);
            "Bias> {0}".PrintInfoEol(bias);
            "Standard Name> {0}".PrintInfoEol(standardName);
            "Standard Date> {0}".PrintInfoEol(standardDate.ToString("dd/MM/yyyy HH:mm:ss.FFFFFFF"));
            "Standard Bias> {0}".PrintInfoEol(standardBias);
            "Daylight Name> {0}".PrintInfoEol(daylightName);
            "Daylight Date> {0}".PrintInfoEol(daylightDate.ToString("dd/MM/yyyy HH:mm:ss.FFFFFFF"));
            "Daylight Bias> {0}".PrintInfoEol(daylightBias);
        }

        /// <summary>
        ///     Print all information of given frame to Console output
        /// </summary>
        /// <param name="frame">Target frame</param>
        internal void PrintMnmFrameLayoutRecord(PmFrameMnm frame)
        {
            UInt64 timeOffsetLocal;
            Int64 frameLengthWire;
            Int64 frameLength;
            Byte[] frameData;
            MnmMediaType frameMediaType;
            Int64 framePiIndex;
            DateTime frameFileTime;
            Int64 frameTziIndex;

            this.GetMnmFlFrameRecord(frame, out timeOffsetLocal, out frameLengthWire, out frameLength, out frameData,
                out frameMediaType, out framePiIndex, out frameFileTime,
                out frameTziIndex);

            "Time offset local> {0}".PrintInfoEol(timeOffsetLocal);
            "Real time> {0}".PrintInfoEol(ConvertMnmFrameTimeOffsetToRealTime(this._mnmHeaderTimeStamp, timeOffsetLocal)
                .ToString("dd/MM/yyyy HH:mm:ss.FFFFFFF"));
            "Frame length wire> {0}".PrintInfoEol(frameLengthWire);
            "Frame length> {0}".PrintInfoEol(frameLength);
            "Frame data> {0}".PrintInfoEol(frameData);

            "Media type> ".PrintInfo();
            if (this._mnmVersionMaj == 2 && this._mnmVersionMin == 0)
            {
                PmConsolePrinter.PrintError(frameMediaType.ToString());
            }
            else
            {
                "{0}".PrintInfoEol(frameMediaType);
            }


            "Process Info index> ".PrintInfo();
            if (framePiIndex == 0xFFFFFFFF || framePiIndex == 0)
            {
                PmConsolePrinter.PrintError("N/A");
            }
            else
            {
                "{0}".PrintInfoEol(framePiIndex);
            }

            "Time stamp> ".PrintInfo();
            if (frameFileTime.CompareTo(new DateTime(1, 1, 1)) <= 0)
            {
                PmConsolePrinter.PrintError("N/A");
            }
            else
            {
                frameFileTime.ToString("dd/MM/yyyy HH:mm:ss.FFFFFFF").PrintInfoEol();
            }

            "TZI index> ".PrintInfo();
            if (frameTziIndex == Byte.MaxValue)
            {
                PmConsolePrinter.PrintError("N/A");
            }
            else
            {
                "{0}".PrintInfoEol(frameTziIndex);
            }
        }

        /// <summary>
        ///     Prints all information stored in MNM Frame Layout to Console output
        /// </summary>
        internal void PrintMnmFrameTable()
        {
            PmConsolePrinter.PrintDebug("\n\nFRAME TABLE INFORMATION FOR " + this.PmCapture.FileInfo.FullName);
            "Frame Table Offset> {0}".PrintInfoEol(this._mnmFtOffset);
            "Frame Table Length> {0}".PrintInfoEol(this._mnmFtLength);
            "Number of frames> {0}".PrintInfoEol(this._mnmNumberOfFrames);

            foreach (var fr in this.PmCapture.Frames.Where(fr => fr.PmFrameType == PmFrameType.Mnm))
            {
                "\nFrame {0}, offset {1}:".PrintInfoEol(fr.FrameIndex, fr.FrameOffset);
                this.PrintMnmFrameLayoutRecord((PmFrameMnm) fr);
            }
        }

        #endregion

        #region MNM misceleanous functions

        /// <summary>
        ///     Determine whether PCAP contains ProcessInfoTable
        /// </summary>
        /// <returns>Returns true if ProcessInfo Table is present, otherwise false</returns>
        internal Boolean HasProcessInfoTable => this._mnmPiOffset != 0;

        /// <summary>
        ///     Determine whether PCAP contains ExtendedInfo
        /// </summary>
        /// <returns>Returns true if ExtendedInfo is present, otherwise false</returns>
        internal Boolean HasExtendedInfo => this._mnmEiOffset != 0;

        /// <summary>
        ///     Convert 16B long DATETIME data type to C# DateTime type
        /// </summary>
        /// <param name="field">16B long byte array</param>
        /// <returns>DateTime instance created from DATETIME</returns>
        private static DateTime ConvertByteArrToDateTime(Byte[] field)
        {
            //for (int i = 0; i< pole.Length; i++) Console.WriteLine(pole[i]);
            var fsm = new BinaryReader(new MemoryStream(field));
            var year = fsm.ReadUInt16();
            //TODO: Doesn't commenting bellow cause any harm?
            //if (year == 0) year = 2000;
            var month = fsm.ReadUInt16();
            //var dayofweek = fsm.ReadUInt16();
            fsm.ReadUInt16();
            var day = fsm.ReadUInt16();
            var hour = fsm.ReadUInt16();
            var minute = fsm.ReadUInt16();
            var second = fsm.ReadUInt16();
            var milisec = fsm.ReadUInt16();
            //var str = year + "-" + month + "-" + day + " (" + dayofweek + ") " + hour + ":" + minute + ":" + minute + ":" + second + "." + milisec;                        
            return new DateTime(year, month, day, hour, minute, second, milisec);
        }

        /// <summary>
        ///     Convert DateTime variable to 16B long byte-array
        /// </summary>
        /// <param name="dattim">Input DateTime</param>
        /// <returns>16B long byte array consisting of year-month-dayofweek-day-hour-minute-second-milisec</returns>
        private static Byte[] ConvertDateTimeToByteArr(DateTime dattim)
        {
            //PrintError(dattim.ToString("dd/MM/yyyy HH:mm:ss.FFFFFFF"));
            var year = BitConverter.GetBytes(dattim.Year);
            var month = BitConverter.GetBytes(dattim.Month);
            var dayofweek = BitConverter.GetBytes(Convert.ToUInt16(dattim.DayOfWeek));
            var day = BitConverter.GetBytes(dattim.Day);
            var hour = BitConverter.GetBytes(dattim.Hour);
            var minute = BitConverter.GetBytes(dattim.Minute);
            var second = BitConverter.GetBytes(dattim.Second);
            var milisec = BitConverter.GetBytes(dattim.Millisecond);
            //PrintDebug(hour[0]+"-"+hour[1]);            
            var ms = new MemoryStream();
            ms.Write(year, 0, 2);
            ms.Write(month, 0, 2);
            ms.Write(dayofweek, 0, 2);
            ms.Write(day, 0, 2);
            ms.Write(hour, 0, 2);
            ms.Write(minute, 0, 2);
            ms.Write(second, 0, 2);
            ms.Write(milisec, 0, 2);
            return ms.ToArray();
        }

        /// <summary>
        ///     Computes DateTime variable from time base by adding appropriate time offset
        /// </summary>
        /// <param name="timeBase">Base timestamp</param>
        /// <param name="timeOffset">Added time offset</param>
        /// <returns>Returns result of adding time offset to base time</returns>
        private static DateTime ConvertMnmFrameTimeOffsetToRealTime(DateTime timeBase, UInt64 timeOffset) =>
            timeBase.AddTicks((Int64) timeOffset * 10);

        /// <summary>
        ///     Create output file in format of Microsoft Network Monitor
        /// </summary>
        /// <param name="outFile">Filename</param>
        /// <returns></returns>
        private Boolean CreateMnmOutput(String outFile)
        {
            try
            {
                var bw = new BinaryWriter(File.Open(Path.GetFullPath(outFile), FileMode.Create));
                /* TCP Dump format global header variables */
                const UInt32 signatura = 0x55424d47;
                const Byte vermin = 3;
                const Byte vermaj = 2;
                //Beginning signature of file "GMBU" and MNM versions
                bw.Write(signatura);
                bw.Write(vermin);
                bw.Write(vermaj);
                //Used mediatype derived from the first frame
                // bw.Write((UInt16)MediaType);

                var newMediaType = MnmMediaType.Null;
                var firstDateTime = new DateTime();
                var referencePmLinkType = PmLinkType.Null;

                if (this.PmCapture.Frames.Any())
                {
                    referencePmLinkType = this.PmCapture.Frames.First().PmLinkType;
                    newMediaType = ConvertCommonLayer2ToMnmLayer2(referencePmLinkType);
                    firstDateTime = this.PmCapture.Frames.First().TimeStamp;
                }


                bw.Write((UInt16) newMediaType);
                bw.Write(ConvertDateTimeToByteArr(firstDateTime));
                //Frame table starts on following offset
                const Byte mnmFrameHeaderLen = 22;
                var ofs = this.PmCapture.Frames.Aggregate<PmFrameBase, Int64>(0x48,
                    (current, fr) => (current + (fr.IncludedLength + mnmFrameHeaderLen)));
                bw.Write(ofs);
                //Number of frames in file
                bw.Write(this.PmCapture.Frames.Count * 4);
                //No userdata table, comment info table, process info table, extended info table, BidirectionalFlow table
                bw.Write(new Byte[40]);
                //Write Frame Layout
                //  DateTime prvni = RealFrames.First().TimeStamp;


                foreach (var fr in this.PmCapture.Frames.Where(fr => fr.PmLinkType == referencePmLinkType))
                {
                    //Write time offset in microseconds since beginning of sniffing                    
                    var timeofs = (UInt64) (fr.TimeStamp.Subtract(firstDateTime).Ticks / 10);
                    //PrintDebug(fr.TimeStamp.Ticks + " - " + prvni.Ticks + " = " + timeofs);
                    bw.Write(timeofs);
                    //Write frame sizes
                    bw.Write(fr.OriginalLength);
                    bw.Write(fr.IncludedLength);
                    //Write raw data
                    //   bw.Write(GetMNMFlFrameData(fr.FrameOffset, fr.IncludedLength));
                    bw.Write(fr.L2Data());
                    //Write mnmmedia type, that should be all same by now
                    bw.Write((UInt16) newMediaType);
                    //No process index present
                    bw.Write(0xffffffff);
                }

                //Write Frame Table                
                Int64 offset = 0x48;
                foreach (var fr in this.PmCapture.Frames.Where(fr => fr.PmLinkType == referencePmLinkType))
                {
                    //PrintDebug(bw.BaseStream.Position + " --- " + offset);
                    bw.Write(offset);
                    offset += fr.IncludedLength + mnmFrameHeaderLen;
                }

                bw.Close();
            }
            /* If anything bad happened print error and return false */
            catch (Exception ex) //todo to general
            {
                //todo fix
                //Log.Error("PmCaptureProcessorMnm General error", ex);
                PmConsolePrinter.PrintError(ex.Message);
                return false;
            }

            /* Otherwise return true */
            return true;
        }

        #endregion

        #region MNM parsing functions

        /// <summary>
        ///     Initialization function for MNM Frame Table Layout variables
        /// </summary>
        /// <param name="offset">Starting offset of Frame Table Layout in MNM PCAP file</param>
        /// <param name="length">Length of Frame Table Layout in MNM PCAP file</param>
        private void SetFrameTable(UInt32 offset, UInt32 length)
        {
            this._mnmFtOffset = offset;
            this._mnmFtLength = length;
            this._mnmNumberOfFrames = this._mnmFtLength = length / sizeof(UInt32);
        }

        /// <summary>
        ///     Initialization function for MNM Extended Info variables
        /// </summary>
        /// <param name="offset">Starting offset of Extended Information in MNM PCAP file</param>
        /// <param name="length">Length of Extended Information in MNM PCAP file</param>
        private void SetExtendedInfo(UInt32 offset, UInt32 length)
        {
            this._mnmEiOffset = offset;
            this._mnmEiLength = length;
        }

        /// <summary>
        ///     Initialization function for MNM Extended Info variables
        /// </summary>
        /// <param name="offset">Starting offset of Process Information in MNM PCAP file</param>
        /// <param name="count">Length of Process Information in MNM PCAP file</param>
        private void SetProcessInfo(UInt32 offset, UInt32 count)
        {
            this._mnmPiOffset = offset;
            this._mnmPiCount = count;
        }

        /// <summary>
        ///     Function parses MNM CaptureProcessor File Header and initialize all basic MNM properties of Sniff File
        ///     Specification
        /// </summary>
        private void ParseMnmHeader()
        {
            //Parse and store everyhting important
            this._mnmVersionMaj = this.GetMnmCfhVersionMaj();
            this._mnmVersionMin = this.GetMnmCfhVersionMin();
            this._mnmHeaderTimeStamp = this.GetMnmCfhDateTime();
            this.SetFrameTable(this.GetMnmCfhFrameTableOffset(), this.GetMnmCfhFrameTableLength());
            this.SetProcessInfo(this.GetMnmCfhProcessInfoTableOffset(), this.GetMnmCfhProcessInfoTableCount());
            this.SetExtendedInfo(this.GetMnmCfhExtendedInfoOffset(), this.GetMnmCfhExtendedInfoLength());
            /* Convert MNM also to common TCPDump format*/
            this._mediaType = this.GetMnmCfhMediaType();
        }

        /// <summary>
        ///     Function parses MNM Process Info Table and initialize all relevant properties in Sniff File Specification
        /// </summary>
        private void ParseMnmProcessInfoTable()
        {
            /* Verify that Process Info Table is present in PCAP */
            if (this._mnmPiOffset == 0)
            {
                PmConsolePrinter.PrintError("No Process Info Table present in " + this.PmCapture.FileInfo.FullName);
                return;
            }

            /* Initialize list of ofsets for processes in Process Info Table */
            this._mnmPiRecords = new List<UInt32>
            {
                this._mnmPiOffset + 2
            };
            /* Add offset for each record after the first one */
            for (var i = 0; i < this._mnmPiCount - 1; i++)
            {
                this._mnmPiRecords.Add(this.CountMnmPiRecordLength(this._mnmPiRecords.Last()));
            }

            this.CreateAppTagDictionary();
        }

        /// <summary>
        ///     Function parses MNM Extended Info and initialize all relevant properties in Sniff File Specification
        /// </summary>
        private void ParseMnmExtendedInfo()
        {
            /* Verify existence in PCAP */
            if (this._mnmEiOffset == 0)
            {
                PmConsolePrinter.PrintError("No Extended Info present in " + this.PmCapture.FileInfo.FullName);
                return;
            }

            /* Initialize variables and list of TZI records */
            this._mnmEiFileTimeStamp = this.GetMnmEiFileTime();
            this._mnmEiTziCount = this.GetMnmEiCountTzi();
            this._mnmEiTziRecords = new List<UInt32>();
            if (this._mnmEiTziCount == 0)
            {
                return;
            }

            this._mnmEiTziRecords.Add(this._mnmEiOffset + 2 + 8 + 1);
            /* Add offset for each TZI record after the first one */
            for (var i = 0; i < this._mnmEiTziCount - 1; i++)
            {
                this._mnmEiTziRecords.Add(CountEiTziRecordLength(this._mnmEiTziRecords.Last()));
            }
            /* Use first TZI informatio to set TimeZoneIndex for TCPDump output information 
               multiply it by 60 seconds because it is in minutes 
               TODO: But is this right? */

            this._timeZoneOffset = 60 * GetMnmTziRecordBias(this.BinReader, this._mnmEiTziRecords.First());
        }

        /// <summary>
        ///     Function parses MNM Frame Table and initialize all relevant properties in Sniff File Specification, but most
        ///     notable it creates MNMFrameTable
        /// </summary>
        private async Task ParseMnmFrameTable()
        {
            /* Verify existence in PCAP */
            if (this._mnmFtOffset == 0)
            {
                PmConsolePrinter.PrintError("No Frame Table present in " + this.PmCapture.FileInfo.FullName);
                return;
            }

            /* Initialize list of Frame Layout records */
            //sfs.MNMFrameTable = new List<uint>();
            /* Add each Frame Layout offset to list */
            UInt32 j = 0;
            for (UInt32 i = 0; i < this._mnmNumberOfFrames; i++)
            {
                //sfs.BinReader.BaseStream.Position = sfs.MNMFtOffset + i*4;
                this.BinReader.BaseStream.Seek(this._mnmFtOffset + i * 4, SeekOrigin.Begin);
                var flay = this.BinReader.ReadUInt32();
                //sfs.MNMFrameTable.Add(flay);
                /* MNM version 2.0 issue where all frames in file belongs to one MNMMediaType */
                if (this._mnmVersionMaj == 2 && this._mnmVersionMin == 0)
                {
                    await this.PmMetaFramesBufferBlock.SendAsync(new PmFrameMnm(this.PmCapture, j++, flay,
                        this._pmLinkType,
                        ConvertMnmFrameTimeOffsetToRealTime(this._mnmHeaderTimeStamp.AddSeconds(this._timeZoneOffset),
                            this.GetMnmFlTimeOffsetLocal(flay)).ToUniversalTime(),
                        this.GetMnmFlFrameLengthWire(flay), this.GetMnmFlFrameLength(flay)));
                }
                /* All higher versions uses MNMMediaType control and adds only supported types */
                else
                {
                    //foreach (var flay in sfs.MNMFrameTable.Where(flay => IsSupportedLinkType(ConvertMNMLayer2ToTCPDLayer2(GetMNMFlMediaType(sfs, flay)))))
                    var ltype = ConvertMnmLayer2ToCommonLayer2(this.GetMnmFlMediaType(flay));
                    if (!PmSupportedTypes.IsSupportedLinkType(ltype))
                    {
                        continue;
                    }

                    await this.PmMetaFramesBufferBlock.SendAsync(new PmFrameMnm(this.PmCapture, j++, flay, ltype,
                        ConvertMnmFrameTimeOffsetToRealTime(this._mnmHeaderTimeStamp.AddSeconds(this._timeZoneOffset),
                            this.GetMnmFlTimeOffsetLocal(flay)).ToUniversalTime(),
                        this.GetMnmFlFrameLengthWire(flay), this.GetMnmFlFrameLength(flay)));
                }
            }

            //PrintError(sfs.MNMNumberOfFrames + "-" + sfs.RealFrames.Count);
        }

        #endregion

        //vytvoreni dictionary s apptagy podle flowkey
        private void CreateAppTagDictionary()
        {
            foreach (var rec in this._mnmPiRecords)
            {
                var ipSource = new IPEndPoint(this.GetMnmPitLocalAddress(rec), this.GetMnmPitLocalPort(rec));
                var ipDest = new IPEndPoint(this.GetMnmPitRemoteAddress(rec), this.GetMnmPitRemotePort(rec));
                var key = new Tuple<IPEndPoint, IPEndPoint>(ipSource, ipDest);
                var appTag = this.GetAppNameFromPathToApp(this.GetMnmPitPathToApp(rec));

                //Console.WriteLine("Offset " + rec);
                //Console.WriteLine("IP Src " + IPSource + "   IP Dst " + IPDest + "\tApp Tag: " + appTag);
                //if (appTag == null || appTag.Contains("Unknown") || appTag.Contains("Unavailable") || appTag.Contains("System")) // || "System"
                if (appTag == null) // || "System"
                {
                    continue;
                }

                this.PmCaptureMnm.AppTagDictionary.AddOrUpdate(key, s => appTag);
            }
        }

        public PmCaptureMnm PmCaptureMnm => this.PmCapture as PmCaptureMnm;

        private String GetAppNameFromPathToApp(String pathToApp)
        {
            return pathToApp?.Split('\\')?.Last()?.Replace("\0", "").Replace(".", "");
        }

        #endregion
    }
}