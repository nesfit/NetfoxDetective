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
using System.Runtime.Serialization;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models.PmLib.Frames
{
    [Serializable]
    [DataContract]
    public class PmFramePcap : PmFrameBase
    {
        /// <summary>
        ///     Constructor used when indexing
        /// </summary>
        public PmFramePcap(
            PmCaptureBase pmCapture,
            Int64 fraIndex,
            Int64 fraOffset,
            PmLinkType pmLinkType,
            DateTime timeStamp,
            Int64 oriLength,
            Int64 incLength) : base(pmCapture, pmLinkType, timeStamp, incLength, PmFrameType.Pcap, fraIndex, oriLength)
        {
            this.FrameOffset = fraOffset;
            this.L2Offset = this.FrameOffset + 16;
        }

        public PmFramePcap() : base() {  }
    }
}