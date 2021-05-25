using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Netfox.Core.Helpers;

namespace Netfox.Detective.Core.BaseTypes
{
    public class BindablePeriodicalTask<TResult> : NotifyTaskCompletion<TResult>, IDisposable where TResult : class
    {
        private readonly INotifyCollectionChanged _observableCollection;

        private readonly INotifyCollectionChanged[] _observableCollections;

        private readonly IDisposable _subscription;
        private bool _sourceUpdatedFlag;


        public BindablePeriodicalTask(Func<Task<TResult>> initFunction, IObservable<TResult> hotObservableSequence)
        {
            this.Init(initFunction);
            this._subscription = hotObservableSequence.Subscribe(item => this.SourceUpdated());
        }

        public BindablePeriodicalTask(Func<Task<TResult>> initFunction, INotifyCollectionChanged observableCollection)
        {
            this._observableCollection = observableCollection;
            observableCollection.CollectionChanged += this.ObservableCollectionOnCollectionChanged;

            this.Init(initFunction);
        }

        public BindablePeriodicalTask(Func<Task<TResult>> initFunction,
            IEnumerable<INotifyCollectionChanged> observableCollections)
        {
            this._observableCollections =
                observableCollections as INotifyCollectionChanged[] ?? observableCollections.ToArray();
            foreach (var observableCollection in this._observableCollections)
            {
                observableCollection.CollectionChanged += this.ObservableCollectionOnCollectionChanged;
            }

            this.Init(initFunction);
        }

        public Func<Task<TResult>> InitFunction { get; set; }

        public Timer Timer { get; private set; }

        private void Init(Func<Task<TResult>> initFunction)
        {
            this.InitFunction = initFunction;
            this.RunTask();

            this.Timer = new Timer(state => this.Update(), null, new TimeSpan(0, 0, 0, 1), new TimeSpan(0, 0, 0, 1));
        }

        private void ObservableCollectionOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) => this.SourceUpdated();

        private void RunTask()
        {
            this.Task = this.InitFunction.Invoke();
            if (!this.Task.IsCompleted)
            {
                var _ = this.WatchTaskAsync();
            }
        }

        private void SourceUpdated()
        {
            this._sourceUpdatedFlag = true;
        }

        private void Update()
        {
            if (!this._sourceUpdatedFlag || !this.IsCompleted)
            {
                return;
            }

            this._sourceUpdatedFlag = false;
            this.DefaultResult = base.Result;
            this.RunTask();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            this._subscription?.Dispose();
            if (this._observableCollection != null)
            {
                this._observableCollection.CollectionChanged -= this.ObservableCollectionOnCollectionChanged;
            }

            foreach (var observableCollection in this._observableCollections)
            {
                observableCollection.CollectionChanged -= this.ObservableCollectionOnCollectionChanged;
            }
        }

        #endregion
    }
}