using System;
using System.Collections.Generic;

namespace AlphaChiTech.VirtualizingCollection.Pageing
{
    public class PagedSourceItemsPacket<T>
    {
        public IEnumerable<T> Items { get; set; }

        public object LoadedAt { get; set; } = DateTime.Now;
    }
}