using System;
using System.Collections.Generic;
using Netfox.Core.Database;
using Netfox.Core.Database.Wrappers;

namespace Netfox.Framework.Models.PmLib.Captures
{
    /// <summary>
    ///     Represents interface block in PCAPng file
    /// </summary>
    [Persistent]
    public class PcapNgInterface : IEntity
    {
        public virtual List<IPAddressEF> Addresses { get; private set; } = new List<IPAddressEF>();
        public Byte[] EuiAddr { get; set; }
        public UInt32 Fcslen { get; set; }
        public Byte[] Filter { get; set; }
        public Boolean HasTsresol { get; set; }
        public UInt16 InterfaceId { get; set; }
        public String IfDescription { get; set; }
        public String IfName { get; set; }
        public UInt16 LinkType { get; set; }
        public Byte[] MacAddress { get; set; }
        public String Os { get; set; }
        public UInt32 SnapLen { get; set; } = 65535;
        public UInt64 Speed { get; set; }
        public UInt64 Tsoffset { get; set; }
        public Byte Tsresol { get; set; }
        public UInt64 Tzone { get; set; }

        private PcapNgInterface()
        {
        }

        public PcapNgInterface(UInt16 linkType, UInt16 interfaceId)
        {
            this.Tsresol = 0;
            this.LinkType = linkType;
            this.InterfaceId = interfaceId;
        }

        public PcapNgInterface(UInt32 snapLen, UInt16 linkType, UInt16 id) : this(linkType, id)
        {
            this.SnapLen = snapLen;
        }


        public enum InterfaceOptions
        {
            OptEndofopt = 0,
            IfName = 2,
            IfDescription = 3,
            IfIPv4Addr = 4,
            IfIPv6Addr = 5,
            IfMaCaddr = 6,
            IfEuIaddr = 7,
            IfSpeed = 8,
            IfTsresol = 9,
            IfTzone = 10,
            IfFilter = 11,
            IfOs = 12,
            IfFcslen = 13,
            IfTsoffset = 14
        }

        #region Implementation of IEntity

        public Guid Id { get; private set; } = Guid.NewGuid();

        #region Implementation of IEntity

        public DateTime FirstSeen { get; set; }

        #endregion

        #endregion
    }
}