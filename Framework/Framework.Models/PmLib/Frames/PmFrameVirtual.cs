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
using System.Linq;
using System.Threading.Tasks;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models.PmLib.Frames
{
    public class PmFrameVirtual : PmFrameBase
    {
        public PmFrameVirtual(
            PmCaptureBase pmCapture,
            Int64 fraIndex,
            DateTime timeStamp,
            Int64 incLength,
            Int64 l2Offset) : base(pmCapture, PmLinkType.Raw, timeStamp, incLength, PmFrameType.Virtual,fraIndex, incLength)
        {
            // NOTE: value of incLength passed to base constructor also as originalLength
            this.L2Offset = l2Offset;
            this.FrameOffset = l2Offset;
        }

        private PmFrameVirtual() : base() { }

        public PmFrameVirtual(PmFrameBase copyFrame) : base()
        {
            var t = typeof(PmFrameBase);
            var properties = t.GetProperties().Where(p=>p.CanWrite&&p.CanRead&&p.Name!=nameof(PmFrameBase.Id));
            foreach (var pi in properties)
            {
                pi.SetValue(this, pi.GetValue(copyFrame, null), null);
            }
        }



#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task IndexFrame(IPmCaptureProcessorBlockBase captureProcessorBlockBase) { }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}