using System.Reflection;
using log4net;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    ///     Transport layer packet
    /// </summary>
    public abstract class TransportPacket : Packet
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

        /// <value>
        ///     The Checksum version
        /// </value>
        public abstract ushort Checksum { get; set; }

        /// <summary>
        ///     Calculates the transport layer checksum, either for the
        ///     tcp or udp packet
        /// </summary>
        /// <param name="option">
        ///     <see cref="TransportPacket.TransportChecksumOption" />
        /// </param>
        /// <returns>
        ///     A <see cref="System.Int32" />
        /// </returns>
        internal int CalculateChecksum(TransportChecksumOption option)
        {
            // save the checksum field value so it can be restored, altering the checksum is not
            // an intended side effect of this method
            var originalChecksum = this.Checksum;

            // reset the checksum field (checksum is calculated when this field is
            // zeroed)
            this.Checksum = 0;

            // copy the tcp section with data
            var dataToChecksum = ((IpPacket) this.ParentPacket).PayloadPacket.Bytes;

            if(option == TransportChecksumOption.AttachPseudoIPHeader) { dataToChecksum = ((IpPacket) this.ParentPacket).AttachPseudoIPHeader(dataToChecksum); }

            // calculate the one's complement sum of the tcp header
            var cs = ChecksumUtils.OnesComplementSum(dataToChecksum);

            // restore the checksum field value
            this.Checksum = originalChecksum;

            return cs;
        }

        /// <summary>
        ///     Determine if the transport layer checksum is valid
        /// </summary>
        /// <param name="option">
        ///     A <see cref="TransportChecksumOption" />
        /// </param>
        /// <returns>
        ///     A <see cref="System.Boolean" />
        /// </returns>
        public virtual bool IsValidChecksum(TransportChecksumOption option)
        {
            var upperLayer = ((IpPacket) this.ParentPacket).PayloadPacket.Bytes;

            log.DebugFormat("option: {0}, upperLayer.Length {1}", option, upperLayer.Length);

            if(option == TransportChecksumOption.AttachPseudoIPHeader) { upperLayer = ((IpPacket) this.ParentPacket).AttachPseudoIPHeader(upperLayer); }

            var onesSum = ChecksumUtils.OnesSum(upperLayer);
            const int expectedOnesSum = 0xffff;
            log.DebugFormat("onesSum {0} expected {1}", onesSum, expectedOnesSum);

            return (onesSum == expectedOnesSum);
        }

        /// <summary>
        ///     Options for use when creating a transport layer checksum
        /// </summary>
        public enum TransportChecksumOption
        {
            /// <summary>
            ///     No extra options
            /// </summary>
            None,

            /// <summary>
            ///     Attach a pseudo IP header to the transport data being checksummed
            /// </summary>
            AttachPseudoIPHeader
        }
    }
}