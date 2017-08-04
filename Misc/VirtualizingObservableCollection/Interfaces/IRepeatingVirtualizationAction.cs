namespace AlphaChiTech.Virtualization.Interfaces
{
    public interface IRepeatingVirtualizationAction
    {
        bool KeepInActionsList();
        bool IsDueToRun();
    }
}
