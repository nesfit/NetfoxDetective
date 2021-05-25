using System;
using System.Net;

namespace Netfox.Framework.Models
{
    /// <summary> A bt IP end point extent.</summary>
    internal static class IpEndPointExt
    {
        /// <summary> Gets smaller of the two passed IPEndPoint objects.</summary>
        /// <param name="x"> The x to act on. </param>
        /// <param name="y"> IP end point to compare to this. </param>
        /// <returns>
        ///     Less than zero - This instance precedes "that" object. Zero -  This instance occurs in the
        ///     same position in the sort order as the "that" object. Greater than zero - This instance
        ///     follows "that" object in the sort order.
        /// </returns>
        internal static Int32 CompareTo(this IPEndPoint x, IPEndPoint y)
        {
            // check all bytes one by one:
            var xb = x.Address.GetAddressBytes();
            var yb = y.Address.GetAddressBytes();

            var min = Math.Min(xb.Length, yb.Length);
            for (var i = 0; i < min; i++)
            {
                var cmp = xb[i].CompareTo(yb[i]);
                if (cmp != 0)
                {
                    return cmp;
                }
            }

            var lenCmp = xb.Length.CompareTo(yb.Length);
            return (lenCmp != 0) ? lenCmp : x.Port.CompareTo(y.Port);
        }
    }
}