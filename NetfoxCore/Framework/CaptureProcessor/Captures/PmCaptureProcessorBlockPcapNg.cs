using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Netfox.Core.Database.Wrappers;
using Netfox.Framework.Models.PmLib;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.CaptureProcessor.Captures
{
    [Serializable]
    internal class PmCaptureProcessorBlockPcapNg : PmCaptureProcessorBlockBase
    {
        #region Constructors

        /// <summary>
        ///     Load constructor - generaly for loading input from file
        ///     (no need to call OpenFile after using this constructor)
        /// </summary>
        public PmCaptureProcessorBlockPcapNg(FileInfo fileInfo)
            : base(new PmCapturePcapNg(fileInfo))
        {
        }

        #endregion

        #region PcapNgLinkTypes

        /// <summary>
        ///     Pcap NG interface Link types
        /// </summary>
        internal enum PcapNgLinkType
        {
            LinktypeNull = 0,
            LinktypeEthernet = 1,
            LinktypeExpEthernet = 2,
            LinktypeAx25 = 3,
            LinktypePronet = 4,
            LinktypeChaos = 5,
            LinktypeTokenRing = 6,
            LinktypeArcnet = 7,
            LinktypeSlip = 8,
            LinktypePpp = 9,
            LinktypeFddi = 10,
            LinktypePppHdlc = 50,
            LinktypePppEther = 51,
            LinktypeSymantecFirewall = 99,
            LinktypeAtmRfc1483 = 100,
            LinktypeRaw = 101,
            LinktypeSlipBsdos = 102,
            LinktypePppBsdos = 103,
            LinktypeCiscoHdlc = 104,
            LinktypeIeee80211 = 105,
            LinktypeAtmClip = 106,
            LinktypeFrelay = 107,
            LinktypeLoop = 108,
            LinktypeEnc = 109,
            LinktypeLane8023 = 110,
            LinktypeHippi = 111,
            LinktypeHdlc = 112,
            LinktypeLinuxSll = 113,
            LinktypeLtalk = 114,
            LinktypeEconet = 115,
            LinktypeIpfilter = 116,
            LinktypePflog = 117,
            LinktypeCiscoIos = 118,
            LinktypePrismHeader = 119,
            LinktypeAironetHeader = 120,
            LinktypeHhdlc = 121,
            LinktypeIPOverFc = 122,
            LinktypeSunatm = 123,
            LinktypeRio = 124,
            LinktypePciExp = 125,
            LinktypeAurora = 126,
            LinktypeIeee80211Radio = 127,
            LinktypeTzsp = 128,
            LinktypeArcnetLinux = 129,
            LinktypeJuniperMlppp = 130,
            LinktypeJuniperMlfr = 131,
            LinktypeJuniperEs = 132,
            LinktypeJuniperGgsn = 133,
            LinktypeJuniperMfr = 134,
            LinktypeJuniperAtm2 = 135,
            LinktypeJuniperServices = 136,
            LinktypeJuniperAtm1 = 137,
            LinktypeAppleIPOverIeee1394 = 138,
            LinktypeMtp2WithPhdr = 139,
            LinktypeMtp2 = 140,
            LinktypeMtp3 = 141,
            LinktypeSccp = 142,
            LinktypeDocsis = 143,
            LinktypeLinuxIrda = 144,
            LinktypeIbmSp = 145,
            LinktypeIbmSn = 146
        }

        #endregion

        #region exceptions

        [Serializable]
        public class PcapNgParsingErrorException : Exception
        {
            public PcapNgParsingErrorException(String message) : base(message)
            {
            }

            protected PcapNgParsingErrorException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
            {
            }
        }

        [Serializable]
        public class PcapNgWriteException : Exception
        {
            public PcapNgWriteException(String message) : base(message)
            {
            }

            protected PcapNgWriteException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
            {
            }
        }

        #endregion

        #region PcapNGfileFormatVariables

        private readonly List<List<PcapNgInterface>> _ifaces = new List<List<PcapNgInterface>>(); // packets interfaces

        private const UInt16 MajorVersion = 0x01;
        private const UInt16 MinorVersion = 0x00;

        private readonly DateTime _unixDateTimeBase = new DateTime(1970, 01, 01, 0, 0, 0);

        #endregion

        #region IPMCaptureFileIOMethods

        /// <summary>
        ///     Initialize FrameTables and appropriate FrameVectors from input file
        /// </summary>
        protected override async Task<Boolean> ParseCaptureFile()
        {
            try
            {
                await this.ParseFile();
            }
            catch (Exception ex) //todo to general
            {
                //todo fix
                //Log.Error("PmCaptureProcessorPcapNg General error", ex);
                PmConsolePrinter.PrintError("Error>\n" + ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Store current included frames to file (type is specified by class instance)
        /// </summary>
        public override Boolean WriteCaptureFile(String outputFile) => this.CreatePcapNgOutput(outputFile);

        public override DateTime? GetFirstTimeStamp()
        {
            //TODO: Implement this!
            throw new NotImplementedException();
        }

        #endregion

        #region PcapNgtoSupprotedtypesConversionMethods

        private static PmLinkType ConvertPcapNgLayer2ToCommonLayer2(PcapNgLinkType linkType)
        {
            switch (linkType)
            {
                case PcapNgLinkType.LinktypeEthernet:
                    return PmLinkType.Ethernet;
                case PcapNgLinkType.LinktypeFddi:
                    return PmLinkType.Fddi;
                case PcapNgLinkType.LinktypeRaw:
                    return PmLinkType.Raw;
                case PcapNgLinkType.LinktypeIeee80211:
                    return PmLinkType.Ieee80211;
                case PcapNgLinkType.LinktypeAtmRfc1483:
                    return PmLinkType.AtmRfc1483;
                default:
                    return PmLinkType.Null;
            }
        }

        private static PcapNgLinkType ConvertCommonLayer2ToPcapNgLayer2(PmLinkType pmLinkType)
        {
            switch (pmLinkType)
            {
                case PmLinkType.Ethernet:
                    return PcapNgLinkType.LinktypeEthernet;
                case PmLinkType.Fddi:
                    return PcapNgLinkType.LinktypeFddi;
                case PmLinkType.Raw:
                    return PcapNgLinkType.LinktypeRaw;
                case PmLinkType.Ieee80211:
                    return PcapNgLinkType.LinktypeIeee80211;
                case PmLinkType.AtmRfc1483:
                    return PcapNgLinkType.LinktypeAtmRfc1483;
                default:
                    return PcapNgLinkType.LinktypeNull;
            }
        }

        #endregion

        #region PcapNgParsing

        #region ByteOrdeSwapMethods

        /// <summary>
        ///     Swap 16-bit number byte order
        /// </summary>
        private static UInt16 Swap(UInt16 input) => ((UInt16) (((0xFF00 & input) >> 8) | ((0x00FF & input) << 8)));

        /// <summary>
        ///     Swap 32-bit number byte order
        /// </summary>
        private static UInt32 Swap(UInt32 input) => ((0xFF000000 & input) >> 24) | ((0x00FF0000 & input) >> 8) |
                                                    ((0x0000FF00 & input) << 8) | ((0x000000FF & input) << 24);

        /// <summary>
        ///     Swap 64-bit number byte order
        /// </summary>
        private static UInt64 Swap(UInt64 input)
            =>
                ((0x00000000000000FF) & (input >> 56) | (0x000000000000FF00) & (input >> 40) |
                 (0x0000000000FF0000) & (input >> 24) | (0x00000000FF000000) & (input >> 8)
                 | (0x000000FF00000000) & (input << 8) | (0x0000FF0000000000) & (input << 24) |
                 (0x00FF000000000000) & (input << 40) | (0xFF00000000000000) & (input << 56));

        #endregion

        /// <summary>
        ///     Pcap-ng file blocks types
        /// </summary>
        private enum PcapNgBlockType
        {
            SectionHeader,
            InterfaceDescription,
            PacketBlock,
            SimplePacket,
            EnhancedPacket,
            InterfaceStatistics,
            NameResolution,
            Unknown
        }

        #region GeneralBlockParsing

        /// <summary>
        ///     Read general block type value
        /// </summary>
        private Int64 GetGeneralBlockType(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var blockType = this.BinReader.ReadUInt32();
            return swapValue ? Swap(blockType) : blockType;
        }

        /// <summary>
        ///     Read general block length
        /// </summary>
        private Int64 GetGeneralBlockTotalLength(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 4, SeekOrigin.Begin);
            var totalLength = this.BinReader.ReadUInt32();
            return swapValue ? Swap(totalLength) : totalLength;
        }

        /// <summary>
        ///     Adjust block length to count new offset
        /// </summary>
        private Int64 AdjustLength(Int64 blockLength)
        {
            switch (blockLength % 4)
            {
                case 0:
                    return blockLength;
                case 1:
                    return blockLength + 3;
                case 2:
                    return blockLength + 2;
                case 3:
                    return blockLength + 1;
                default:
                    return blockLength;
            }
        }

        /// <summary>
        ///     Count padding length that had to be added to current data
        /// </summary>
        private UInt32 CountPadding(UInt32 blockLength)
        {
            switch (blockLength % 4)
            {
                case 0:
                    return 0;
                case 1:
                    return 3;
                case 2:
                    return 2;
                case 3:
                    return 1;
                default:
                    return 0;
            }
        }

        /// <summary>
        ///     Count next starting offset behind current block (at current offset)
        /// </summary>
        private Int64 GetNextGeneralBlokOffset(Int64 offset, Boolean swapValue) =>
            offset + this.AdjustLength(this.GetGeneralBlockTotalLength(offset, swapValue));

        /// <summary>
        ///     Converts general block value included in file to enumaration type
        /// </summary>
        private PcapNgBlockType RecognizeGeneralBlock(Int64 offset, Boolean swapValue)
        {
            var blockType = this.GetGeneralBlockType(offset, swapValue);

            // PmConsolePrinter.PrintDebug("Block type\t: " + blockType);

            switch (blockType)
            {
                case 0x0A0D0D0A:
                    return PcapNgBlockType.SectionHeader;
                case 0x00000001:
                    return PcapNgBlockType.InterfaceDescription;
                case 0x00000002:
                    return PcapNgBlockType.PacketBlock;
                case 0x00000003:
                    return PcapNgBlockType.SimplePacket;
                case 0x00000005:
                    return PcapNgBlockType.InterfaceStatistics;
                case 0x00000006:
                    return PcapNgBlockType.EnhancedPacket;
                case 0x00000004:
                    return PcapNgBlockType.NameResolution;
            }

            return PcapNgBlockType.Unknown;
        }

        #endregion

        #region ParseOptionBlock

        private UInt16 GetOptionCode(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var optionCode = this.BinReader.ReadUInt16();
            return swapValue ? Swap(optionCode) : optionCode;
        }

        private UInt16 GetOptionLength(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 2, SeekOrigin.Begin);
            var optionLength = this.BinReader.ReadUInt16();
            return swapValue ? Swap(optionLength) : optionLength;
        }

        private UInt64 GetOptionUint64(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var num = this.BinReader.ReadUInt64();
            return swapValue ? Swap(num) : num;
        }

        private Byte GetOptionByte(Int64 offset)
        {
            this.BinReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return this.BinReader.ReadByte();
        }

        private String GetOptionString(Int64 offset, UInt32 length)
        {
            var stringBuffer = new Byte[length];
            this.BinReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            this.BinReader.Read(stringBuffer, 0, (Int32) length);
            return Encoding.UTF8.GetString(stringBuffer, 0, (Int32) length);
        }

        private Byte[] GetOptionBytes(Int64 offset, UInt32 length)
        {
            var dataBuffer = new Byte[length];
            this.BinReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            this.BinReader.Read(dataBuffer, 0, (Int32) length);
            return dataBuffer;
        }

        #endregion

        #region SectionHeaderBlock

        private UInt32 GetByteOrderMagic(Int64 offset)
        {
            this.BinReader.BaseStream.Seek(offset + 8, SeekOrigin.Begin);
            return this.BinReader.ReadUInt32();
        }

        private UInt32 GetVersion(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 12, SeekOrigin.Begin);
            var versionMa = this.BinReader.ReadUInt16();
            var versionMi = this.BinReader.ReadUInt16();

            if (swapValue)
            {
                versionMa = Swap(versionMa);
                versionMi = Swap(versionMi);
            }

            return (((UInt32) versionMa) << 16) | versionMi;
        }

        private void ParseSectionHeader(Int64 currentOffset, out Boolean swapEndian)
        {
            var byteOrderMagic = this.GetByteOrderMagic(currentOffset);
            if (byteOrderMagic == 0x4D3C2B1A)
            {
                swapEndian = true;
            }
            else if (byteOrderMagic == 0x1A2B3C4D)
            {
                swapEndian = false;
            }
            else
            {
                throw new PcapNgParsingErrorException("Unknown BYTE ORDER MAGIC VALUE.");
            }

            // Console.WriteLine(GetVersion(currentOffset, false));

            if (this.GetVersion(currentOffset, false) != 0x010000)
            {
                throw new PcapNgParsingErrorException("Unsupported file version.");
            }
        }

        #endregion

        #region InterfaceDescriptionBlock

        private UInt16 GetInterfaceLinkType(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 8, SeekOrigin.Begin);
            var linkType = this.BinReader.ReadUInt16();
            return swapValue ? Swap(linkType) : linkType;
        }

        private UInt32 GetInterfaceSnapLen(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 12, SeekOrigin.Begin);
            var snapLen = this.BinReader.ReadUInt32();
            return swapValue ? Swap(snapLen) : snapLen;
        }

        private void ParseInterfaceDescriptionBlock(Int64 offset, Boolean swapValue, UInt16 ifaceId,
            List<PcapNgInterface> sectionIfaceList)
        {
            var linkType = this.GetInterfaceLinkType(offset, swapValue);
            var snapLen = this.GetInterfaceSnapLen(offset, swapValue);

            var newIface = new PcapNgInterface(snapLen, linkType, ifaceId);
            sectionIfaceList.Add(newIface);

            var blockLen = this.GetGeneralBlockTotalLength(offset, swapValue);

            var currentOffset = offset + 16;
            var eofBlockOffset = offset + blockLen - 4;

            // PmConsolePrinter.PrintDebug("Interface description block " + currentOffset + " " + eofBlockOffset);

            var eofFound = false;
            while (!eofFound && currentOffset < eofBlockOffset)
            {
                var optionCode = this.GetOptionCode(currentOffset, swapValue);
                var optionLength = this.GetOptionLength(currentOffset, swapValue);

                var dataOffset = currentOffset + 4;

                //Console.WriteLine("Option :" + (PcapNgInterface.InterfaceOptions)optionCode + " " + optionLength + " Lenght : "+ optionLength);

                switch ((PcapNgInterface.InterfaceOptions) optionCode)
                {
                    case PcapNgInterface.InterfaceOptions.OptEndofopt:
                        eofFound = true;
                        break;
                    case PcapNgInterface.InterfaceOptions.IfName:
                        newIface.IfName = this.GetOptionString(dataOffset, optionLength);
                        break;
                    case PcapNgInterface.InterfaceOptions.IfDescription:
                        newIface.IfDescription = this.GetOptionString(dataOffset, optionLength);
                        break;
                    case PcapNgInterface.InterfaceOptions.IfIPv4Addr:
                        newIface.Addresses.Add(new IPAddressEF(this.GetOptionBytes(dataOffset, optionLength), 4));
                        break;
                    case PcapNgInterface.InterfaceOptions.IfIPv6Addr:
                        newIface.Addresses.Add(new IPAddressEF(this.GetOptionBytes(dataOffset, optionLength), 4));
                        break;
                    case PcapNgInterface.InterfaceOptions.IfMaCaddr:
                        newIface.MacAddress = this.GetOptionBytes(dataOffset, optionLength);
                        break;
                    case PcapNgInterface.InterfaceOptions.IfEuIaddr:
                        newIface.EuiAddr = this.GetOptionBytes(dataOffset, optionLength);
                        break;
                    case PcapNgInterface.InterfaceOptions.IfSpeed:
                        newIface.Speed = this.GetOptionUint64(dataOffset, swapValue);
                        break;
                    case PcapNgInterface.InterfaceOptions.IfTsresol:
                        newIface.Tsresol = this.GetOptionByte(dataOffset);
                        newIface.HasTsresol = true;
                        break;
                    case PcapNgInterface.InterfaceOptions.IfTzone:
                        newIface.Tzone = this.GetOptionUint64(dataOffset, swapValue);
                        break;
                    case PcapNgInterface.InterfaceOptions.IfFilter:
                        newIface.Filter = this.GetOptionBytes(dataOffset, optionLength);
                        break;
                    case PcapNgInterface.InterfaceOptions.IfOs:
                        newIface.Os = this.GetOptionString(dataOffset, optionLength);
                        break;
                    case PcapNgInterface.InterfaceOptions.IfFcslen:
                        newIface.Fcslen = this.GetOptionByte(dataOffset);
                        break;
                    case PcapNgInterface.InterfaceOptions.IfTsoffset:
                        newIface.Tsoffset = this.GetOptionUint64(dataOffset, swapValue);
                        break;
                }

                currentOffset += this.AdjustLength(optionLength) + 4;
            }
        }

        #endregion

        private DateTime ConvertUint64ToTimeStamp(UInt64 data, PcapNgInterface iface)
        {
            UInt64 mult;
            if (iface.HasTsresol)
            {
                var tsresol = iface.Tsresol;
                var pow = (tsresol & 0x7F);
                if ((tsresol & 0x80) == 0) // negPower of 10
                {
                    mult = (UInt64) Math.Pow(10, pow);
                }
                else // negPower of 2
                {
                    mult = (UInt64) Math.Pow(2, pow);
                }
            }
            else
            {
                mult = 1000000;
            }

            var sec = data / mult;
            var frac = data % mult;
            sec += iface.Tzone;

            const UInt32 ticksPerSecond = 10000000;

            var fracMult = ticksPerSecond / mult;

            //  Console.WriteLine("Ticks per second : " + DateTime.TicksPerSecond);


            var pTime = new DateTime(1970, 01, 01, 0, 0, 0);
            pTime = pTime.AddSeconds(sec);
            pTime = pTime.AddTicks((Int64) (frac * fracMult));
            return pTime;
        }

        #region ParseEnhancedPacketBlock

        private UInt32 GetEhcPacketBlockInterfaceId(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 8, SeekOrigin.Begin);
            var interfaceId = this.BinReader.ReadUInt32();
            return swapValue ? Swap(interfaceId) : interfaceId;
        }

        private UInt32 GetEhcPacketBlockTimestampH(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 12, SeekOrigin.Begin);
            var value = this.BinReader.ReadUInt32();
            return swapValue ? Swap(value) : value;
        }

        private UInt32 GetEhcPacketBlockTimestampL(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 16, SeekOrigin.Begin);
            var value = this.BinReader.ReadUInt32();
            return swapValue ? Swap(value) : value;
        }

        private Int64 GetEhcPacketBlockCapturedLen(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 20, SeekOrigin.Begin);
            var capturedLen = this.BinReader.ReadUInt32();
            return swapValue ? Swap(capturedLen) : capturedLen;
        }

        private Int64 GetEhcPacketBlockPacketLen(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 24, SeekOrigin.Begin);
            var packetLen = this.BinReader.ReadUInt32();
            return swapValue ? Swap(packetLen) : packetLen;
        }

        private enum EnhancedPacketBlockOptions
        {
            EpbFlags = 2,
            EpbHash = 3,
            EpbDropcount = 4
        }

        private async Task ParseEnhancedPacketBlock(Int64 offset, Boolean swapValue,
            List<PcapNgInterface> sectionIfaceList, UInt32 index)
        {
            //PmConsolePrinter.PrintDebug("Enhanced Packet Block " + offset);
            var ifaceId = this.GetEhcPacketBlockInterfaceId(offset, swapValue);

            if (sectionIfaceList.Count() <= ifaceId || sectionIfaceList[(Int32) ifaceId].InterfaceId != ifaceId)
            {
                throw new PcapNgParsingErrorException("No such an interface !");
            }

            var iface = sectionIfaceList[(Int32) ifaceId];

            var timeStamp = (UInt64) this.GetEhcPacketBlockTimestampH(offset, swapValue) << 32 |
                            this.GetEhcPacketBlockTimestampL(offset, swapValue);
            var capturedLen = this.GetEhcPacketBlockCapturedLen(offset, swapValue);
            var packetLen = this.GetEhcPacketBlockPacketLen(offset, swapValue);

            var blockLen = this.GetGeneralBlockTotalLength(offset, swapValue);

            var packetTimeStamp = this.ConvertUint64ToTimeStamp(timeStamp, iface);

            var newFrame = new PmFramePcapNg(this.PmCapture, index, offset,
                ConvertPcapNgLayer2ToCommonLayer2((PcapNgLinkType) iface.LinkType), packetTimeStamp, packetLen,
                capturedLen, PmFramePcapNg.FrameBLockType.EnhancedPacket, iface);

            var currentOffset = offset + 28 + this.AdjustLength(capturedLen);
            var eofBlockOffset = offset + blockLen - 4;

            //bool eofFound = false;
            while (currentOffset < eofBlockOffset)
            {
                var optionCode = this.GetOptionCode(currentOffset, swapValue);
                var optionLength = this.GetOptionLength(currentOffset, swapValue);

                var dataOffset = currentOffset + 4;

                switch ((EnhancedPacketBlockOptions) optionCode)
                {
                    case EnhancedPacketBlockOptions.EpbFlags:
                        newFrame.EpbFlags = this.GetOptionBytes(dataOffset, 4);
                        break;
                    case EnhancedPacketBlockOptions.EpbHash:
                        newFrame.EpbHash = this.GetOptionBytes(dataOffset, optionLength);
                        break;
                    case EnhancedPacketBlockOptions.EpbDropcount:
                        newFrame.Dropcount = this.GetOptionUint64(dataOffset, swapValue);
                        break;
                }

                currentOffset += this.AdjustLength(optionLength) + 4;
            }

            await this.PmMetaFramesBufferBlock.SendAsync(newFrame);
        }

        #endregion

        #region ParseSimplyPacketBlock

        private UInt32 GetSimplyPacketBlockPacketLen(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 8, SeekOrigin.Begin);
            var packetLen = this.BinReader.ReadUInt32();
            return swapValue ? Swap(packetLen) : packetLen;
        }

        private async Task ParseSimplyPacketBlock(Int64 offset, Boolean swapValue,
            List<PcapNgInterface> sectionIfaceList, UInt32 index)
        {
            //PmConsolePrinter.PrintDebug("Simply Packet Block " + offset);

            if (!sectionIfaceList.Any())
            {
                throw new PcapNgParsingErrorException("No interface for this packet !");
            }

            var packetLen = this.GetSimplyPacketBlockPacketLen(offset, swapValue);

            var packetTimeStamp = new DateTime(0, 0, 0, 0, 0, 0);

            var newFrame = new PmFramePcapNg(this.PmCapture, index, offset,
                ConvertPcapNgLayer2ToCommonLayer2((PcapNgLinkType) sectionIfaceList.First().LinkType), packetTimeStamp,
                packetLen, packetLen, PmFramePcapNg.FrameBLockType.SimplePacket, sectionIfaceList.First());

            await this.PmMetaFramesBufferBlock.SendAsync(newFrame);
        }

        #endregion

        #region ParsePacketBlock(Obsolete)

        private UInt16 GetPacketBlockInterfaceId(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 8, SeekOrigin.Begin);
            var interfaceId = this.BinReader.ReadUInt16();
            return swapValue ? Swap(interfaceId) : interfaceId;
        }

        private UInt16 GetPacketBlockDropsCount(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 10, SeekOrigin.Begin);
            var interfaceId = this.BinReader.ReadUInt16();
            return swapValue ? Swap(interfaceId) : interfaceId;
        }

        private UInt32 GetPacketBlockTimestampH(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 12, SeekOrigin.Begin);
            var value = this.BinReader.ReadUInt32();
            return swapValue ? Swap(value) : value;
        }

        private UInt32 GetPacketBlockTimestampL(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 16, SeekOrigin.Begin);
            var value = this.BinReader.ReadUInt32();
            return swapValue ? Swap(value) : value;
        }

        private UInt32 GetPacketBlockCapturedLen(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 20, SeekOrigin.Begin);
            var capturedLen = this.BinReader.ReadUInt32();
            return swapValue ? Swap(capturedLen) : capturedLen;
        }

        private Int64 GetPacketBlockPacketLen(Int64 offset, Boolean swapValue)
        {
            this.BinReader.BaseStream.Seek(offset + 24, SeekOrigin.Begin);
            var packetLen = this.BinReader.ReadUInt32();
            return swapValue ? Swap(packetLen) : packetLen;
        }

        private void ParsePacketBlock(Int64 offset, Boolean swapValue, List<PcapNgInterface> sectionIfaceList,
            UInt32 index)
        {
            var interfaceId = this.GetPacketBlockInterfaceId(offset, swapValue);

            if (sectionIfaceList.Count() <= interfaceId || sectionIfaceList[interfaceId].InterfaceId != interfaceId)
            {
                throw new PcapNgParsingErrorException("No such an interface !");
            }

            var iface = sectionIfaceList[interfaceId];
            var dropCount = this.GetPacketBlockDropsCount(offset, swapValue);

            var timeStamp = (UInt64) this.GetPacketBlockTimestampH(offset, swapValue) << 32 |
                            this.GetPacketBlockTimestampL(offset, swapValue);
            var capturedLen = this.GetPacketBlockCapturedLen(offset, swapValue);
            var packetLen = this.GetPacketBlockPacketLen(offset, swapValue);

            var blockLen = this.GetGeneralBlockTotalLength(offset, swapValue);

            var packetTimeStamp = this.ConvertUint64ToTimeStamp(timeStamp, iface);
            //DateTime packetTimeStamp = DateTime.Now;

            var newFrame = new PmFramePcapNg(this.PmCapture, index, offset,
                ConvertPcapNgLayer2ToCommonLayer2((PcapNgLinkType) iface.LinkType), packetTimeStamp, packetLen,
                capturedLen, PmFramePcapNg.FrameBLockType.PacketBlock, iface);


            this.PmCapture.Frames.Add(newFrame);
        }

        #endregion

        private async Task ParseFile()
        {
            Int64 currentOffset = 0;
            UInt32 blockCounter = 0;

            var fileSize = this.BinReader.BaseStream.Length;
            var swapEndian = false;

            UInt16 ifaceIdCounter = 0;

            List<PcapNgInterface> sectionIfaceList = null;

            var unknownBlocksCount = 0;

            UInt32 index = 0;
            while (currentOffset < fileSize)
            {
                var currentBlockType = this.RecognizeGeneralBlock(currentOffset, swapEndian);
                blockCounter++;
                /* PmConsolePrinter.PrintDebug("Block type\t: " + currentBlockType);
                PmConsolePrinter.PrintDebug("Current offset\t: " + currentOffset.ToString());
                PmConsolePrinter.PrintDebug("Total Length\t: " + (GetNextGeneralBlokOffset(currentOffset, swapEndian) - currentOffset).ToString());
                PmConsolePrinter.PrintDebug("Block No.\t: " + blockCounter.ToString());
                PmConsolePrinter.PrintDebugDelimiter();*/

                switch (currentBlockType)
                {
                    case PcapNgBlockType.SectionHeader:
                        this.ParseSectionHeader(currentOffset, out swapEndian);
                        ifaceIdCounter = 0;
                        sectionIfaceList = new List<PcapNgInterface>();
                        this._ifaces.Add(sectionIfaceList);
                        break;
                    case PcapNgBlockType.InterfaceDescription:
                        this.ParseInterfaceDescriptionBlock(currentOffset, swapEndian, ifaceIdCounter,
                            sectionIfaceList);
                        ifaceIdCounter++;
                        break;
                    case PcapNgBlockType.EnhancedPacket:
                        await this.ParseEnhancedPacketBlock(currentOffset, swapEndian, sectionIfaceList, index);
                        index++;
                        break;
                    case PcapNgBlockType.SimplePacket:
                        await this.ParseSimplyPacketBlock(currentOffset, swapEndian, sectionIfaceList, index);
                        index++;
                        break;
                    case PcapNgBlockType.PacketBlock:
                        this.ParsePacketBlock(currentOffset, swapEndian, sectionIfaceList, index);
                        index++;
                        break;
                }

                var newOffset = this.GetNextGeneralBlokOffset(currentOffset, swapEndian);

                if (currentOffset == newOffset)
                {
                    throw new PcapNgParsingErrorException("Broken file ?");
                }

                currentOffset = newOffset;

                if (currentBlockType == PcapNgBlockType.Unknown)
                {
                    unknownBlocksCount++;
                }
            }

            if (unknownBlocksCount > 0)
            {
                throw new PcapNgParsingErrorException(unknownBlocksCount.ToString(CultureInfo.InvariantCulture) +
                                                      " unknown blocks where skipped ...");
            }
        }

        #endregion

        #region PcapNgOutput

        /// <summary>
        ///     Count section header block size without any options (in bytes)
        /// </summary>
        private UInt32 CountSectionHeaderBlockSize()
        {
            UInt32 sectionHeaderSize = 0;

            sectionHeaderSize += 4; // Block type
            sectionHeaderSize += 4; // Block total length
            sectionHeaderSize += 4; // Byte Order magic
            sectionHeaderSize += 2; // Major version
            sectionHeaderSize += 2; // Minor- version
            sectionHeaderSize += 8; // Section length
            sectionHeaderSize += 4; // Block total length

            return sectionHeaderSize;
        }

        /// <summary>
        ///     Write section header block without any options to output file stream
        /// </summary>
        private UInt32 WriteSectionHeaderBlock(BinaryWriter binWriter, UInt64 sectionSize)
        {
            var blockTotalLength = this.CountSectionHeaderBlockSize();
            binWriter.Write((UInt32) 0x0A0D0D0A);
            binWriter.Write(blockTotalLength);
            binWriter.Write((UInt32) 0x1A2B3C4D);
            binWriter.Write(MajorVersion);
            binWriter.Write(MinorVersion);
            binWriter.Write(sectionSize);
            binWriter.Write(blockTotalLength);

            return blockTotalLength;
        }

        // <summary>
        // Write section header block without any options to output file stream
        // Can be used if section size is not known
        // </summary>
        UInt32 WriteSectionHeaderBlock(BinaryWriter binWriter)
        {
            return this.WriteSectionHeaderBlock(binWriter, 0xFFFFFFFFFFFFFFFF);
        }

        /// <summary>
        ///     Count size of interface description block with one option (tsresol)
        /// </summary>
        private UInt32 CountInterfaceDescriptionBlockSize()
        {
            UInt32 interfaceDescriptionBlockSize = 0;

            interfaceDescriptionBlockSize += 4; // Block type
            interfaceDescriptionBlockSize += 4; // Block total length

            interfaceDescriptionBlockSize += 2; // LinkType
            interfaceDescriptionBlockSize += 2; // Reserved

            interfaceDescriptionBlockSize += 4; // SnapLen

            interfaceDescriptionBlockSize += 8; // Option tsresol
            interfaceDescriptionBlockSize += 4; // Option opt_endofopt

            interfaceDescriptionBlockSize += 4; // Block total length

            return interfaceDescriptionBlockSize;
        }

        /// <summary>
        ///     Convert DataTime stamp to value in nanoseconds (can be used in output)
        /// </summary>
        private UInt64 ConvertTimeStampToUint64(DateTime timeStamp)
        {
            var diff = timeStamp - this._unixDateTimeBase;
            return (UInt64) diff.Ticks * 100;
        }

        /// <summary>
        ///     Write interface description block to output file based on specified interface instance
        /// </summary>
        private UInt32 WriteInterfaceDescriptionBlock(BinaryWriter binWriter, PcapNgInterface iface)
        {
            var blockTotalLength = this.CountInterfaceDescriptionBlockSize();
            binWriter.Write((UInt32) 0x00000001); // Block type
            binWriter.Write(blockTotalLength); // Block total length
            binWriter.Write(iface.LinkType); // LinkType
            binWriter.Write((UInt16) 0x0000); // reserved
            binWriter.Write(iface.SnapLen);

            // Option tsresol :
            binWriter.Write((UInt16) PcapNgInterface.InterfaceOptions.IfTsresol);
            binWriter.Write((UInt16) 0x0001);

            binWriter.Write((Byte) 0x09); // in nanoseconds
            binWriter.Write((Byte) 0x00); // padding
            binWriter.Write((Byte) 0x00); // padding
            binWriter.Write((Byte) 0x00); // padding

            // Option opt_endofopt :
            binWriter.Write((UInt32) 0x0000);

            binWriter.Write(blockTotalLength); // Block total length

            return blockTotalLength;
        }

        /// <summary>
        ///     Count size (in bytes) of Enhanced packet block without packet data and any options
        /// </summary>
        private UInt32 CountEnhancedPacketBlockBlockSize()
        {
            UInt32 enhancedPacketBlockBlockSize = 0;

            enhancedPacketBlockBlockSize += 4; // Block type
            enhancedPacketBlockBlockSize += 4; // Block total length
            enhancedPacketBlockBlockSize += 4; // InterfaceID  
            enhancedPacketBlockBlockSize += 4; // Timestamp(High)  
            enhancedPacketBlockBlockSize += 4; // Timestamp(Low)  
            enhancedPacketBlockBlockSize += 4; // CapturedLen  
            enhancedPacketBlockBlockSize += 4; // PacketLen 

            enhancedPacketBlockBlockSize += 4; // Block total length

            return enhancedPacketBlockBlockSize;
        }

        /// <summary>
        ///     Write Enhanced pcket block to output file based on specified frame.
        ///     Interface will one of the virtual interfaces from vIfaces dictionary
        ///     according to frame link type.
        /// </summary>
        private UInt32 WriteEnhancedPacketBlock(BinaryWriter binWriter, PmFrameBase frame,
            Dictionary<PmLinkType, PcapNgInterface> vIfaces)
        {
            var frameData = frame.L2Data();
            if (frameData == null)
            {
                return 0;
            }

            var enhancedPacketBlockLength = this.CountEnhancedPacketBlockBlockSize() + (UInt32) frameData.Length;

            var vIface = vIfaces[frame.PmLinkType];

            binWriter.Write((UInt32) 0x00000006);
            binWriter.Write(enhancedPacketBlockLength);
            binWriter.Write((UInt32) vIface.InterfaceId);

            var timeStampValue = this.ConvertTimeStampToUint64(frame.TimeStamp);

            binWriter.Write((UInt32) (timeStampValue >> 32));
            binWriter.Write((UInt32) (timeStampValue & 0xFFFFFFFF));

            binWriter.Write(frame.IncludedLength);
            //binWriter.Write(frame.OriginalLength);
            frame.L2OffsetCaptureL4 = binWriter.Seek(0, SeekOrigin.Current);
            frame.L3OffsetCaptureL4 = frame.L2OffsetCaptureL4 + (frame.L3Offset - frame.L2Offset);
            frame.L4OffsetCaptureL4 = frame.L4Offset == -1
                ? frame.L4Offset
                : (frame.L2OffsetCaptureL4 + (frame.L4Offset - frame.L2Offset));
            frame.L7OffsetCaptureL4 = frame.L7Offset == -1
                ? frame.L7Offset
                : (frame.L2OffsetCaptureL4 + (frame.L7Offset - frame.L2Offset));
            frame.IsDoneUpdateOffsetsForCaptureL4 = true;
            binWriter.Write(frameData);

            var paddingLen = this.CountPadding((UInt32) frameData.Length);

            for (UInt32 i = 0; i < paddingLen; i++)
            {
                binWriter.Write((Byte) 0x00);
            }

            binWriter.Write(enhancedPacketBlockLength);

            return enhancedPacketBlockLength + paddingLen;
        }

        /// <summary>
        ///     Create PCAP-ng file that includes all frames stored in current instace.
        ///     For each link type will be created "virtual interface" and all frames
        ///     with this link type will in output file belong to it.
        /// </summary>
        private Boolean CreatePcapNgOutput(String outputFile)
        {
            try
            {
                // holds list of all link types inclede in frame vector
                var framesLinkTypes = new List<PmLinkType>();

                // TODO - optimize LINQ ?
                foreach (var fr in this.PmCapture.Frames)
                {
                    if (!framesLinkTypes.Contains(fr.PmLinkType))
                    {
                        framesLinkTypes.Add(fr.PmLinkType);
                    }
                }

                // holds list of new created virtual interfaces :
                var virtualInterfaces = new List<PcapNgInterface>();
                // directory for faster lookup :
                var virtualInterfacesDictionary = new Dictionary<PmLinkType, PcapNgInterface>();

                UInt16 ifaceId = 0;
                foreach (var type in framesLinkTypes)
                {
                    // create new itnerface for each link type :
                    var newIface = new PcapNgInterface((UInt16) ConvertCommonLayer2ToPcapNgLayer2(type), ifaceId);
                    virtualInterfaces.Add(newIface);
                    virtualInterfacesDictionary.Add(type, newIface);
                    ifaceId++;
                }

                // open output file stream :
                var binWriter = new BinaryWriter(File.Open(Path.GetFullPath(outputFile), FileMode.Create,
                    FileAccess.Write, FileShare.ReadWrite));

                // skipt section header for now - will be add at end
                binWriter.BaseStream.Seek(this.CountSectionHeaderBlockSize(), SeekOrigin.Begin);
                var sectionSize = virtualInterfaces.Aggregate<PcapNgInterface, UInt64>(0,
                    (current, iface) => current + this.WriteInterfaceDescriptionBlock(binWriter, iface));

                sectionSize = this.PmCapture.Frames.Aggregate(sectionSize,
                    (current, fr) =>
                        current + this.WriteEnhancedPacketBlock(binWriter, fr, virtualInterfacesDictionary));

                // add section header size (section size is now known) :
                binWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                this.WriteSectionHeaderBlock(binWriter, sectionSize);

                // close output file
                binWriter.Close();
            }
            /* If anything went bad generate exception and return false */
            catch (Exception ex) //todo to general 
            {
                //todo fix
                //Log.Error("PmCaptureProcessorPcapNg General error", ex);
                PmConsolePrinter.PrintError(ex.Message);
                return false;
            }

            /* otherwise return true if everything went good */
            return true;
        }

        #endregion

        #region PcapNgDebugPrints

        private String MacAddressToString(Byte[] address)
        {
            var result = "";
            if (address != null && address.Length == 6)
            {
                for (var i = 0; i < address.Length; i++)
                {
                    // Display the physical address in hexadecimal.
                    result += address[i].ToString("X2");
                    // Insert a hyphen after each byte, unless we are at the end of the 
                    // address.
                    if (i != address.Length - 1)
                    {
                        result += "-";
                    }
                }
            }

            return result;
        }

        internal void PrintInterfacesList()
        {
            PmConsolePrinter.PrintDebug("PCAP-NG INTERFACES FOR FILE " + this.PmCapture.FileInfo.FullName);

            var sectionId = 0;
            foreach (var section in this._ifaces)
            {
                PmConsolePrinter.PrintDebug("SECTION #" + sectionId);
                foreach (var iface in section)
                {
                    "ID> {0}".PrintInfoEol(iface.Id);
                    "SNAPLEN> {0}".PrintInfoEol(iface.SnapLen);
                    "LINKTYPE> {0} \"{1}\"".PrintInfoEol(iface.LinkType, (PcapNgLinkType) iface.LinkType);
                    "IF NAME> \"{0}\"".PrintInfoEol(iface.IfName);
                    "IF DESCRIPTION> \"{0}\"".PrintInfoEol(iface.IfDescription);
                    "ADDRESSES> ".PrintInfoEol();
                    foreach (var address in iface.Addresses)
                    {
                        ("\t\t" + address).PrintInfoEol();
                    }

                    ("MAC ADDRESSES> " + this.MacAddressToString(iface.MacAddress)).PrintInfoEol();
                    "SPEED> {0}".PrintInfoEol(iface.Speed);
                    "OS> \"{0}\"".PrintInfoEol(iface.Os);
                }

                sectionId++;
            }
        }

        #endregion
    }
}