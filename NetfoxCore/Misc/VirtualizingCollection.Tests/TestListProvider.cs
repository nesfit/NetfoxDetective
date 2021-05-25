using System.Collections.Generic;
using AlphaChiTech.VirtualizingCollection.Interfaces;

namespace VirtualizingCollection.Tests
{
    public class TestListProvider<T> : IItemSourceProvider<T>, IEditableProviderItemBased<T>, IEditableProviderIndexBased<T>
    {
        public bool IsSynchronized => true;
        public object SyncRoot => _list;
        private readonly IList<T> _list;

        public TestListProvider() : this(new List<T>())
        {
            
        }
        
        public TestListProvider(IList<T> list)
        {
            _list = list;
        }

        public void OnReset(int count)
        {
            lock (_list)
            {
                _list.Clear();
            }
        }
        
        public bool Contains(T item)
        {
            lock (_list)
            {
                return _list.Contains(item);
            }
        }

        public T GetAt(int index, object voc, bool usePlaceholder)
        {
            lock (_list)
            {
                return _list[index];
            }
        }

        public int GetCount(bool asyncOk)
        {
            lock (_list)
            {
                return _list.Count;
            }
        }

        public int IndexOf(T item)
        {
            lock (_list)
            {
                return _list.IndexOf(item);
            }
        }

        public int OnAppend(T item, object timestamp)
        {
            lock (_list)
            {
                _list.Add(item);
                return _list.Count - 1;
            }
        }

        public void OnInsert(int index, T item, object timestamp)
        {
            lock (_list)
            {
                _list.Insert(index, item);
            }
        }

        public void OnReplace(int index, T oldItem, T newItem, object timestamp)
        {
            lock (_list)
            {
                _list[index] = newItem;
            }
        }

        public int OnRemove(T item, object timestamp)
        {
            lock (_list)
            {
                int idx = _list.IndexOf(item);
                if (idx < 0)
                    return idx;

                _list.RemoveAt(idx);
                return idx;
            }
        }

        public int OnReplace(T oldItem, T newItem, object timestamp)
        {
            lock (_list)
            {
                int idx = _list.IndexOf(oldItem);
                if (idx < 0)
                    return idx;

                _list[idx] = newItem;
                return idx;
            }
        }

        public T OnRemove(int index, object timestamp)
        {
            lock (_list)
            {
                var val = _list[index];
                _list.RemoveAt(index);
                return val;
            }
        }

        public T OnReplace(int index, T newItem, object timestamp)
        {
            lock (_list)
            {
                var val = _list[index];
                _list[index] = newItem;
                return val;
            }
        }
    }
}