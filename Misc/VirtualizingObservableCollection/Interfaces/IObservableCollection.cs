using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AlphaChiTech.Virtualization.Interfaces
{
    public interface IObservableCollection<T> : INotifyCollectionChanged, ICollection<T>, IObservable<T>, IObservableCollection
    {
        new int Count { get; }
        new bool Remove(object item);
    }

    public interface IObservableCollection : ICollection, INotifyCollectionChanged
    {
        void Add(object item);
        bool Remove(object item);
        void Clear();
    }
}
