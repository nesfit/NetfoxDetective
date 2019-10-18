using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.SnooperMAFF.Interfaces
{
    public interface IWrapperConstants
    {
        bool GenerateSnapshots { get; set; }
        long SnapshotsTimeSeparator { get; set; }
        bool ObjectRewrite { get; set; }
        bool StaticTurnOffConfigurationFile { get; set; }
    }
}
