using System.Collections;
using AlphaChiTech.Virtualization.Pageing;

namespace AlphaChiTech.Virtualization.Interfaces
{
    public interface IPagedSourceProvider<T> : IBaseSourceProvider<T>
    {
        PagedSourceItemsPacket<T> GetItemsAt(int pageoffset, int count, bool usePlaceholder);

        int Count { get; }

        int IndexOf(T item);
        bool Contains(T item);
    }
}
