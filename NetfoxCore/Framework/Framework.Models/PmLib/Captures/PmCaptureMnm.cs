using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Netfox.Framework.Models.PmLib.Captures
{
    public class PmCaptureMnm : PmCaptureBase
    {
        protected PmCaptureMnm()
        {
        }

        public PmCaptureMnm(FileInfo fileInfo) : base(fileInfo)
        {
        }

        //todo persistent
        public Dictionary<Tuple<IPEndPoint, IPEndPoint>, String> AppTagDictionary { get; } =
            new Dictionary<Tuple<IPEndPoint, IPEndPoint>, String>();

        public String GetApplicationTag(IPEndPoint ipSource, IPEndPoint ipDest)
        {
            if (this.AppTagDictionary.Count == 0)
            {
                return null;
            }

            var key = new Tuple<IPEndPoint, IPEndPoint>(ipSource, ipDest);
            if (this.AppTagDictionary.ContainsKey(key)) return this.AppTagDictionary[key];

            key = new Tuple<IPEndPoint, IPEndPoint>(ipDest, ipSource);
            if (this.AppTagDictionary.ContainsKey(key)) return this.AppTagDictionary[key];

            return null;
        }
    }
}