namespace AlphaChiTech.Virtualization.Interfaces
{
    public interface IBaseSourceProvider<T> : ISynchronized
    {
        void OnReset(int count);
        
    }
}
