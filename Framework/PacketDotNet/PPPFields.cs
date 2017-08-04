namespace PacketDotNet
{
    /// <summary>
    ///     The fields in a PPP packet
    ///     See http://en.wikipedia.org/wiki/Point-to-Point_Protocol
    /// </summary>
    public class PPPFields
    {
        /// <summary>
        ///     Offset from the start of the PPP packet where the Protocol field is located
        /// </summary>
        public static readonly int ProtocolPosition = 0;

        /// <summary>
        ///     Length of the Protocol field in bytes, the field is of type
        ///     PPPProtocol
        /// </summary>
        public static readonly int ProtocolLength = 2;

        /// <summary>
        ///     The length of the header
        /// </summary>
        public static readonly int HeaderLength = ProtocolLength;
    }
}