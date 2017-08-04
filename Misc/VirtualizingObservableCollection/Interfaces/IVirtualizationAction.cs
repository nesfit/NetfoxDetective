using AlphaChiTech.Virtualization.Actions;

namespace AlphaChiTech.Virtualization.Interfaces
{
    public interface IVirtualizationAction
    {
        VirtualActionThreadModelEnum ThreadModel { get; }

        void DoAction();
    }
}
