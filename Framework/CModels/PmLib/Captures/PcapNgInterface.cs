/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
 * Author(s):
 * Vladimir Vesely (mailto:ivesely@fit.vutbr.cz)
 * Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
 * Jan Plusal (mailto:xplusk03@stud.fit.vutbr.cz)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Net;

namespace Netfox.Framework.Models.PmLib.Captures
{
    /// <summary>
    ///     Represents interface block in PCAPng file
    /// </summary>
    public class PcapNgInterface
    {
        public virtual List<IPAddress> Addresses { get; private set; } = new List<IPAddress>();
        public Byte[] EuiAddr { get;  set; }
        public UInt32 Fcslen { get;  set; }
        public Byte[] Filter { get;  set; }
        public Boolean HasTsresol { get;  set; }
        public UInt16 InterfaceId { get;  set; }
        public String IfDescription { get;  set; }
        public String IfName { get;  set; }
        public UInt16 LinkType { get;  set; }
        public Byte[] MacAddress { get;  set; }
        public String Os { get;  set; }
        public UInt32 SnapLen { get;  set; } = 65535;
        public UInt64 Speed { get;  set; }
        public UInt64 Tsoffset { get;  set; }
        public Byte Tsresol { get;  set; }
        public UInt64 Tzone { get;  set; }
        private PcapNgInterface() { }

        public PcapNgInterface(UInt16 linkType, UInt16 interfaceId)
        {
            this.Tsresol = 0;
            this.LinkType = linkType;
            this.InterfaceId = interfaceId;
        }

        public PcapNgInterface(UInt32 snapLen, UInt16 linkType, UInt16 id) : this(linkType, id) { this.SnapLen = snapLen; }


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
        public Guid Id { get; private set; }= Guid.NewGuid();

        #region Implementation of IEntity
        public DateTime FirstSeen { get; set; }
        #endregion

        #endregion
    }
}