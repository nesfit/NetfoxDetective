using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models;
using PacketDotNet.Utils;

namespace Netfox.Framework.CaptureProcessor.L7PDUTracking.DVB
{
    /// <remarks> 
    /// ETSI EN 302 307 v1.1.2: "Digital Video Broadcasting(DVB); Second generation framing structure, channel coding
    /// and  modulation systems  for  Broadcasting, Interactive Services, News  Gathering and  other broadband
    /// satellite applications" (2006-06)
    /// http://www.etsi.org/deliver/etsi_en/302300_302399/302307/01.01.02_60/en_302307v010102p.pdf
    /// </remarks>
    class BaseBandFrame
    {
        /// <summary>
        /// Base-Band Header placed in between <see cref="ModeAdaptationHeaderL3"/> and UPs.
        /// </summary>
        public readonly BaseBandHeader BaseBandHeader;

        /// <summary>Parent Layer 7 PDU in which this Base-Band Frame is contained.</summary>
        public readonly L7PDU L7PDU;

        /// <summary>
        /// Mode Adaptation Header L3 which is in the begining of this Base-Band Frame.
        /// </summary>
        public readonly ModeAdaptationHeaderL3 ModeAdaptationHeaderL3;

        /// <summary>List of User Packets (UP) contained in this Base-Band Frame.</summary>
        public readonly ImmutableList<GsePacket> UserPackets;

        public BaseBandFrame(
            ModeAdaptationHeaderL3 modeAdaptationHeaderL3,
            BaseBandHeader baseBandHeader,
            ImmutableList<GsePacket> userPackets,
            L7PDU l7PDU)
        {
            this.ModeAdaptationHeaderL3 =
                modeAdaptationHeaderL3 ?? throw new ArgumentNullException(nameof(modeAdaptationHeaderL3));
            this.BaseBandHeader = baseBandHeader ?? throw new ArgumentNullException(nameof(baseBandHeader));
            this.UserPackets = userPackets ?? throw new ArgumentNullException(nameof(userPackets));
            this.L7PDU = l7PDU ?? throw new ArgumentNullException(nameof(l7PDU));
        }

        /// <summary>
        /// Factory method parsing <see cref="BaseBandFrame"/>.
        /// </summary>
        /// <param name="reader"><see cref="PDUStreamReader"/> for stream from which the <see cref="BaseBandFrame"/>
        /// should be parsed.</param>
        /// <exception cref="NotImplementedException">If TS/GS (<see cref="TsGs"/>) is other than Generic Continuous Stream Input.</exception>
        /// <exception cref="InvalidPacketFormatException">If read data does not correspond to valid format.</exception>
        /// <returns>Parsed <see cref="BaseBandFrame"/> object.</returns>
        public static BaseBandFrame Parse(PDUStreamReader reader)
        {
            var mah = ModeAdaptationHeaderL3.Parse(reader);
            var bbh = BaseBandHeader.Parse(reader);

            if (bbh.Matype1.TsGs != TsGs.GenericContinuous)
            {
                throw new NotImplementedException("Only Generic Continuous Stream Input is supported at this moment.");
            }

            var l7PDU = reader.PDUStreamBasedProvider.GetCurrentPDU();

            var userPackets = new List<GsePacket>();
            while (!reader.EndOfPDU)
            {
                try
                {
                    userPackets.Add(GsePacket.Parse(reader));
                }
                catch (InvalidPacketFormatException)
                {
                    // NOTE: Invalid format for GSE packet, in base band frame
                }
            }

            return new BaseBandFrame(mah, bbh, ImmutableList.CreateRange(userPackets), l7PDU);
        }
    }

    /// <summary>
    /// DVB-S2 Mode Adaptation Header L.3
    /// </summary>
    /// <remarks>
    /// <para>"L.3: Receiver Adaptation serial output interface with in-band signalling"</para> 
    /// <para>"For VCM/ACM applications it may be useful to divide a receiver implementation into a demodulation/decoding
    /// device and a baseband processing device.This section defines an interface between these two elements by
    /// including the frame header and ACM quality measurement information as in-band signalling information."</para> 
    /// <para>"The Receiver Header shall consist of 4 bytes."</para>
    /// <a href="http://satlabs.org/pdf/sl_561_Mode_Adaptation_Input_and_Output_Interfaces_for_DVB-S2_Equipment_v1.3.pdf">
    /// Mode Adaptation Input and Output Interfaces for DVB-S2 equipment, SatLabs ref.: sl_561, Version 1.3, SatLabs
    /// Group EEIG 2008</a>
    /// </remarks>
    class ModeAdaptationHeaderL3
    {
        /// <summary>Length of <see cref="ModeAdaptationHeaderL3"/> measured in <see cref="byte"/>s.</summary>
        public const byte HeaderLength = 4;

        /// <summary>
        /// The first byte identifies the start of the Receiver Adaptation packet and shall contain the sequence 0xB8.
        /// </summary>
        public const byte Sync = 0xB8;

        /// <summary>
        /// Measured Carrier to Noise plus Interference ratio for the frame
        /// <para>Set to <b>null</b> if coresponding byte in Mode Adaptation header was 0 (modem unlocked, SNR not
        /// available).</para>
        /// </summary>
        /// <remarks>
        /// 
        /// <list type="bullet">
        /// <item><term>Resolution: </term><description>0.125 dB/LSB</description></item>
        /// <item><term>Range: </term><description>-1.0 … 30.75 dB</description></item>
        /// </list>
        /// </remarks>
        public readonly double? CarrierToNoise;

        public readonly FecFrameSize FecFrameSize;

        /// <summary>Physical-layer frame counter. This counter increments each time a PLHEADER is received/detected.</summary>
        public readonly byte FrameNumber;

        /// <summary>code rates and modulations</summary>
        /// <seealso cref="DVB.Modcod"/>
        public readonly Modcod Modcod;

        public readonly bool PilotsConfiguration;

        public ModeAdaptationHeaderL3(
            bool pilotsConfiguration,
            FecFrameSize fecFrameSize,
            Modcod modcod,
            double? carrierToNoise,
            byte frameNumber)
        {
            this.PilotsConfiguration = pilotsConfiguration;
            this.FecFrameSize = fecFrameSize;
            this.Modcod = modcod;
            this.CarrierToNoise = carrierToNoise;
            this.FrameNumber = frameNumber;
        }

        /// <summary>
        /// Factory method parsing <see cref="ModeAdaptationHeaderL3"/>.
        /// </summary>
        /// 
        /// <param name="reader"><see cref="PDUStreamReader"/> for stream from which the <see cref="ModeAdaptationHeaderL3"/>
        /// should be parsed.</param>
        /// <exception cref="InvalidPacketFormatException">If read data does not correspond to valid format.</exception>
        /// <returns>Parsed <see cref="ModeAdaptationHeaderL3"/> object.</returns>
        public static ModeAdaptationHeaderL3 Parse(PDUStreamReader reader)
        {
            // Byte 0, SYNC
            var b = reader.ReadByte();
            if (b != Sync)
            {
                throw new InvalidPacketFormatException($"SYNC byte mismatch at byte 0. {b}!={Sync}");
            }

            // Byte 1, ACM command, received MODCOD and frame type
            // Individual bits of ACM command byte are indexed from MSB to LSB according to the standard
            // (Table 5-3 in SatLabs ref.: sl_561 v1.3 
            // http://satlabs.org/pdf/sl_561_Mode_Adaptation_Input_and_Output_Interfaces_for_DVB-S2_Equipment_v1.3.pdf).
            // Bit 0 is therefore MSB and bit 7 is LSB. Indexing of this field is inverted in comparison
            // with other fields. The other fields have even explicit statement of "Bit 0: LSB".
            b = reader.ReadByte();
            // Bit 0 (7): Not used, set to 0
            if ((b & 0b1000_0000) != 0)
            {
                throw new InvalidPacketFormatException($"Unexpected bit 1 at byte 1, bit 7.");
            }

            // Bit 1 (6): TYPE(0) - pilots configuration (0 = no pilots, 1 = pilots)
            var pilotsConfiguration = (b & 0b0100_0000) == 1;
            // Bit 2 (5): TYPE(1) - FECFRAME sizes (0 = normal; 1 = short)
            var fecFrameSize = (b & 0b0010_0000) == 0 ? FecFrameSize.Normal : FecFrameSize.Short;
            // Bit 3-7: MODCOD
            // ModCod enum has 32 values, therefore we use Enum.Parse instead of Enum.TryParse.
            var modcod = (Modcod) Enum.Parse(typeof(Modcod), (b & 0b0001_1111).ToString());

            // Byte 2, CNI
            // 0: modem unlocked, SNR not available
            // 1:   -1.0   dB
            // 2:   -0.875 dB
            // 254: 30.625 dB
            b = reader.ReadByte();
            var carrierToNoise = b == 0 ? null : (double?) (-1.125 + b * 0.125);

            // Byte 3, Frame number (PL FRAME ID)
            var frameNumber = reader.ReadByte();

            return new ModeAdaptationHeaderL3(pilotsConfiguration, fecFrameSize, modcod, carrierToNoise, frameNumber);
        }
    }

    /// <summary>(Forward Error Correction) This field is part of <see cref="ModeAdaptationHeaderL3"/>.</summary>
    public enum FecFrameSize
    {
        /// 64 800 bits
        Normal,

        /// 16 200 bits
        Short
    }

    /// <summary>MODCOD identifies code rates and modulations according to table 12 in
    /// <a href="http://www.etsi.org/deliver/etsi_en/302300_302399/302307/01.01.02_60/en_302307v010102p.pdf#page=29">
    /// ETSI EN 302 307 v1.1.2</a>.
    /// This field is part of <see cref="ModeAdaptationHeaderL3"/>.</summary>
    /// <remarks> 
    /// ETSI EN 302 307 v1.1.2: "Digital Video Broadcasting(DVB); Second generation framing structure, channel coding
    /// and  modulation systems  for  Broadcasting, Interactive Services, News  Gathering and  other broadband
    /// satellite applications" (2006-06)
    /// </remarks>
    public enum Modcod
    {
        ModeDummyPhysicalLayerFrame = 0,
        ModeQpsk14 = 1,
        ModeQpsk13 = 2,
        ModeQpsk25 = 3,
        ModeQpsk12 = 4,
        ModeQpsk35 = 5,
        ModeQpsk23 = 6,
        ModeQpsk34 = 7,
        ModeQpsk45 = 8,
        ModeQpsk56 = 9,
        ModeQpsk89 = 10,
        ModeQpsk910 = 11,
        Mode8Psk35 = 12,
        Mode8Psk23 = 13,
        Mode8Psk34 = 14,
        Mode8Psk56 = 15,
        Mode8Psk89 = 16,
        Mode8Psk910 = 17,
        Mode16Apsk23 = 18,
        Mode16Apsk34 = 19,
        Mode16Apsk45 = 20,
        Mode16Apsk56 = 21,
        Mode16Apsk89 = 22,
        Mode16Apsk910 = 23,
        Mode32Apsk34 = 24,
        Mode32Apsk45 = 25,
        Mode32Apsk56 = 26,
        Mode32Apsk89 = 27,
        Mode32Apsk910 = 28,
        ModeReserved1 = 29,
        ModeReserved2 = 30,
        ModeReserved3 = 31
    }

    /// <summary>
    /// Base-Band header describing format of following data field.
    /// </summary>
    /// <remarks> 
    /// ETSI EN 302 307 v1.1.2: "Digital Video Broadcasting(DVB); Second generation framing structure, channel coding
    /// and  modulation systems  for  Broadcasting, Interactive Services, News  Gathering and  other broadband
    /// satellite applications" (2006-06)
    /// http://www.etsi.org/deliver/etsi_en/302300_302399/302307/01.01.02_60/en_302307v010102p.pdf
    /// </remarks>
    class BaseBandHeader
    {
        /// <summary>Length of <see cref="BaseBandHeader"/> measured in <see cref="byte"/>s.</summary>
        public const byte HeaderLength = 10;

        /// <summary>CRC-8: error detection code applied to the first 9 bytes of the Base-Band Header.</summary>
        public readonly byte Crc8;

        /// <summary>DFL: Data Field Length in bits, in the range[0, 58112]. </summary>3
        /// <example>"0x000A = Data Field length of 10 bits."
        /// <a href="http://www.etsi.org/deliver/etsi_en/302300_302399/302307/01.01.02_60/en_302307v010102p.pdf#page=18">
        /// ETSI EN 302 307 V1.1.2(2006-06)</a>
        /// </example>
        public readonly ushort DataFieldLength;

        public readonly Matype1 Matype1;

        /// <summary>If SIS/MIS = Multiple Input Stream, then Input Stream Identifier (ISI); else byte reserved (<b>null</b>).</summary>
        public readonly byte Matype2;

        /// <summary>SYNC: copy of the User Packet Sync-byte. Not relevant for Generic continuous input streams.</summary>
        public readonly byte Sync;

        /// <summary>SYNCD: distance in bits from the beginning of the DATA FIELD and the first UP from this frame
        /// (first bit of the CRC-8). SYNCD = 65535 means that no UP starts in the DATA FIELD.</summary>
        public readonly ushort SyncD;

        /// <summary>UPL: User Packet Length in bits, in the range [0,65535]</summary>
        public readonly ushort UserPacketLenght;

        public BaseBandHeader(
            Matype1 matype1,
            byte matype2,
            ushort userPacketLenght,
            ushort dataFieldLength,
            byte sync,
            ushort syncD,
            byte crc8)
        {
            this.Matype1 = matype1 ?? throw new ArgumentNullException(nameof(matype1));
            this.Matype2 = matype2;
            this.UserPacketLenght = userPacketLenght;
            this.DataFieldLength = dataFieldLength;
            this.Sync = sync;
            this.SyncD = syncD;
            this.Crc8 = crc8;
        }

        /// If SIS/MIS = Multiple Input Stream, then Input Stream Identifier (ISI); else byte reserved (<b>null</b>).
        public byte? InputStreamIdentifier => this.Matype1.SisMis == SisMis.Multiple ? (byte?) this.Matype2 : null;

        /// <summary>
        /// Factory method parsing <see cref="BaseBandHeader"/>.
        /// </summary> 
        /// <param name="reader"><see cref="PDUStreamReader"/> for stream from which the <see cref="BaseBandHeader"/>
        /// should be parsed.</param>
        /// <exception cref="InvalidPacketFormatException">If DFL (Data Field Length) field is not in range [0,58112].</exception>
        /// <returns>Parsed <see cref="BaseBandHeader"/> object.</returns>
        public static BaseBandHeader Parse(PDUStreamReader reader)
        {
            // Byte 0, MATYPE-1
            var matype1 = Matype1.Parse(reader.ReadByte());

            // Byte 1, MATYPE-2
            var matype2 = reader.ReadByte();

            // Byte 2-3, UPL
            var userPacketLenght = reader.ReadUInt16();

            // Byte 4-5, DFL
            var dataFieldLength = reader.ReadUInt16();

            // http://www.etsi.org/deliver/etsi_en/302300_302399/302307/01.01.02_60/en_302307v010102p.pdf#page=18
            if (dataFieldLength > 58112)
            {
                throw new InvalidPacketFormatException(
                    $"DFL (Data Field Length) field has be in range [0,58112]. {dataFieldLength} > 58112");
            }

            // Byte 6, SYNC
            var sync = reader.ReadByte();

            // Byte 7-8, SYNCD
            var syncD = reader.ReadUInt16();

            // Byte 9, CRC-8
            var crc8 = reader.ReadByte();

            // NOTE: DVB-S2 base band frame, base band header, CRC-8 is not validated.

            return new BaseBandHeader(matype1, matype2, userPacketLenght, dataFieldLength, sync, syncD, crc8);
        }
    }

    /// <summary>
    /// MATYPE: describes the input stream(s) format, the type of Mode Adaptation and the transmission Roll-off factor.
    /// This field is part of <see cref="BaseBandHeader"/>.</summary>
    class Matype1
    {
        /// <seealso cref="DVB.CcmAcm"/>
        public readonly CcmAcm CcmAcm;

        /// <summary>Input Stream Synchronization Indicator: If ISSYI = 1 = active, the ISSY field is inserted after UPs.</summary>
        public readonly bool Issyi;

        /// <summary>Null-packet deletion active.</summary>
        public readonly bool Npd;

        /// <summary>Transmission Roll-off factor.</summary>
        public readonly double? Ro;

        /// <seealso cref="DVB.SisMis"/>
        public readonly SisMis SisMis;

        /// <seealso cref="DVB.TsGs"/>
        public readonly TsGs TsGs;

        public Matype1(TsGs tsGs, SisMis sisMis, CcmAcm ccmAcm, bool issyi, bool npd, double? ro)
        {
            this.TsGs = tsGs;
            this.SisMis = sisMis;
            this.CcmAcm = ccmAcm;
            this.Issyi = issyi;
            this.Npd = npd;
            this.Ro = ro;
        }

        /// <summary>
        /// Factory method parsing <see cref="Matype1"/> from provided <see cref="byte"/> <paramref name="b"/>.
        /// </summary> 
        /// <param name="b"><see cref="byte"/> from which the <see cref="Matype1"/> should be parsed.</param> 
        /// <returns>Parsed <see cref="Matype1"/> object.</returns>
        public static Matype1 Parse(byte b)
        {
            // Bit 0-1, RO
            double? ro;
            switch (b & 0b0000_0011)
            {
                case 0b00:
                    ro = 0.35;
                    break;
                case 0b01:
                    ro = 0.25;
                    break;
                case 0b10:
                    ro = 0.20;
                    break;
                default: // 11 = reserved
                    ro = null;
                    break;
            }

            // Bit 2, NPD
            var npd = (b & 0b0000_0100) != 0;
            // Bit 3, ISSYI
            var issyi = (b & 0b0000_1000) != 0;
            // Bit 4, CCM/ACM
            var ccmAcm = (b & 0b0001_0000) == 0 ? CcmAcm.Acm : CcmAcm.Ccm;
            // Bit 5, SIS/MIS
            var sisMis = (b & 0b0010_0000) == 0 ? SisMis.Multiple : SisMis.Single;
            // Bit 6-7, TS/GS
            // TsGs enum has 4 values, therefore we use Enum.Parse instead of Enum.TryParse.
            var tsGs = (TsGs) Enum.Parse(typeof(TsGs), (b >> 6).ToString());

            return new Matype1(tsGs, sisMis, ccmAcm, issyi, npd, ro);
        }
    }

    /// <summary>CCM/ACM (Constant Coding and Modulation or Adaptive Coding and Modulation) field.
    /// VCM is signalled as ACM. This field is part of <see cref="Matype1"/> byte from <see cref="BaseBandHeader"/>.</summary>
    internal enum CcmAcm
    {
        Acm = 0,
        Ccm = 1
    }

    /// <summary>SIS/MIS (Single Input Stream or Multiple Input Stream) field.
    /// This field is part of <see cref="Matype1"/> byte from <see cref="BaseBandHeader"/>.</summary>
    internal enum SisMis
    {
        Multiple = 0,
        Single = 1
    }

    /// <summary>TS/GS (Transport Stream Input or Generic Stream Input (packetized or continuous)).
    /// This field is part of <see cref="Matype1"/> byte from <see cref="BaseBandHeader"/>.</summary>
    internal enum TsGs
    {
        GenericPacketized = 0b00,
        GenericContinuous = 0b01,
        Reserved = 0b10,
        Transport = 0b11
    }
}