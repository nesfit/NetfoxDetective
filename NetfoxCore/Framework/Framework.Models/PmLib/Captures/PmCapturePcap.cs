using System.IO;

namespace Netfox.Framework.Models.PmLib.Captures
{
    public class PmCapturePcap : PmCaptureBase
    {
        protected PmCapturePcap()
        {
        }

        public PmCapturePcap(FileInfo fileInfo) : base(fileInfo)
        {
        }
    }
}