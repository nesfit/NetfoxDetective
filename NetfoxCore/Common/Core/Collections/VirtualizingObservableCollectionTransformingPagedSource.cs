using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AlphaChiTech.VirtualizingCollection.Interfaces;
using AlphaChiTech.VirtualizingCollection.Pageing;

namespace Netfox.Core.Collections
{
    public class VirtualizingObservableCollectionTransformingPagedSource<TTransformedType, TSourceType> :
        IPagedSourceObservableProvider<TTransformedType>, ICollection,
        INotifyCollectionChanged
        where TTransformedType : class where TSourceType : class //IEditableProvider<TSourceType>
    {
        private bool _added;

        public VirtualizingObservableCollectionTransformingPagedSource(
            INotifyCollectionChanged sourceCollection,
            Func<TSourceType, TTransformedType> transformItem,
            Func<TTransformedType, TSourceType> transformItemBack)
        {
            sourceCollection.CollectionChanged += this.SourceCollectionOnCollectionChanged;
            this.SourceCollection = sourceCollection as ICollection;

            var collectionAddMethodInfo = sourceCollection.GetType().GetMethod("Add", new[]
            {
                typeof(TSourceType)
            }) ?? sourceCollection.GetType().GetMethod("Add", new[]
            {
                typeof(object)
            });
            var collectionRemoveMethodInfo = sourceCollection.GetType().GetMethod("Remove", new[]
            {
                typeof(TSourceType)
            }) ?? sourceCollection.GetType().GetMethod("Remove", new[]
            {
                typeof(object)
            });

            var collectionClearMethodInfo = sourceCollection.GetType().GetMethod("Clear", new Type[0]);


            if (this.SourceCollection == null || collectionAddMethodInfo == null ||
                collectionRemoveMethodInfo == null || collectionClearMethodInfo == null)
            {
                throw new ArgumentException(
                    "SourceCollection not implements ICollection<TSourceType> and/or ICollection",
                    nameof(sourceCollection));
            }

            this.AddToSourceCollection = item =>
            {
                collectionAddMethodInfo.Invoke(this.SourceCollection, new[]
                {
                    item
                });
            };
            this.RemoveFromSourceCollection = item => collectionRemoveMethodInfo.Invoke(this.SourceCollection, new[]
            {
                item
            });

            this.ClearSourceCollection = () => collectionClearMethodInfo.Invoke(this.SourceCollection, new object[0]);

            this.SourceCollectionList = sourceCollection as IList<TSourceType>;
            this.TransformItem = transformItem;
            this.TransformItemBack = transformItemBack;
            Task.Factory.StartNew(this.PeriodicalUpdate, TaskCreationOptions.LongRunning);
        }

        public Func<TSourceType, object> RemoveFromSourceCollection { get; }

        public Action<TSourceType> AddToSourceCollection { get; }
        public Action ClearSourceCollection { get; }
        private ICollection SourceCollection { get; }
        private Func<TSourceType, TTransformedType> TransformItem { get; }
        private Func<TTransformedType, TSourceType> TransformItemBack { get; }
        private IList<TSourceType> SourceCollectionList { get; }

        #region Implementation of IBaseSourceProvider

        /// <summary>
        ///     This is a callback that runs when a Reset is called on a provider. Implementing this is also optional.
        ///     If you don’t need to do anything in particular when resets occur, you can leave this method body empty.
        /// </summary>
        /// <param name="count"></param>
        public void OnReset(int count)
        {
            this.ClearSourceCollection.Invoke();
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        private async void PeriodicalUpdate()
        {
            while (true)
            {
                if (this._added)
                {
                    this._added = false;
                    this.CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }

                await Task.Delay(1000);
            }
        }

        private void SourceCollectionOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (this.Count > 100)
                    {
                        this._added = true;
                    }
                    else
                    {
                        var list = new List<TTransformedType>();
                        foreach (var item in notifyCollectionChangedEventArgs.NewItems)
                        {
                            if (item == null) Debugger.Break();
                            if (!(item is TSourceType)) Debugger.Break();
                            var transformedType = this.TransformItem?.Invoke(item as TSourceType);
                            if (transformedType == null) Debugger.Break();
                            list.Add(transformedType);
                        }

                        this.CollectionChanged?.Invoke(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list,
                                notifyCollectionChangedEventArgs.NewStartingIndex));
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.CollectionChanged?.Invoke(sender, notifyCollectionChangedEventArgs);
                    break;
                case NotifyCollectionChangedAction.Remove: //TODO
                    this.CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                            notifyCollectionChangedEventArgs.OldItems,
                            notifyCollectionChangedEventArgs.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Replace: //TODO
                case NotifyCollectionChangedAction.Move: //TODO
                    break;
            }
        }

       
        #region Implementation of IPagedSourceProvider<TTransformedType>

        public PagedSourceItemsPacket<TTransformedType> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            var transformedItems = new List<TTransformedType>();
            //var transformedItems = new TTransformedType[count];

            var sourceCollectionICollectionT = this.SourceCollection as ICollection<TSourceType>;
            if (sourceCollectionICollectionT != null)
            {
                foreach (var item in (sourceCollectionICollectionT.Select(items => items)).Skip(pageoffset).Take(count))
                {
                    transformedItems.Add(this.TransformItem(item));
                }

                //TODO parallel... changing to parallel disrupts order of source collectou... WTF??
                //var src = (sourceCollectionICollectionT.Select(items => items)).Skip(pageoffset).Take(count).ToArray();
                //Parallel.For(0, count, i =>
                //{
                //    transformedItems[i] = this.TransformItem(src[i]);
                //});

                //TODO parallel... changing to parallel disrupts order of source collectou... WTF??
                //Parallel.ForEach((sourceCollectionICollectionT.Select(items => items)).Skip(pageoffset).Take(count), (item, state, index) =>
                //{
                //    transformedItems[(int)index] = this.TransformItem(item);
                //});
            }
            else
            {
                var sourceCollectionEnumerator = this.SourceCollection.GetEnumerator();
                var i = 0;
                for (; i < pageoffset; i++)
                {
                    sourceCollectionEnumerator.MoveNext();
                }

                for (; i < pageoffset + count && sourceCollectionEnumerator.MoveNext(); i++)
                {
                    //transformedItems[i]=(this.TransformItem(sourceCollectionEnumerator.Current as TSourceType));
                    transformedItems.Add(this.TransformItem(sourceCollectionEnumerator.Current as TSourceType));
                }
            }

            return new PagedSourceItemsPacket<TTransformedType>
            {
                LoadedAt = DateTime.Now,
                Items = transformedItems
            };
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />,
        ///     starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
        ///     from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based
        ///     indexing.
        /// </param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="array" /> is multidimensional.-or- The number of elements
        ///     in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from
        ///     <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source
        ///     <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination
        ///     <paramref name="array" />.
        /// </exception>
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count => this.SourceCollection.Count;

        /// <summary>
        ///     Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        /// <returns>
        ///     An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </returns>
        public object SyncRoot => this.SourceCollection.SyncRoot;

        /// <summary>
        ///     Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized
        ///     (thread safe).
        /// </summary>
        /// <returns>
        ///     true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise,
        ///     false.
        /// </returns>
        public bool IsSynchronized => this.SourceCollection.IsSynchronized;

        public int IndexOf(TTransformedType item)
        {
            if (this.TransformItemBack != null && this.SourceCollectionList != null)
            {
                var sourceItem = this.TransformItemBack(item);
                if (sourceItem == null) return -1;
                var index = this.SourceCollectionList.IndexOf(sourceItem);
                return index;
            } //TODO Make sure that provider implements IList...

            //IMPORTANT Fallback very slow!!!
            var i = 0;
            foreach (var sourceItem in this.SourceCollection)
            {
                var transItem = this.TransformItem(sourceItem as TSourceType);
                if (transItem == item)
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public bool Contains(TTransformedType item)
        {
            if (this.TransformItemBack != null && this.SourceCollectionList != null)
            {
                var sourceItem = this.TransformItemBack(item);
                if (sourceItem == null) return false;
                return this.SourceCollectionList.Contains(sourceItem);
            } //TODO Make sure that provider implements IList...

            //IMPORTANT Fallback very slow!!!
            foreach (var sourceItem in this.SourceCollection)
            {
                var transItem = this.TransformItem(sourceItem as TSourceType);
                if (transItem == item)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Implementation of IPagedSourceProviderAsync<TTransformedType>

        public Task<PagedSourceItemsPacket<TTransformedType>> GetItemsAtAsync(int pageoffset, int count,
            bool usePlaceholder)
            => Task.Run(() => this.GetItemsAt(pageoffset, count, usePlaceholder));

        public TTransformedType GetPlaceHolder(int index, int page, int offset) =>
            default(TTransformedType); //this.CreatePlaceHolder.Invoke();

        public Task<int> GetCountAsync() => Task.Run(() => this.Count);
        public Task<int> IndexOfAsync(TTransformedType item) => Task.Run(() => this.IndexOf(item));

        #endregion

        #region Implementation of IEditableProvider<TTransformedType>

        public int OnAppend(TTransformedType item, object timestamp)
        {
            if (item == null) Debugger.Break();
            if (this.TransformItemBack == null)
            {
                throw new NotImplementedException("TransformItemBack method have to be provided!");
            }

            lock (this.SyncRoot)
            {
                this.AddToSourceCollection(this.TransformItemBack(item));
                return this.SourceCollection.Count - 1;
            }
        }

        public void OnInsert(int index, TTransformedType item, object timestamp)
        {
            throw new NotImplementedException();
        }

        public void OnRemove(int index, TTransformedType item, object timestamp)
        {
            if (item == null) Debugger.Break();
            lock (this.SyncRoot)
            {
                this.RemoveFromSourceCollection(this.TransformItemBack(item));
            }
        }

        public void OnReplace(int index, TTransformedType oldItem, TTransformedType newItem, object timestamp)
        {
            throw new NotImplementedException();
        }

        public int OnAppend(TSourceType item, object timestamp)
        {
            if (item == null) Debugger.Break();
            lock (this.SyncRoot)
            {
                this.AddToSourceCollection(item);
                return this.SourceCollection.Count - 1;
            }
        }

        public void OnInsert(int index, TSourceType item, object timestamp)
        {
            throw new NotImplementedException();
        }

        public void OnRemove(int index, TSourceType item, object timestamp)
        {
            throw new NotImplementedException();
        }

        public void OnRemove(TSourceType item, object timestamp)
        {
            if (item == null) Debugger.Break();
            lock (this.SyncRoot)
            {
                this.RemoveFromSourceCollection(item);
            }
        }

        public void OnReplace(int index, TSourceType oldItem, TSourceType newItem, object timestamp)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}