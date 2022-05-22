using System;
using System.Net;
using System.Runtime.Serialization;
using Netfox.Framework.Models.PmLib.Frames;
using PacketDotNet;

namespace Netfox.Framework.Models
{
    [DataContract]
    public class L4FlowKey
    {
        public L4FlowKey()
        {
        }

        /// <summary> The type of the protocol.</summary>
        /// <value> The type of the 4 protocol.</value>
        [DataMember]
        public IPProtocolType L4ProtocolType { get; set; }

        /// <summary> Gets or sets the end points.</summary>
        /// <value> The end points.</value>
        [DataMember]
        public IPEndPoint[] EndPoints { get; set; } = new IPEndPoint[2];

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents the current
        ///     <see
        ///         cref="IBidirectionalFlowKey" />
        ///     .
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents the current
        ///     <see
        ///         cref="IBidirectionalFlowKey" />
        ///     .
        /// </returns>
        public override String ToString() =>
            String.Format("{0}:{1}<->{2}", this.L4ProtocolType, this.EndPoints[0], this.EndPoints[1]);

        #region Hash and Equals

        /// <summary>
        ///     Serves as a hash function for a <see cref="IBidirectionalFlowKey" /> object.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance that is suitable for use in hashing algorithms and data
        ///     structures such as a hash table.
        /// </returns>
        public override Int32 GetHashCode() => (this.EndPoints[0] ?? PmFrameBase.NullEndPoint).GetHashCode() ^
                                               (this.EndPoints[1] ?? PmFrameBase.NullEndPoint).GetHashCode() ^
                                               this.L4ProtocolType.GetHashCode();

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to the current
        ///     <see
        ///         cref="IBidirectionalFlowKey" />
        ///     .
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="System.Object" /> to compare with the current
        ///     <see
        ///         cref="IBidirectionalFlowKey" />
        ///     .
        /// </param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to the current
        ///     <see cref="IBidirectionalFlowKey" />; otherwise, <c>false</c>.
        /// </returns>
        public override Boolean Equals(Object obj)
        {
            var l4FlowKey = obj as L4FlowKey;
            return l4FlowKey != null && this._Equals(l4FlowKey);
        }

        /// <summary> Tests if this IIBBidirectionalFlowKey is considered equal to another.</summary>
        /// <param name="other"> The i bt bidirectional flow key to compare to this object. </param>
        /// <returns> true if the objects are considered equal, false if they are not.</returns>
        public Boolean Equals(L4FlowKey other) => this._Equals(other);

        /// <summary> Equals the given other.</summary>
        /// <param name="other"> The other. </param>
        /// <returns> true if the objects are considered equal, false if they are not.</returns>
        private Boolean _Equals(L4FlowKey other)
        {
            var e1 = Equals(this.EndPoints[0], other.EndPoints[0]);
            var e2 = Equals(this.EndPoints[1], other.EndPoints[1]);
            var e3 = Equals(this.EndPoints[0], other.EndPoints[1]);
            var e4 = Equals(this.EndPoints[1], other.EndPoints[0]);
            var e5 = this.L4ProtocolType.Equals(other.L4ProtocolType);
            //var e6 = this.L2ProtocolType.Equals(other.L2ProtocolType);
            return e5 && ((e1 && e2) || (e3 && e4)); // && e6;
        }

        #endregion
    }
}