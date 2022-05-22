using Netfox.Detective.ViewModels.ApplicationSettingsVms;
using Netfox.Snoopers.SnooperMAFF.Interfaces;

namespace Netfox.Snoopers.SnooperMAFF.WPF
{
    public class WrapperConstantsWPF : IWrapperConstants
    {
        public bool GenerateSnapshots { get; set; } = MAFFSnooperSettingsVm.StaticGenerateSnapshots;
        public long SnapshotsTimeSeparator { get; set; } = MAFFSnooperSettingsVm.StaticSnapshotsTimeSeparator;
        public bool ObjectRewrite { get; set; } = MAFFSnooperSettingsVm.StaticObjectRewrite;
        public bool StaticTurnOffConfigurationFile { get; set; } = MAFFSnooperSettingsVm.StaticTurnOffConfigurationFile;
    }
}
