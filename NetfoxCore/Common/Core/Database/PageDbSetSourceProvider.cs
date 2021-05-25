using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AlphaChiTech.VirtualizingCollection.Pageing;
using AlphaChiTech.VirtualizingCollection.Interfaces;

namespace Netfox.Core.Database
{
    public class PageDbSetSourceProvider<T> : INotifyImmediately, IPagedSourceObservableProvider<T>
        where T : class, IEntity
    {
        private ObservableDbSetRepository<T> ObservableDbSetRepository { get; }

        public bool IsNotifyImmidiately
        {
            get => this.ObservableDbSetRepository.IsNotifyImmidiately;
            set => this.ObservableDbSetRepository.IsNotifyImmidiately = value;
        }

        public PageDbSetSourceProvider([NotNull] ObservableDbSetRepository<T> observableDbSetRepository)
        {
            this.ObservableDbSetRepository = observableDbSetRepository;
            this.ObservableDbSetRepository.CollectionChanged += this.OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Remove: //TODO
                case NotifyCollectionChangedAction.Replace: //TODO
                case NotifyCollectionChangedAction.Move: //TODO
                    this.CollectionChanged?.Invoke(sender, notifyCollectionChangedEventArgs);
                    break;
            }
        }

        #region Implementation of IBaseSourceProvider

        public void OnReset(int count)
        {
            //Debugger.Break();
        }

        public bool Contains(T item)
        {
            return this.ObservableDbSetRepository.Contains(item);
        }

        #endregion
        

        #region Implementation of ICollection

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing. </param><param name="index">The zero-based index in <paramref name="array"/> at which copying begins. </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero. </exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>.-or-The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(Array array, int index)
        {
            Debugger.Break();
        }

        int IPagedSourceProvider<T>.Count => this.ObservableDbSetRepository.Count;

        public int IndexOf(T item)
        {
            return this.ObservableDbSetRepository.IndexOf(item);
        }

        public PagedSourceItemsPacket<T> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            var pagedSourceItemsPacket = new PagedSourceItemsPacket<T>
            {
                //Items = this.ObservableDbSetRepository.Query.OrderBy(i=>i.FirstSeen).Skip(pageoffset).Take(count).ToArray()
                Items = this.ObservableDbSetRepository.SkipTake(pageoffset, count)
            };
            return pagedSourceItemsPacket;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        //int ICollection.Count => this.ObservableDbSetRepository.Count;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        public object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.
        /// </returns>
        public bool IsSynchronized { get; }

        #endregion

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Implementation of IEditableProvider<in T>

        public int OnAppend(T item, object timestamp)
        {
            //  Debugger.Break();
            this.ObservableDbSetRepository.Insert(item); //.Wait();
            return this.IndexOf(item);
        }

        public void OnInsert(int index, T item, object timestamp)
        {
            Debugger.Break();
            throw new NotImplementedException();
        }

        public void OnReplace(int index, T oldItem, T newItem, object timestamp)
        {
            Debugger.Break();
            throw new NotImplementedException();
        }

        #endregion
    }
}