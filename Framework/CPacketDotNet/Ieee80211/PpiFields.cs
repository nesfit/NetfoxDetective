using System;
using System.IO;
using System.Text;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Contains information specific to 802.3 packets.
        /// </summary>
        public class Ppi802_3 : PpiField
        {
            /// <summary>
            ///     Flags for errors detected at the time the packet was captured.
            /// </summary>
            [Flags]
            public enum ErrorFlags : uint
            {
                /// <summary>
                ///     The frames FCS is invalid.
                /// </summary>
                InvalidFcs = 1,

                /// <summary>
                ///     The frame has a sequence error.
                /// </summary>
                SequenceError = 2,

                /// <summary>
                ///     The frame has a symbol error.
                /// </summary>
                SymbolError = 4,

                /// <summary>
                ///     The frame has a data error.
                /// </summary>
                DataError = 8
            }

            /// <summary>
            ///     802.3 specific extension flags.
            /// </summary>
            [Flags]
            public enum StandardFlags : uint
            {
                /// <summary>
                ///     FCS is present at the end of the packet
                /// </summary>
                FcsPresent = 1
            }

            #region Properties
            /// <summary>Type of the field</summary>
            public override PpiFieldType FieldType
            {
                get { return PpiFieldType.Ppi802_3; }
            }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public override int Length
            {
                get { return 8; }
            }

            /// <summary>
            ///     Gets or sets the standard 802.2 flags.
            /// </summary>
            /// <value>
            ///     The standard flags.
            /// </value>
            public StandardFlags Flags { get; set; }

            /// <summary>
            ///     Gets or sets the 802.3 error flags.
            /// </summary>
            /// <value>
            ///     The error flags.
            /// </value>
            public ErrorFlags Errors { get; set; }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public override byte[] Bytes
            {
                get
                {
                    var ms = new MemoryStream();
                    var writer = new BinaryWriter(ms);
                    writer.Write((uint) this.Flags);
                    writer.Write((uint) this.Errors);
                    return ms.ToArray();
                }
            }
            #endregion Properties

            #region Constructors
            /// <summary>
            ///     Initializes a new instance of the <see cref="Ppi802_3" /> class from the
            ///     provided stream.
            /// </summary>
            /// <remarks>
            ///     The position of the BinaryReader's underlying stream will be advanced to the end
            ///     of the PPI field.
            /// </remarks>
            /// <param name='br'>
            ///     The stream the field will be read from
            /// </param>
            public Ppi802_3(BinaryReader br)
            {
                this.Flags = (StandardFlags) br.ReadUInt32();
                this.Errors = (ErrorFlags) br.ReadUInt32();
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="Ppi802_3" /> class.
            /// </summary>
            /// <param name='Flags'>
            ///     Standard Flags.
            /// </param>
            /// <param name='Errors'>
            ///     Error Flags.
            /// </param>
            public Ppi802_3(StandardFlags Flags, ErrorFlags Errors)
            {
                this.Flags = Flags;
                this.Errors = Errors;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="Ppi802_3" /> class.
            /// </summary>
            public Ppi802_3() { }
            #endregion Constructors
        }

        /// <summary>
        ///     The PPI Aggregation field is used to identify which physical interface a frame was collected on
        ///     when multiple capture interfaces are in use.
        /// </summary>
        public class PpiAggregation : PpiField
        {
            #region Properties
            /// <summary>Type of the field</summary>
            public override PpiFieldType FieldType
            {
                get { return PpiFieldType.PpiAggregation; }
            }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public override int Length
            {
                get { return 4; }
            }

            /// <summary>
            ///     Zero-based index of the physical interface the packet was captured from.
            /// </summary>
            /// <value>
            ///     The interface id.
            /// </value>
            public uint InterfaceId { get; set; }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public override byte[] Bytes
            {
                get { return BitConverter.GetBytes(this.InterfaceId); }
            }
            #endregion Properties

            #region Constructors
            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiAggregation" /> class from the
            ///     provided stream.
            /// </summary>
            /// <remarks>
            ///     The position of the BinaryReader's underlying stream will be advanced to the end
            ///     of the PPI field.
            /// </remarks>
            /// <param name='br'>
            ///     The stream the field will be read from
            /// </param>
            public PpiAggregation(BinaryReader br) { this.InterfaceId = br.ReadUInt32(); }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiAggregation" /> class.
            /// </summary>
            /// <param name='InterfaceId'>
            ///     The interface id.
            /// </param>
            public PpiAggregation(uint InterfaceId) { this.InterfaceId = InterfaceId; }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiAggregation" /> class.
            /// </summary>
            public PpiAggregation() { }
            #endregion Constructors
        }

        /// <summary>
        ///     The PPI Capture Info field has been assigned a PPI field type but currently has no defined
        ///     field body.
        /// </summary>
        public class PpiCaptureInfo : PpiField
        {
            #region Properties
            /// <summary>Type of the field</summary>
            public override PpiFieldType FieldType
            {
                get { return PpiFieldType.PpiCaptureInfo; }
            }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public override int Length
            {
                get { return 0; }
            }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public override byte[] Bytes
            {
                get { return new byte[0]; }
            }
            #endregion Properties

            #region Constructors
            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiCaptureInfo" /> class from the
            ///     provided stream.
            /// </summary>
            /// <remarks>
            ///     The position of the BinaryReader's underlying stream will be advanced to the end
            ///     of the PPI field.
            /// </remarks>
            /// <param name='br'>
            ///     The stream the field will be read from
            /// </param>
            public PpiCaptureInfo(BinaryReader br) { }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiCaptureInfo" /> class.
            /// </summary>
            public PpiCaptureInfo() { }
            #endregion Constructors
        }

        /// <summary>
        ///     The PPI Common field contains fields common to all 802.11 specifications.
        ///     This field is loosely based on the Radio Tap header format.
        /// </summary>
        public class PpiCommon : PpiField
        {
            /// <summary>
            ///     Common 802.11 flags.
            /// </summary>
            [Flags]
            public enum CommonFlags : ushort
            {
                /// <summary>
                ///     Defines whether or not an FCS is included at the end of the encapsulated 802.11 frame.
                /// </summary>
                FcsIncludedInFrame = 0x1,

                /// <summary>
                ///     If set the TSF-timer is in milliseconds, if not set the TSF-timer is in microseconds
                /// </summary>
                TimerSynchFunctionInUse = 0x2,

                /// <summary>
                ///     Indicates that the FCS on the encapsulated 802.11 frame is invalid
                /// </summary>
                FailedFcsCheck = 0x4,

                /// <summary>
                ///     Indicates that there was some type of physical error when receiving the packet.
                /// </summary>
                PhysicalError = 0x8
            }

            #region Properties
            /// <summary>Type of the field</summary>
            public override PpiFieldType FieldType
            {
                get { return PpiFieldType.PpiCommon; }
            }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public override int Length
            {
                get { return 20; }
            }

            /// <summary>
            ///     Radiotap-formatted channel flags.
            /// </summary>
            public RadioTapChannelFlags ChannelFlags { get; set; }

            /// <summary>
            ///     Radiotap-formatted channel frequency, in MHz. 0 indicates an invalid value.
            /// </summary>
            /// <value>
            ///     The channel frequency.
            /// </value>
            public UInt16 ChannelFrequency { get; set; }

            /// <summary>
            ///     The common flags.
            /// </summary>
            public CommonFlags Flags { get; set; }

            /// <summary>
            ///     Data rate in multiples of 500 Kbps. 0 indicates an invalid value.
            /// </summary>
            /// <value>
            ///     The data rate.
            /// </value>
            public double Rate { get; set; }

            /// <summary>
            ///     Gets or sets the TSF timer.
            /// </summary>
            /// <value>
            ///     The TSF Timer value.
            /// </value>
            public UInt64 TSFTimer { get; set; }

            /// <summary>
            ///     Gets or sets the Frequency-hopping spread spectrum (FHSS) hopset
            /// </summary>
            /// <value>
            ///     The FHSS hopset.
            /// </value>
            public Byte FhssHopset { get; set; }

            /// <summary>
            ///     Gets or sets the Frequency-hopping spread spectrum (FHSS) pattern.
            /// </summary>
            /// <value>
            ///     The FHSS pattern.
            /// </value>
            public Byte FhssPattern { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal power at antenna.
            /// </summary>
            /// <value>
            ///     The antenna signal power.
            /// </value>
            public SByte AntennaSignalPower { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal noise at antenna
            /// </summary>
            /// <value>
            ///     The antenna signal noise.
            /// </value>
            public SByte AntennaSignalNoise { get; set; }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public override byte[] Bytes
            {
                get
                {
                    var ms = new MemoryStream();
                    var writer = new BinaryWriter(ms);

                    writer.Write(this.TSFTimer);
                    writer.Write((ushort) this.Flags);
                    writer.Write((ushort) (this.Rate*2));
                    writer.Write(this.ChannelFrequency);
                    writer.Write((ushort) this.ChannelFlags);
                    writer.Write(this.FhssHopset);
                    writer.Write(this.FhssPattern);
                    writer.Write(this.AntennaSignalPower);
                    writer.Write(this.AntennaSignalNoise);

                    return ms.ToArray();
                }
            }
            #endregion Properties

            #region Constructors
            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiCommon" /> class from the
            ///     provided stream.
            /// </summary>
            /// <remarks>
            ///     The position of the BinaryReader's underlying stream will be advanced to the end
            ///     of the PPI field.
            /// </remarks>
            /// <param name='br'>
            ///     The stream the field will be read from
            /// </param>
            public PpiCommon(BinaryReader br)
            {
                this.TSFTimer = br.ReadUInt64();
                this.Flags = (CommonFlags) br.ReadUInt16();
                this.Rate = 0.5f*br.ReadUInt16();
                this.ChannelFrequency = br.ReadUInt16();
                this.ChannelFlags = (RadioTapChannelFlags) br.ReadUInt16();
                this.FhssHopset = br.ReadByte();
                this.FhssPattern = br.ReadByte();
                this.AntennaSignalPower = br.ReadSByte();
                this.AntennaSignalNoise = br.ReadSByte();
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiCommon" /> class.
            ///     AntennaSignalPower and AntennaSignalNoise are both set to their minimum value of -128.
            /// </summary>
            public PpiCommon()
            {
                this.AntennaSignalPower = -128;
                this.AntennaSignalNoise = -128;
            }
            #endregion Constructors
        }

        /// <summary>
        ///     Abstract class for all PPI fields
        /// </summary>
        public abstract class PpiField
        {
            #region Public Methods
            /// <summary>
            ///     Parse a PPI indicated by type, from a given BinaryReader
            /// </summary>
            /// <param name="fieldType">
            ///     A <see cref="System.Int32" />
            /// </param>
            /// <param name="br">
            ///     A <see cref="BinaryReader" />
            /// </param>
            /// <param name="fieldLength">
            ///     The maximum number of bytes that the field to be parsed can encompass.
            /// </param>
            /// <returns>
            ///     A <see cref="PpiField" />
            /// </returns>
            public static PpiField Parse(int fieldType, BinaryReader br, ushort fieldLength)
            {
                var type = (PpiFieldType) fieldType;
                switch(type)
                {
                    case PpiFieldType.PpiReserved0:
                        return new PpiUnknown(fieldType, br, fieldLength);
                    case PpiFieldType.PpiReserved1:
                        return new PpiUnknown(fieldType, br, fieldLength);
                    case PpiFieldType.PpiCommon:
                        return new PpiCommon(br);
                    case PpiFieldType.PpiMacExtensions:
                        return new PpiMacExtensions(br);
                    case PpiFieldType.PpiMacPhy:
                        return new PpiMacPhy(br);
                    case PpiFieldType.PpiSpectrum:
                        return new PpiSpectrum(br);
                    case PpiFieldType.PpiProcessInfo:
                        return new PpiProcessInfo(br);
                    case PpiFieldType.PpiCaptureInfo:
                        return new PpiCaptureInfo(br);
                    case PpiFieldType.PpiAggregation:
                        return new PpiAggregation(br);
                    case PpiFieldType.Ppi802_3:
                        return new Ppi802_3(br);
                    default:
                        return new PpiUnknown(fieldType, br, fieldLength);
                }
            }
            #endregion Public Methods

            #region Properties
            /// <summary>Type of the field</summary>
            public abstract PpiFieldType FieldType { get; }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public abstract int Length { get; }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public abstract byte[] Bytes { get; }
            #endregion Properties
        }

        /// <summary>
        ///     802.11n MAC Extension flags.
        /// </summary>
        public enum PpiMacExtensionFlags : uint
        {
            /// <summary>
            ///     Indicates the use of Greenfield (or HT) mode. In greenfield mode support for 802.11 a/b/g devices is sacrificed for
            ///     increased efficiency.
            /// </summary>
            GreenField = 0x1,

            /// <summary>
            ///     Indicates the High Throughput (HT) mode. If not set channel width is 20MHz, if set it is 40MHz.
            /// </summary>
            HtIndicator = 0x2,

            /// <summary>
            ///     Indicates the use of a Short Guard Interval (SGI).
            /// </summary>
            RxSgi = 0x4,

            /// <summary>
            ///     Indicates the use of HT Duplicate mode.
            /// </summary>
            DuplicateRx = 0x8,

            /// <summary>
            ///     Indicates the use of MPDU aggregation.
            /// </summary>
            Aggregate = 0x10,

            /// <summary>
            ///     Indicates the presence of more aggregate frames.
            /// </summary>
            MoreAggregates = 0x20,

            /// <summary>
            ///     Indicates there was a CRC error in the A-MPDU delimiter after this frame.
            /// </summary>
            AggregateDelimiterCrc = 0x40
        }

        /// <summary>
        ///     The 802.11n MAC Extension field contains radio information specific to 802.11n.
        /// </summary>
        public class PpiMacExtensions : PpiField
        {
            #region Properties
            /// <summary>Type of the field</summary>
            public override PpiFieldType FieldType
            {
                get { return PpiFieldType.PpiMacExtensions; }
            }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public override int Length
            {
                get { return 12; }
            }

            /// <summary>
            ///     Gets or sets the 802.11n MAC extension flags.
            /// </summary>
            /// <value>
            ///     The flags.
            /// </value>
            public PpiMacExtensionFlags Flags { get; set; }

            /// <summary>
            ///     Gets or sets the A-MPDU identifier.
            /// </summary>
            /// <value>
            ///     the A-MPDU id.
            /// </value>
            public uint AMpduId { get; set; }

            /// <summary>
            ///     Gets or sets the number of zero-length pad delimiters
            /// </summary>
            /// <value>
            ///     The delimiter count.
            /// </value>
            public byte DelimiterCount { get; set; }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public override byte[] Bytes
            {
                get
                {
                    var ms = new MemoryStream();
                    var writer = new BinaryWriter(ms);

                    writer.Write((uint) this.Flags);
                    writer.Write(this.AMpduId);
                    writer.Write(this.DelimiterCount);
                    writer.Write(new byte[3]);

                    return ms.ToArray();
                }
            }
            #endregion Properties

            #region Constructors
            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiMacExtensions" /> class from the
            ///     provided stream.
            /// </summary>
            /// <remarks>
            ///     The position of the BinaryReader's underlying stream will be advanced to the end
            ///     of the PPI field.
            /// </remarks>
            /// <param name='br'>
            ///     The stream the field will be read from
            /// </param>
            public PpiMacExtensions(BinaryReader br)
            {
                this.Flags = (PpiMacExtensionFlags) br.ReadUInt32();
                this.AMpduId = br.ReadUInt32();
                this.DelimiterCount = br.ReadByte();
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiMacExtensions" /> class.
            /// </summary>
            public PpiMacExtensions() { }
            #endregion Constructors
        }

        /// <summary>
        ///     The 802.11n MAC + PHY Extension field contains radio information specific to 802.11n.
        /// </summary>
        public class PpiMacPhy : PpiField
        {
            #region Properties
            /// <summary>Type of the field</summary>
            public override PpiFieldType FieldType
            {
                get { return PpiFieldType.PpiMacPhy; }
            }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public override int Length
            {
                get { return 48; }
            }

            /// <summary>
            ///     Gets or sets the 802.11n MAC extension flags.
            /// </summary>
            /// <value>
            ///     The flags.
            /// </value>
            public PpiMacExtensionFlags Flags { get; set; }

            /// <summary>
            ///     Gets or sets the A-MPDU identifier.
            /// </summary>
            /// <value>
            ///     the A-MPDU id.
            /// </value>
            public uint AMpduId { get; set; }

            /// <summary>
            ///     Gets or sets the number of zero-length pad delimiters
            /// </summary>
            /// <value>
            ///     The delimiter count.
            /// </value>
            public byte DelimiterCount { get; set; }

            /// <summary>
            ///     Gets or sets the modulation coding scheme.
            /// </summary>
            /// <value>
            ///     The modulation coding scheme.
            /// </value>
            public byte ModulationCodingScheme { get; set; }

            /// <summary>
            ///     Gets or sets the number of spatial streams.
            /// </summary>
            /// <value>
            ///     The spatial stream count.
            /// </value>
            public byte SpatialStreamCount { get; set; }

            /// <summary>
            ///     Gets or sets the combined Received Signal Strength Indication (RSSI) value
            ///     from all the active antennas and channels.
            /// </summary>
            /// <value>
            ///     The combined RSSI.
            /// </value>
            public byte RssiCombined { get; set; }

            /// <summary>
            ///     Gets or sets the Received Signal Strength Indication (RSSI) value for the antenna 0, control channel.
            /// </summary>
            /// <value>
            ///     The antenna 0 RSSI value.
            /// </value>
            public byte RssiAntenna0Control { get; set; }

            /// <summary>
            ///     Gets or sets the Received Signal Strength Indication (RSSI) value for the antenna 1, control channel.
            /// </summary>
            /// <value>
            ///     The antenna 1 control channel RSSI value.
            /// </value>
            public byte RssiAntenna1Control { get; set; }

            /// <summary>
            ///     Gets or sets the Received Signal Strength Indication (RSSI) value for the antenna 2, control channel.
            /// </summary>
            /// <value>
            ///     The antenna 2 control channel RSSI value.
            /// </value>
            public byte RssiAntenna2Control { get; set; }

            /// <summary>
            ///     Gets or sets the Received Signal Strength Indication (RSSI) value for the antenna 3, control channel.
            /// </summary>
            /// <value>
            ///     The antenna 3 control channel RSSI value.
            /// </value>
            public byte RssiAntenna3Control { get; set; }

            /// <summary>
            ///     Gets or sets the Received Signal Strength Indication (RSSI) value for the antenna 0, extension channel
            /// </summary>
            /// <value>
            ///     The antenna 0 extension channel RSSI value.
            /// </value>
            public byte RssiAntenna0Ext { get; set; }

            /// <summary>
            ///     Gets or sets the Received Signal Strength Indication (RSSI) value for the antenna 1, extension channel
            /// </summary>
            /// <value>
            ///     The antenna 1 extension channel RSSI value.
            /// </value>
            public byte RssiAntenna1Ext { get; set; }

            /// <summary>
            ///     Gets or sets the Received Signal Strength Indication (RSSI) value for the antenna 2, extension channel
            /// </summary>
            /// <value>
            ///     The antenna 2 extension channel RSSI value.
            /// </value>
            public byte RssiAntenna2Ext { get; set; }

            /// <summary>
            ///     Gets or sets the Received Signal Strength Indication (RSSI) value for the antenna 3, extension channel
            /// </summary>
            /// <value>
            ///     The antenna 3 extension channel RSSI value.
            /// </value>
            public byte RssiAntenna3Ext { get; set; }

            /// <summary>
            ///     Gets or sets the extension channel frequency.
            /// </summary>
            /// <value>
            ///     The extension channel frequency.
            /// </value>
            public ushort ExtensionChannelFrequency { get; set; }

            /// <summary>
            ///     Gets or sets the extension channel flags.
            /// </summary>
            /// <value>
            ///     The extension channel flags.
            /// </value>
            public RadioTapChannelFlags ExtensionChannelFlags { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal power at antenna 0.
            /// </summary>
            /// <value>
            ///     The signal power.
            /// </value>
            public byte DBmAntenna0SignalPower { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal noise at antenna 0.
            /// </summary>
            /// <value>
            ///     The signal noise.
            /// </value>
            public byte DBmAntenna0SignalNoise { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal power at antenna 1.
            /// </summary>
            /// <value>
            ///     The signal power.
            /// </value>
            public byte DBmAntenna1SignalPower { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal noise at antenna 1.
            /// </summary>
            /// <value>
            ///     The signal noise.
            /// </value>
            public byte DBmAntenna1SignalNoise { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal power at antenna 2.
            /// </summary>
            /// <value>
            ///     The signal power.
            /// </value>
            public byte DBmAntenna2SignalPower { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal noise at antenna 2.
            /// </summary>
            /// <value>
            ///     The signal noise.
            /// </value>
            public byte DBmAntenna2SignalNoise { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal power at antenna 3.
            /// </summary>
            /// <value>
            ///     The signal power.
            /// </value>
            public byte DBmAntenna3SignalPower { get; set; }

            /// <summary>
            ///     Gets or sets the RF signal noise at antenna 3.
            /// </summary>
            /// <value>
            ///     The signal noise.
            /// </value>
            public byte DBmAntenna3SignalNoise { get; set; }

            /// <summary>
            ///     Gets or sets the error vector magnitude for Chain 0.
            /// </summary>
            /// <value>
            ///     The error vector magnitude.
            /// </value>
            public uint ErrorVectorMagnitude0 { get; set; }

            /// <summary>
            ///     Gets or sets the error vector magnitude for Chain 1.
            /// </summary>
            /// <value>
            ///     The error vector magnitude.
            /// </value>
            public uint ErrorVectorMagnitude1 { get; set; }

            /// <summary>
            ///     Gets or sets the error vector magnitude for Chain 2.
            /// </summary>
            /// <value>
            ///     The error vector magnitude.
            /// </value>
            public uint ErrorVectorMagnitude2 { get; set; }

            /// <summary>
            ///     Gets or sets the error vector magnitude for Chain 3.
            /// </summary>
            /// <value>
            ///     The error vector magnitude.
            /// </value>
            public uint ErrorVectorMagnitude3 { get; set; }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public override byte[] Bytes
            {
                get
                {
                    var ms = new MemoryStream();
                    var writer = new BinaryWriter(ms);

                    writer.Write(this.AMpduId);
                    writer.Write(this.DelimiterCount);
                    writer.Write(this.ModulationCodingScheme);
                    writer.Write(this.SpatialStreamCount);
                    writer.Write(this.RssiCombined);
                    writer.Write(this.RssiAntenna0Control);
                    writer.Write(this.RssiAntenna1Control);
                    writer.Write(this.RssiAntenna2Control);
                    writer.Write(this.RssiAntenna3Control);
                    writer.Write(this.RssiAntenna0Ext);
                    writer.Write(this.RssiAntenna1Ext);
                    writer.Write(this.RssiAntenna2Ext);
                    writer.Write(this.RssiAntenna3Ext);
                    writer.Write(this.ExtensionChannelFrequency);
                    writer.Write((ushort) this.ExtensionChannelFlags);
                    writer.Write(this.DBmAntenna0SignalPower);
                    writer.Write(this.DBmAntenna0SignalNoise);
                    writer.Write(this.DBmAntenna1SignalPower);
                    writer.Write(this.DBmAntenna1SignalNoise);
                    writer.Write(this.DBmAntenna2SignalPower);
                    writer.Write(this.DBmAntenna2SignalNoise);
                    writer.Write(this.DBmAntenna3SignalPower);
                    writer.Write(this.DBmAntenna3SignalNoise);
                    writer.Write(this.ErrorVectorMagnitude0);
                    writer.Write(this.ErrorVectorMagnitude1);
                    writer.Write(this.ErrorVectorMagnitude2);
                    writer.Write(this.ErrorVectorMagnitude3);

                    return ms.ToArray();
                }
            }
            #endregion Properties

            #region Constructors
            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiMacPhy" /> class from the
            ///     provided stream.
            /// </summary>
            /// <remarks>
            ///     The position of the BinaryReader's underlying stream will be advanced to the end
            ///     of the PPI field.
            /// </remarks>
            /// <param name='br'>
            ///     The stream the field will be read from
            /// </param>
            public PpiMacPhy(BinaryReader br)
            {
                this.AMpduId = br.ReadUInt32();
                this.DelimiterCount = br.ReadByte();
                this.ModulationCodingScheme = br.ReadByte();
                this.SpatialStreamCount = br.ReadByte();
                this.RssiCombined = br.ReadByte();
                this.RssiAntenna0Control = br.ReadByte();
                this.RssiAntenna1Control = br.ReadByte();
                this.RssiAntenna2Control = br.ReadByte();
                this.RssiAntenna3Control = br.ReadByte();
                this.RssiAntenna0Ext = br.ReadByte();
                this.RssiAntenna1Ext = br.ReadByte();
                this.RssiAntenna2Ext = br.ReadByte();
                this.RssiAntenna3Ext = br.ReadByte();
                this.ExtensionChannelFrequency = br.ReadUInt16();
                this.ExtensionChannelFlags = (RadioTapChannelFlags) br.ReadUInt16();
                this.DBmAntenna0SignalPower = br.ReadByte();
                this.DBmAntenna0SignalNoise = br.ReadByte();
                this.DBmAntenna1SignalPower = br.ReadByte();
                this.DBmAntenna1SignalNoise = br.ReadByte();
                this.DBmAntenna2SignalPower = br.ReadByte();
                this.DBmAntenna2SignalNoise = br.ReadByte();
                this.DBmAntenna3SignalPower = br.ReadByte();
                this.DBmAntenna3SignalNoise = br.ReadByte();
                this.ErrorVectorMagnitude0 = br.ReadUInt32();
                this.ErrorVectorMagnitude1 = br.ReadUInt32();
                this.ErrorVectorMagnitude2 = br.ReadUInt32();
                this.ErrorVectorMagnitude3 = br.ReadUInt32();
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiMacPhy" /> class.
            /// </summary>
            public PpiMacPhy() { }
            #endregion Constructors
        }

        /// <summary>
        ///     PPI process info field.
        /// </summary>
        public class PpiProcessInfo : PpiField
        {
            #region Properties
            /// <summary>Type of the field</summary>
            public override PpiFieldType FieldType
            {
                get { return PpiFieldType.PpiProcessInfo; }
            }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public override int Length
            {
                get
                {
                    var processLength = (String.IsNullOrEmpty(this.ProcessPath))? 0 : Encoding.UTF8.GetByteCount(this.ProcessPath);
                    var userLength = (String.IsNullOrEmpty(this.UserName))? 0 : Encoding.UTF8.GetByteCount(this.UserName);
                    var groupLength = (String.IsNullOrEmpty(this.GroupName))? 0 : Encoding.UTF8.GetByteCount(this.GroupName);
                    return 19 + processLength + userLength + groupLength;
                }
            }

            /// <summary>
            ///     Gets or sets the process identifier.
            /// </summary>
            /// <value>
            ///     The process identifier.
            /// </value>
            public uint ProcessId { get; set; }

            /// <summary>
            ///     Gets or sets the thread identifier.
            /// </summary>
            /// <value>
            ///     The thread identifier.
            /// </value>
            public uint ThreadId { get; set; }

            /// <summary>
            ///     Gets or sets the process path.
            /// </summary>
            /// <value>
            ///     The process path.
            /// </value>
            public String ProcessPath { get; set; }

            /// <summary>
            ///     Gets or sets the user identifier.
            /// </summary>
            /// <value>
            ///     The user identifier.
            /// </value>
            public uint UserId { get; set; }

            /// <summary>
            ///     Gets or sets the user name.
            /// </summary>
            /// <value>
            ///     The user name.
            /// </value>
            public String UserName { get; set; }

            /// <summary>
            ///     Gets or sets the group identifier.
            /// </summary>
            /// <value>
            ///     The group identifier.
            /// </value>
            public uint GroupId { get; set; }

            /// <summary>
            ///     Gets or sets the group name.
            /// </summary>
            /// <value>
            ///     The group name.
            /// </value>
            public String GroupName { get; set; }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public override byte[] Bytes
            {
                get
                {
                    var ms = new MemoryStream();
                    var writer = new BinaryWriter(ms);

                    writer.Write(this.ProcessId);
                    writer.Write(this.ThreadId);

                    var pathBytes = Encoding.UTF8.GetBytes(this.ProcessPath ?? String.Empty);
                    writer.Write((byte) pathBytes.Length);
                    writer.Write(pathBytes);

                    writer.Write(this.UserId);

                    var userBytes = Encoding.UTF8.GetBytes(this.UserName ?? String.Empty);
                    writer.Write((byte) userBytes.Length);
                    writer.Write(userBytes);

                    writer.Write(this.GroupId);

                    var groupBytes = Encoding.UTF8.GetBytes(this.GroupName ?? String.Empty);
                    writer.Write((byte) groupBytes.Length);
                    writer.Write(groupBytes);

                    return ms.ToArray();
                }
            }
            #endregion Properties

            #region Constructors
            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiProcessInfo" /> class from the
            ///     provided stream.
            /// </summary>
            /// <remarks>
            ///     The position of the BinaryReader's underlying stream will be advanced to the end
            ///     of the PPI field.
            /// </remarks>
            /// <param name='br'>
            ///     The stream the field will be read from
            /// </param>
            public PpiProcessInfo(BinaryReader br)
            {
                this.ProcessId = br.ReadUInt32();
                this.ThreadId = br.ReadUInt32();

                var pathLength = br.ReadByte();
                this.ProcessPath = Encoding.UTF8.GetString(br.ReadBytes(pathLength));

                this.UserId = br.ReadUInt32();

                var userLength = br.ReadByte();
                this.UserName = Encoding.UTF8.GetString(br.ReadBytes(userLength));

                this.GroupId = br.ReadUInt32();

                var groupLength = br.ReadByte();
                this.GroupName = Encoding.UTF8.GetString(br.ReadBytes(groupLength));
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiProcessInfo" /> class.
            /// </summary>
            public PpiProcessInfo() { }
            #endregion Constructors
        }

        /// <summary>
        ///     The PpiUnknown field class can be used to represent any field types not
        ///     currently supported by PacketDotNet. Any unsupported field types encountered during
        ///     parsing will be stored as PpiUnknown fields.
        /// </summary>
        public class PpiUnknown : PpiField
        {
            #region Properties
            /// <summary>Type of the field</summary>
            public override PpiFieldType FieldType { get; }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public override int Length
            {
                get { return this.Bytes.Length; }
            }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public override byte[] Bytes
            {
                get { return this.UnknownBytes; }
            }

            /// <summary>
            ///     Gets or sets the field data.
            /// </summary>
            /// <value>
            ///     The fields values bytes.
            /// </value>
            public byte[] UnknownBytes { get; set; }
            #endregion Properties

            #region Constructors
            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiUnknown" /> class from the
            ///     provided stream.
            /// </summary>
            /// <remarks>
            ///     The position of the BinaryReader's underlying stream will be advanced to the end
            ///     of the PPI field.
            /// </remarks>
            /// <param name='typeNumber'>
            ///     The PPI field type number
            /// </param>
            /// <param name='br'>
            ///     The stream the field will be read from
            /// </param>
            /// <param name='length'>
            ///     The number of bytes the unknown field contains.
            /// </param>
            public PpiUnknown(int typeNumber, BinaryReader br, int length)
            {
                this.FieldType = (PpiFieldType) typeNumber;
                this.UnknownBytes = br.ReadBytes(length);
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiUnknown" /> class.
            /// </summary>
            /// <param name='typeNumber'>
            ///     The PPI field type number.
            /// </param>
            public PpiUnknown(int typeNumber) { this.FieldType = (PpiFieldType) typeNumber; }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiUnknown" /> class.
            /// </summary>
            /// <param name='typeNumber'>
            ///     The PPI field type number.
            /// </param>
            /// <param name='UnknownBytes'>
            ///     The field data.
            /// </param>
            public PpiUnknown(int typeNumber, byte[] UnknownBytes)
            {
                this.FieldType = (PpiFieldType) typeNumber;
                this.UnknownBytes = UnknownBytes;
            }
            #endregion Constructors
        }

        /// <summary>
        ///     The PPI Spectrum field is intended to be compatible with the sweep records
        ///     returned by the Wi-Spy spectrum analyzer.
        /// </summary>
        public class PpiSpectrum : PpiField
        {
            #region Properties
            /// <summary>Type of the field</summary>
            public override PpiFieldType FieldType
            {
                get { return PpiFieldType.PpiSpectrum; }
            }

            /// <summary>
            ///     Gets the length of the field data.
            /// </summary>
            /// <value>
            ///     The length.
            /// </value>
            public override int Length
            {
                get { return 20 + this.SamplesData.Length; }
            }

            /// <summary>
            ///     Gets or sets the starting frequency in kHz.
            /// </summary>
            /// <value>
            ///     The starting frequency.
            /// </value>
            public uint StartingFrequency { get; set; }

            /// <summary>
            ///     Gets or sets the resolution of each sample in Hz.
            /// </summary>
            /// <value>
            ///     The resolution in Hz.
            /// </value>
            public uint Resolution { get; set; }

            /// <summary>
            ///     Gets or sets the amplitude offset (in 0.001 dBm)
            /// </summary>
            /// <value>
            ///     The amplitude offset.
            /// </value>
            public uint AmplitudeOffset { get; set; }

            /// <summary>
            ///     Gets or sets the amplitude resolution (in .001 dBm)
            /// </summary>
            /// <value>
            ///     The amplitude resolution.
            /// </value>
            public uint AmplitudeResolution { get; set; }

            /// <summary>
            ///     Gets or sets the maximum raw RSSI value reported by the device.
            /// </summary>
            /// <value>
            ///     The maximum rssi.
            /// </value>
            public ushort MaximumRssi { get; set; }

            /// <summary>
            ///     Gets or sets the data samples.
            /// </summary>
            /// <value>
            ///     The data samples.
            /// </value>
            public byte[] SamplesData { get; set; }

            /// <summary>
            ///     Gets the field bytes. This doesn't include the PPI field header.
            /// </summary>
            /// <value>
            ///     The bytes.
            /// </value>
            public override byte[] Bytes
            {
                get
                {
                    var ms = new MemoryStream();
                    var writer = new BinaryWriter(ms);

                    writer.Write(this.StartingFrequency);
                    writer.Write(this.Resolution);
                    writer.Write(this.AmplitudeOffset);
                    writer.Write(this.AmplitudeResolution);
                    writer.Write(this.MaximumRssi);
                    writer.Write((ushort) this.SamplesData.Length);
                    writer.Write(this.SamplesData);

                    return ms.ToArray();
                }
            }
            #endregion Properties

            #region Constructors
            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiSpectrum" /> class from the
            ///     provided stream.
            /// </summary>
            /// <remarks>
            ///     The position of the BinaryReader's underlying stream will be advanced to the end
            ///     of the PPI field.
            /// </remarks>
            /// <param name='br'>
            ///     The stream the field will be read from
            /// </param>
            public PpiSpectrum(BinaryReader br)
            {
                this.StartingFrequency = br.ReadUInt32();
                this.Resolution = br.ReadUInt32();
                this.AmplitudeOffset = br.ReadUInt32();
                this.AmplitudeResolution = br.ReadUInt32();
                this.MaximumRssi = br.ReadUInt16();
                var samplesLength = br.ReadUInt16();
                this.SamplesData = br.ReadBytes(samplesLength);
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="PpiSpectrum" /> class.
            /// </summary>
            public PpiSpectrum() { }
            #endregion Constructors
        }
    }
}