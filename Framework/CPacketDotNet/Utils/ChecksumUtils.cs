using System;
using System.IO;
using System.Net;

namespace PacketDotNet.Utils
{
    /// <summary>
    ///     Computes the one's sum on a byte array.
    ///     Based TCP/IP Illustrated Vol. 2(1995) by Gary R. Wright and W. Richard
    ///     Stevens. Page 236. And on http://www.cs.utk.edu/~cs594np/unp/checksum.html
    /// </summary>
    /*
    * taken from TCP/IP Illustrated Vol. 2(1995) by Gary R. Wright and W.
    * Richard Stevens. Page 236
    */
    public sealed class ChecksumUtils
    {
        private ChecksumUtils() { }

        /// <summary>
        ///     Computes the one's complement sum on a byte array
        /// </summary>
        public static int OnesComplementSum(byte[] bytes)
        {
            //just complement the one's sum
            return OnesComplementSum(bytes, 0, bytes.Length);
        }

        /// <summary>
        ///     Computes the one's complement sum on a byte array
        /// </summary>
        public static int OnesComplementSum(byte[] bytes, int start, int len)
        {
            //just complement the one's sum
            return (~OnesSum(bytes, start, len))&0xFFFF;
        }

        /// <summary>
        ///     Compute a ones sum of a byte array
        /// </summary>
        /// <param name="bytes">
        ///     A <see cref="System.Byte" />
        /// </param>
        /// <returns>
        ///     A <see cref="System.Int32" />
        /// </returns>
        public static int OnesSum(byte[] bytes) { return OnesSum(bytes, 0, bytes.Length); }

        /// <summary>
        ///     16 bit sum of all values
        ///     http://en.wikipedia.org/wiki/Signed_number_representations#Ones.27_complement
        /// </summary>
        /// <param name="bytes">
        ///     A <see cref="System.Byte" />
        /// </param>
        /// <param name="start">
        ///     A <see cref="System.Int32" />
        /// </param>
        /// <param name="len">
        ///     A <see cref="System.Int32" />
        /// </param>
        /// <returns>
        ///     A <see cref="System.Int32" />
        /// </returns>
        public static int OnesSum(byte[] bytes, int start, int len)
        {
            var memStream = new MemoryStream(bytes, start, len);
            var br = new BinaryReader(memStream);
            var sum = 0;

            UInt16 val;

            while(memStream.Position < memStream.Length - 1)
            {
                val = (UInt16) IPAddress.NetworkToHostOrder(br.ReadInt16());
                sum += val;
            }

            // if we have a remaining byte we should add it
            if(memStream.Position < len) { sum += br.ReadByte(); }

            // fold the sum into 16 bits
            while((sum >> 16) != 0) { sum = (sum&0xffff) + (sum >> 16); }

            return sum;
        }
    }
}