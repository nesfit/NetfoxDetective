using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;

namespace AlphaChiTech.Virtualization
{

    /// <summary>
    /// Implementation of ReplaySubject in Rx Framework... reimplement with care!!!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReplaySubject<T> : ISubject<T>, ISubject<T, T>, IObserver<T>, IObservable<T>, IDisposable
    {
        private readonly object _gate = new object();

        private Exception _error;
        private bool _isDisposed;
        private bool _isStopped;
        private ImmutableList<IObserver<T>> _observers;

        protected ReplaySubject()
        {
            this._observers = new ImmutableList<IObserver<T>>();
            this.Queue = new Queue<T>(0);
        }

        public bool HasObservers
        {
            get
            {
                var immutableList = this._observers;
                return immutableList?.Data.Length > 0;
            }
        }

        protected Queue<T> Queue { get; }

        #region Implementation of IDisposable
        public void Dispose() { this.Dispose(true); }
        #endregion

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if(observer == null) { throw new ArgumentNullException(nameof(observer)); }
            var subscription = new Subscription((IReplaySubjectImplementation) this, observer);
            lock(this._gate)
            {
                this.CheckDisposed();
                this._observers = this._observers.Add(observer);
                this.ReplayBuffer(observer);
                if(this._error != null) { observer.OnError(this._error); }
                else if(this._isStopped) { observer.OnCompleted(); }
            }
            return subscription;
        }

        public void OnCompleted()
        {
            lock(this._gate)
            {
                this.CheckDisposed();
                if(this._isStopped) { return; }
                this._isStopped = true;
                foreach(var item_0 in this._observers.Data) { item_0.OnCompleted(); }
                this._observers = new ImmutableList<IObserver<T>>();
            }
        }

        public void OnError(Exception error)
        {
            if(error == null) { throw new ArgumentNullException("error"); }
            lock(this._gate)
            {
                this.CheckDisposed();
                if(this._isStopped) { return; }
                this._isStopped = true;
                this._error = error;
                foreach(var item_0 in this._observers.Data) { item_0.OnError(error); }
                this._observers = new ImmutableList<IObserver<T>>();
            }
        }

        public void OnNext(T value)
        {
            lock(this._gate)
            {
                this.CheckDisposed();
                if(this._isStopped) { return; }
                this.AddValueToBuffer(value);
                foreach(var item_0 in this._observers.Data) { item_0.OnNext(value); }
            }
        }

        public void Dispose(bool disposing)
        {
            lock(this._gate)
            {
                this._isDisposed = true;
                this._observers = null;
            }
            this.Queue.Clear();
        }

        public void Unsubscribe(IObserver<T> observer)
        {
            lock(this._gate)
            {
                if(this._isDisposed) { return; }
                this._observers = this._observers.Remove(observer);
            }
        }

        protected void AddValueToBuffer(T value) { this.Queue.Enqueue(value); }

        protected void ReplayBuffer(IObserver<T> observer) { foreach(var obj in this.Queue) { observer.OnNext(obj); } }


        private void CheckDisposed() { if(this._isDisposed) { throw new ObjectDisposedException(string.Empty); } }

        private interface IReplaySubjectImplementation : ISubject<T>, ISubject<T, T>, IObserver<T>, IObservable<T>, IDisposable
        {
            bool HasObservers { get; }

            void Unsubscribe(IObserver<T> observer);
        }

        private class Subscription : IDisposable
        {
            private IObserver<T> _observer;
            private IReplaySubjectImplementation _subject;

            public Subscription(IReplaySubjectImplementation subject, IObserver<T> observer)
            {
                this._subject = subject;
                this._observer = observer;
            }

            public void Dispose()
            {
                var observer = Interlocked.Exchange(ref this._observer, null);
                if(observer == null) { return; }
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
    }

    
}