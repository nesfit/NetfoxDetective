using System.IO;

namespace Netfox.Framework.Models.PmLib.Captures
{
    public class PmCapturePcapNg : PmCaptureBase
    {
        protected PmCapturePcapNg()
        {
        }

        public PmCapturePcapNg(FileInfo fileInfo) : base(fileInfo)
        {
        }
    }
}