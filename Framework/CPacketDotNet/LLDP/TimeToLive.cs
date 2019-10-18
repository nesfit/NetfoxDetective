using System.Reflection;
using log4net;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.LLDP
{
    /// <summary>
    ///     A Time to Live TLV
    /// </summary>
    public class TimeToLive : TLV
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
        ///     Number of bytes in the value portion of this tlv
        /// </summary>
        private const int ValueLength = 2;

        #region Constructors
        /// <summary>
        ///     Creates a TTL TLV
        /// </summary>
        /// <param name="bytes">
        /// </param>
        /// <param name="offset">
        ///     The TTL TLV's offset from the
        ///     origin of the LLDP
        /// </param>
        public TimeToLive(byte[] bytes, int offset) : base(bytes, offset) { log.Debug(""); }

        /// <summary>
        ///     Creates a TTL TLV and sets it value
        /// </summary>
        /// <param name="seconds">
        ///     The length in seconds until the LLDP
        ///     is refreshed
        /// </param>
        public TimeToLive(ushort seconds)
        {
            log.Debug("");

            var bytes = new byte[TLVTypeLength.TypeLengthLength + ValueLength];
            var offset = 0;
            var length = bytes.Length;
            this.tlvData = new ByteArraySegment(bytes, offset, length);

            this.Type = TLVTypes.TimeToLive;
            this.Seconds = seconds;
        }
        #endregion

        #region Properties
        /// <value>
        ///     The number of seconds until the LLDP needs
        ///     to be refreshed
        ///     A value of 0 means that the LLDP source is
        ///     closed and should no longer be refreshed
        /// </value>
        public ushort Seconds
        {
            get
            {
                // get the seconds
                return EndianBitConverter.Big.ToUInt16(this.tlvData.Bytes, this.tlvData.Offset + TLVTypeLength.TypeLengthLength);
            }
            set { EndianBitConverter.Big.CopyBytes(value, this.tlvData.Bytes, this.tlvData.Offset + TLVTypeLength.TypeLengthLength); }
        }

        /// <summary>
        ///     Convert this TTL TLV to a string.
        /// </summary>
        /// <returns>
        ///     A human readable string
        /// </returns>
        public override string ToString() { return string.Format("[TimeToLive: Seconds={0}]", this.Seconds); }
        #endregion
    }
}