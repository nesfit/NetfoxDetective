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
using System.IO;
using System.Net;

namespace Netfox.Framework.Models.PmLib.Captures
{
   public class PmCaptureMnm:PmCaptureBase
    {
        protected PmCaptureMnm()
        {
        }

        public PmCaptureMnm(FileInfo fileInfo) : base(fileInfo)
        {
        }
        
        //todo persistent
        public Dictionary<Tuple<IPEndPoint, IPEndPoint>, String> AppTagDictionary { get; } = new Dictionary<Tuple<IPEndPoint, IPEndPoint>, String>();
        
        public String GetApplicationTag(IPEndPoint ipSource, IPEndPoint ipDest)
        {
            if (this.AppTagDictionary.Count == 0) { return null; }

            var key = new Tuple<IPEndPoint, IPEndPoint>(ipSource, ipDest);
            if (this.AppTagDictionary.ContainsKey(key)) return this.AppTagDictionary[key];

            key = new Tuple<IPEndPoint, IPEndPoint>(ipDest, ipSource);
            if (this.AppTagDictionary.ContainsKey(key)) return this.AppTagDictionary[key];

            return null;
        }

      
    }
}
