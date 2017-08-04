namespace AlphaChiTech.Virtualization.Interfaces
{
    public interface ISynchronized
    {
        object SyncRoot { get; }
        bool IsSynchronized { get; }
    }
}