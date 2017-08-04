using System.Collections.Specialized;

namespace AlphaChiTech.Virtualization.Interfaces
{
    public interface IPagedSourceObservableProvider<T>:IPagedSourceProvider<T>, INotifyCollectionChanged, IEditableProvider<T>
    {
    }
}
