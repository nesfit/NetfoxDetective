namespace Netfox.Snoopers.SnooperMAFF.Interfaces
{
    public interface IWrapperConstants
    {
        bool GenerateSnapshots { get; set; }
        long SnapshotsTimeSeparator { get; set; }
        bool ObjectRewrite { get; set; }
        bool StaticTurnOffConfigurationFile { get; set; }
    }
}
