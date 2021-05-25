using System;
using System.Collections.Generic;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.Models.Interfaces
{
    public interface IAppTagProvider
    {
        String GetAppTagShort(IEnumerable<PmCaptureBase> captures, PmFrameBase frame);
        String GetAppTagShort(string appTag); 
    }
}