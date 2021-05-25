namespace Netfox.Framework.Models.Enums
{
    public enum EfcPDUProviderType
    {
        /// <summary>
        ///     PDUProviderMixed provides PDUs ordered by TIMESTAMP of first packet arrival to the client.
        ///     When Application message is spread to more PDUs, then next PDU is selected by TIMESTAMP so it
        ///     can be from same or other flow direction
        /// </summary>
        Mixed,

        /// <summary>
        ///     PDUProviderBreakedInterlay provides PDUs ordered by TIMESTAMP of first packet arrival to the
        ///     client. When Application message is spread to more PDUs, then next PDU is selected from the
        ///     SAME FLOW as the first PDU, but if next PDU in row is not with same flowDirection, update
        ///     fails. Same interpetation as Wireshark`s follow TCP stream.
        /// </summary>
        Breaked,

        /// <summary>
        ///     PDUProviderContinuingInterlay provides PDUs ordered by TIMESTAMP of first packet arrival to
        ///     the client. When Application message is spread to more PDUs, then next PDU is selected from
        ///     the SAME FLOW as the first PDU
        ///     Same interpetation as Wireshark`s follow TCP stream.
        /// </summary>
        ContinueInterlay,

        /// <summary>
        ///     PDUProviderSingleMessage provides ony one PDU ordered by TIMESTAMP of first packet arrival to
        ///     the client.
        /// </summary>
        SingleMessage
    }
}