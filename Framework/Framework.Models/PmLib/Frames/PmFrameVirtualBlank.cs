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
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models.PmLib.Frames
{
    public sealed class PmFrameVirtualBlank : PmFrameBase
    {
        /// <summary>
        ///     Constructor used when creating new one or indexing existing
        /// </summary>
        public PmFrameVirtualBlank(PmFrameBase template, Int64 fraLength, DateTime dateTime)
            : base(template.PmCapture, PmLinkType.Null, dateTime, fraLength, PmFrameType.VirtualBlank, template.FrameIndex, fraLength)
        {
            this.L7PayloadLength = fraLength;
            this.FrameOffset = 0;
            this.L2Offset = 0;
            this.L3Offset = 0;
            this.L4Offset = 0;
            this.L7Offset = 0;
            this.DstAddress = template.DstAddress;
            this.SrcAddress = template.SrcAddress;
        }

        private PmFrameVirtualBlank() : base() { }
        
        /// <summary>
        ///     Retrieves fake zeroed L7 data that could be use as binary stuffing
        /// </summary>
        /// <returns>Byte array containing only 0</returns>
        public override Byte[] L7Data() => new Byte[this.IncludedLength];

        #region Functions not applicable for VirtualFrame
        /// <summary>
        ///     Function returns ALWAYS null for VirtualFrame
        /// </summary>
        public override Byte[] L2Data() => null;

        /// <summary>
        ///     Function returns ALWAYS null for VirtualFrame
        /// </summary>
        public override Byte[] L3Data() => null;

        /// <summary>
        ///     Function returns ALWAYS null for VirtualFrame
        /// </summary>
        public override Byte[] L4Data() => null;
        #endregion
    }
}