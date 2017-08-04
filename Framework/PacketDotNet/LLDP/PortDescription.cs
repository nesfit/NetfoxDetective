using System.Reflection;
using log4net;

namespace PacketDotNet.LLDP
{
    /// <summary>
    ///     A Port Description TLV
    /// </summary>
    public class PortDescription : StringTLV
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

        #region Constructors
        /// <summary>
        ///     Creates a Port Description TLV
        /// </summary>
        /// <param name="bytes">
        /// </param>
        /// <param name="offset">
        ///     The Port Description TLV's offset from the
        ///     origin of the LLDP
        /// </param>
        public PortDescription(byte[] bytes, int offset) : base(bytes, offset) { log.Debug(""); }

        /// <summary>
        ///     Creates a Port Description TLV and sets it value
        /// </summary>
        /// <param name="description">
        ///     A textual description of the port
        /// </param>
        public PortDescription(string description) : base(TLVTypes.PortDescription, description) { log.Debug(""); }
        #endregion

        #region Properties
        /// <value>
        ///     A textual Description of the port
        /// </value>
        public string Description
        {
            get { return this.StringValue; }
            set { this.StringValue = value; }
        }
        #endregion
    }
}