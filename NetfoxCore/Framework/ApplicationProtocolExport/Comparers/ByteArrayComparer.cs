using System;
using System.Collections.Generic;
using System.Linq;

namespace Netfox.Framework.ApplicationProtocolExport.Comparers
{
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(Byte[] a, Byte[] b)
        {
            return a.SequenceEqual(b);
        }

        public int GetHashCode(Byte[] key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return key.Sum(b => b);
        }
    }
}