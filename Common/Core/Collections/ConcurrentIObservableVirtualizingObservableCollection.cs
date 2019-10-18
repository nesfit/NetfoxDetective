using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;

using AlphaChiTech.VirtualizingCollection.Interfaces;

namespace AlphaChiTech.Virtualization
{
    internal interface IReplaySubjectImplementation<T> : ISubject<T>, ISubject<T, T>, IObserver<T>, IObservable<T>, IDisposable
    {
        bool HasObservers { get; }

        void Unsubscribe(IObserver<T> observer);
    }

    public class ConcurrentIObservableVirtualizingObservableCollection<T> : VirtualizingObservableCollection<T>, IReplaySubjectImplementation<T>, IObservable<T>, IObservableCollection<T> where T : class
    {
        public ConcurrentIObservableVirtualizingObservableCollection(IItemSourceProvider<T> provider) : base(provider) { throw new NotImplementedException(); }
        public ConcurrentIObservableVirtualizingObservableCollection(IItemSourceProviderAsync<T> asyncProvider) : base(asyncProvider) { throw new NotImplementedException(); }

        public ConcurrentIObservableVirtualizingObservableCollection(
            IPagedSourceProvider<T> provider,
            IPageReclaimer<T> reclaimer = null,
            IPageExpiryComparer expiryComparer = null,
            int pageSize = 100,
            int maxPages = 100,
            int maxDeltas = -1,
            int maxDistance = -1) : base(provider, reclaimer, expiryComparer, pageSize, maxPages, maxDeltas, maxDistance) { throw new NotImplementedException(); }

        public ConcurrentIObservableVirtualizingObservableCollection(IEnumerable<T> sourceCollection) : base(new VirtualizingObservableCollectionSource<T>()) { this.AddRange(sourceCollection); }
        public ConcurrentIObservableVirtualizingObservableCollection() : base(new VirtualizingObservableCollectionSource<T>()) { }


        public new void CopyTo(T[] array, int arrayIndex)
        {
            var provider = this.Provider as VirtualizingObservableCollectionSource<T>;
            if(provider != null)
            {
                var index = arrayIndex;
                foreach(var item in this)
                {
                    if(index >= array.Length) break;
                    provider[index] = item;
                    index++;
                }
                return;
            }
            throw new NotImplementedException("Copy to failed... implement in your provider!");
        }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
        public new IEnumerator<T> GetEnumerator() => new Enumerator<T>(this);
        public void ResumeCollectionChangeNotification() { } //todo
        public void SuspendCollectionChangeNotification() { } //todo
        private IEnumerator GetBaseEnumerator() => base.GetEnumerator();

        public new class Enumerator<TT> : IEnumerator<TT> where TT : class
        {
            private readonly IEnumerator _enumerator;
            private readonly ConcurrentIObservableVirtualizingObservableCollection<TT> _sourceCollection;
            

            internal Enumerator(ConcurrentIObservableVirtualizingObservableCollection<TT> sourceCollection)
            {
                this._sourceCollection = sourceCollection;
                this._enumerator = this._sourceCollection.GetBaseEnumerator();
            }

            #region Implementation of IDisposable
            public void Dispose() { }
            #endregion

            #region Implementation of IEnumerator
            public bool MoveNext() => this._enumerator.MoveNext();
            public void Reset() => this._enumerator.Reset();

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>
            ///     The element in the collection at the current position of the enumerator.
            /// </returns>
            public TT Current => this._enumerator.Current as TT;

            object IEnumerator.Current => this.Current;
            #endregion
        }

        #region Implementation of IObservableCollection
        public new void Add(T item)
        {
            base.Add(item);
            this.OnNext(item);
        }
        public new void Add(object item)
        {
            this.Add((T)item);
        }

        bool IObservableCollection.Remove(object item)
        {
            return base.Remove((T)item);
        }

        bool IObservableCollection<T>.Remove(object item) { return base.Remove((T)item); }

        public new void AddRange(IEnumerable<T> items, object timestamp = null)
        {
            foreach(var item in items)
            {
                base.Add(item);
                this.OnNext(item);
            }
        }
        #endregion

        #region Implementation of IObservable<out T>
        private readonly object _gate = new object();

        private Exception _error;
        private bool _isDisposed;
        private bool _isStopped;
        private ImmutableList<IObserver<T>> _observers = new ImmutableList<IObserver<T>>();


        public new bool HasObservers
        {
            get
            {
                var immutableList = this._observers;
                return immutableList?.Data.Length > 0;
            }
        }
        
        #region Implementation of IDisposable
        public new void Dispose() { this.Dispose(true); }
        #endregion

        public new IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null) { throw new ArgumentNullException(nameof(observer)); }
            var subscription = new Subscription((IReplaySubjectImplementation<T>)this, observer);
            lock (this._gate)
            {
                this.CheckDisposed();
                this._observers = this._observers.Add(observer);
                this.ReplayBuffer(observer);
                if (this._error != null) { observer.OnError(this._error); }
                else if (this._isStopped) { observer.OnCompleted(); }
            }
            return subscription;
        }

        public new void OnCompleted()
        {
            lock (this._gate)
            {
                this.CheckDisposed();
                if (this._isStopped) { return; }
                this._isStopped = true;
                foreach (var item_0 in this._observers.Data) { item_0.OnCompleted(); }
                this._observers = new ImmutableList<IObserver<T>>();
            }
        }

        public new void OnError(Exception error)
        {
            if (error == null) { throw new ArgumentNullException("error"); }
            lock (this._gate)
            {
                this.CheckDisposed();
                if (this._isStopped) { return; }
                this._isStopped = true;
                this._error = error;
                foreach (var item_0 in this._observers.Data) { item_0.OnError(error); }
                this._observers = new ImmutableList<IObserver<T>>();
            }
        }

        public new void OnNext(T value)
        {
            lock (this._gate)
            {
                this.CheckDisposed();
                if (this._isStopped) { return; }
                foreach (var item_0 in this._observers.Data) { item_0.OnNext(value); }
            }
        }

        public new void Dispose(bool disposing)
        {
            lock (this._gate)
            {
                this._isDisposed = true;
                this._observers = null;
            }
        }

        public new void Unsubscribe(IObserver<T> observer)
        {
            lock (this._gate)
            {
                if (this._isDisposed) { return; }
                this._observers = this._observers.Remove(observer);
            }
        }
        

        protected new void ReplayBuffer(IObserver<T> observer) { foreach (var obj in this) { observer.OnNext(obj); } }


        private void CheckDisposed() { if (this._isDisposed) { throw new ObjectDisposedException(string.Empty); } }


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
                if (observer == null) { return; }
                this._subject.Unsubscribe(observer);
                this._subject = null;
            }
        }

        private class ImmutableList<TT>
        {
            public ImmutableList() { this.Data = new TT[0]; }

            private ImmutableList(TT[] data) { this.Data = data; }

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
                for (var i = 0; i < this.Data.Length; ++i) { if (this.Data[i].Equals(value)) { return i; } }
                return -1;
            }

            public ImmutableList<TT> Remove(TT value)
            {
                var i = this.IndexOf(value);
                if (i < 0) { return this; }
                var newData = new TT[this.Data.Length - 1];
                Array.Copy(this.Data, 0, newData, 0, i);
                Array.Copy(this.Data, i + 1, newData, i, this.Data.Length - i - 1);
                return new ImmutableList<TT>(newData);
            }
        }

        #endregion

    }
}