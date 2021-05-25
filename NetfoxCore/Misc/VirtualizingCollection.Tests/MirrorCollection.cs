using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AlphaChiTech.VirtualizingCollection.Interfaces;
using NUnit.Framework;

namespace VirtualizingCollection.Tests
{
    public class MirrorCollection<T> : IDisposable, IEnumerable<T>
    {
        public IList<T> Items => _list;
        private readonly IObservableCollection<T> _collection;
        private readonly List<T> _list;
        
        public MirrorCollection(IObservableCollection<T> collection)
        {
            _collection = collection;
            _collection.CollectionChanged += OnChange;
            _list = new List<T>(_collection);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Dispose()
        {
            _collection.CollectionChanged -= OnChange;
        }

        private void OnChange(object? s, NotifyCollectionChangedEventArgs e)
        {
            Assert.AreEqual(_collection, s);
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _list.Clear();
                _list.AddRange(_collection);
                return;
            }
            
            var rem = e.OldItems?.Count ?? 0;
            if (rem > 0)
                _list.RemoveRange(e.OldStartingIndex, rem);
            
            var add = e.NewItems?.Count ?? 0;
            if (add > 0)
                _list.InsertRange(e.NewStartingIndex, e.NewItems.OfType<T>());
        }
    }
}