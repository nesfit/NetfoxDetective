using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Netfox.Core.Collections
{
    public class ConcurrentObservableHashSet<T> : IEnumerable<T>, IObservable<T>, ICollection<T>,
        IReplaySubjectImplementation<T>, INotifyCollectionChanged
    {
        private readonly HashSet<T> _hashSet = new HashSet<T>();
        private readonly object _lock = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public ConcurrentObservableHashSet()
        {
        }

        public ConcurrentObservableHashSet(IEnumerable<T> initCollection)
        {
            this.AddRange(initCollection);
        }

        public void AddRange(IEnumerable<T> initCollection)
        {
            foreach (var item in initCollection)
            {
                this.Add(item);
            }
        }

        public class ConcurrentHashSetEnumerator<TT> : IEnumerator<TT>
        {
            private TT[] _array;
            private IEnumerator _enumerator;

            public ConcurrentHashSetEnumerator(TT[] array)
            {
                this._array = array;
                this._enumerator = array.GetEnumerator();
            }

            #region Implementation of IDisposable

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this._array = null;
                this._enumerator = null;
            }

            #endregion

            #region Implementation of IEnumerator

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of
            ///     the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public bool MoveNext() => this._enumerator.MoveNext();

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public void Reset() => this._enumerator.Reset();

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>
            ///     The element in the collection at the current position of the enumerator.
            /// </returns>
            public TT Current => (TT) this._enumerator.Current;

            /// <summary>
            ///     Gets the current element in the collection.
            /// </summary>
            /// <returns>
            ///     The current element in the collection.
            /// </returns>
            object IEnumerator.Current => this._enumerator.Current;

            #endregion
        }

        #region Implementation of ICollection<T> ...ish

        public bool Add(T item)
        {
            lock (this._lock)
            {
                var inserted = this._hashSet.Add(item);
                if (inserted)
                {
                    this.OnNext(item);
                    this.CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }

                return inserted;
            }
        }

        public void Clear()
        {
            lock (this._lock)
            {
                this._hashSet.Clear();
                this.CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool Contains(T item)
        {
            lock (this._lock)
            {
                return this._hashSet.Contains(item);
            }
        }

        public bool Remove(T item)
        {
            lock (this._lock)
            {
                var ret = this._hashSet.Remove(item);
                this.RemovedItems.OnNext(item);
                this.CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return ret;
            }
        }

        public int Count
        {
            get
            {
                lock (this._lock)
                {
                    return this._hashSet.Count;
                }
            }
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            T[] arr;
            lock (this._lock)
            {
                arr = this._hashSet.ToArray();
            }

            return new ConcurrentHashSetEnumerator<T>(arr);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Implementation of IObservable<out T>

        public Subject<T> RemovedItems { get; } = new Subject<T>();

        private readonly object _gate = new object();

        private Exception _error;
        private bool _isDisposed;
        private bool _isStopped;
        private ImmutableList<IObserver<T>> _observers = new ImmutableList<IObserver<T>>();


        public bool HasObservers
        {
            get
            {
                var immutableList = this._observers;
                return immutableList?.Data.Length > 0;
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            var subscription = new Subscription((IReplaySubjectImplementation<T>) this, observer);
            lock (this._gate)
            {
                this.CheckDisposed();
                this._observers = this._observers.Add(observer);
                this.ReplayBuffer(observer);
                if (this._error != null)
                {
                    observer.OnError(this._error);
                }
                else if (this._isStopped)
                {
                    observer.OnCompleted();
                }
            }

            return subscription;
        }

        public void OnCompleted()
        {
            lock (this._gate)
            {
                this.CheckDisposed();
                if (this._isStopped)
                {
                    return;
                }

                this._isStopped = true;
                foreach (var item_0 in this._observers.Data)
                {
                    item_0.OnCompleted();
                }

                this._observers = new ImmutableList<IObserver<T>>();
            }
        }

        public void OnError(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            lock (this._gate)
            {
                this.CheckDisposed();
                if (this._isStopped)
                {
                    return;
                }

                this._isStopped = true;
                this._error = error;
                foreach (var item_0 in this._observers.Data)
                {
                    item_0.OnError(error);
                }

                this._observers = new ImmutableList<IObserver<T>>();
            }
        }

        public void OnNext(T value)
        {
            lock (this._gate)
            {
                this.CheckDisposed();
                if (this._isStopped)
                {
                    return;
                }

                foreach (var item_0 in this._observers.Data)
                {
                    item_0.OnNext(value);
                }
            }
        }

        public void Dispose(bool disposing)
        {
            lock (this._gate)
            {
                this._isDisposed = true;
                this._observers = null;
            }
        }

        public void Unsubscribe(IObserver<T> observer)
        {
            lock (this._gate)
            {
                if (this._isDisposed)
                {
                    return;
                }

                this._observers = this._observers.Remove(observer);
            }
        }


        protected void ReplayBuffer(IObserver<T> observer)
        {
            foreach (var obj in this)
            {
                observer.OnNext(obj);
            }
        }


        private void CheckDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException(string.Empty);
            }
        }


        private class Subscription : IDisposable
        {
            private IObserver<T> _observer;
            private IReplaySubjectImplementation<T> _subject;

            public Subscription(IReplaySubjectImplementation<T> subject, IObserver<T> observer)
            {
                this._subject = subject;
                this._observer = observer;
            }

            public void Dispose()
            {
                var observer = Interlocked.Exchange(ref this._observer, null);
                if (observer == null)
                {
                    return;
                }

                this._subject.Unsubscribe(observer);
                this._subject = null;
            }
        }

        private class ImmutableList<TT>
        {
            public ImmutableList()
            {
                this.Data = new TT[0];
            }

            private ImmutableList(TT[] data)
            {
                this.Data = data;
            }

            public TT[] Data { get; }

            public ImmutableList<TT> Add(TT value)
            {
                var newData = new TT[this.Data.Length + 1];
                Array.Copy(this.Data, newData, this.Data.Length);
                newData[this.Data.Length] = value;
                return new ImmutableList<TT>(newData);
            }

            private int IndexOf(TT value)
            {
                for (var i = 0; i < this.Data.Length; ++i)
                {
                    if (this.Data[i].Equals(value))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public ImmutableList<TT> Remove(TT value)
            {
                var i = this.IndexOf(value);
                if (i < 0)
                {
                    return this;
                }

                var newData = new TT[this.Data.Length - 1];
                Array.Copy(this.Data, 0, newData, 0, i);
                Array.Copy(this.Data, i + 1, newData, i, this.Data.Length - i - 1);
                return new ImmutableList<TT>(newData);
            }
        }

        #endregion

        #region Implementation of ICollection<T>

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly { get; } = false;

        #endregion

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }
}