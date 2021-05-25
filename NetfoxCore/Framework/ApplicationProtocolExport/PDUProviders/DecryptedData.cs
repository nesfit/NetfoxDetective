using System;
using Netfox.Framework.Models;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    public class DecryptedData
    {
        public int DataOffset;
        public Byte[] Data { get; set; }

        public int DataLength
        {
            get { return this.Data.Length; }
        }

        public Boolean IsAllRead
        {
            get { return this.DataLength == this.DataOffset; }
        }

        public int RemainingBytes
        {
            get { return this.DataLength - this.DataOffset; }
        }

        public L7PDU PDU { get; set; }
    }
}