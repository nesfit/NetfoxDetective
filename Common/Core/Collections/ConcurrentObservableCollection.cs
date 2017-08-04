// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using Castle.Core.Logging;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces;
using Netfox.Core.Messages;

namespace Netfox.Core.Collections
{
    public class ConcurrentObservableCollection<T> : ObservableCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged, IList<T>, IEnumerable<T>, ICollection<T>, ILoggable
    {
        public ConcurrentObservableCollection() {}
        public ConcurrentObservableCollection(List<T> list) : base(list) {}
        public ConcurrentObservableCollection(IEnumerable<T> collection) : base(collection) {}

        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) { base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName)); }

        #region Notification manipulation
        private bool _isCollectionChangeNotificationEnabled = true;

        public bool IsCollectionChangeNotificationEnabled
        {
            get { return this._isCollectionChangeNotificationEnabled; }
            set
            {
                this._isCollectionChangeNotificationEnabled = value;
                if(value) { this.NotifyChanges(); }
            }
        }

        public bool IsReadOnly { get; } = false;
 
        private void NotifyChanges()
        {
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(arg);
        }

        public void ResumeCollectionChangeNotification() { this.IsCollectionChangeNotificationEnabled = true; }

        public void SuspendCollectionChangeNotification() { this.IsCollectionChangeNotificationEnabled = false; }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            //if(!IsCollectionChangeNotificationEnabled) return;
            // using (this.BlockReentrancy())
            //this.CollectionChanged((object)this, e);
            var eventHandler = this.CollectionChanged;
            if(eventHandler == null) { return; }

            // Walkthru invocation list.
            var delegates = eventHandler.GetInvocationList();

            foreach(var @delegate in delegates)
            {
                var handler = (NotifyCollectionChangedEventHandler) @delegate;
                // If the subscriber is a DispatcherObject and different thread.
                var dispatcherObject = handler.Target as DispatcherObject;

                if(dispatcherObject != null && !dispatcherObject.CheckAccess())
                {
                    // Invoke handler in the target dispatcher's thread... 
                    // asynchronously for better responsiveness.
                    dispatcherObject.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, handler, this, e);
                }
                else
                {
                    // Execute handler as is.
                    try { handler(this, e); }
                    catch(InvalidOperationException ex) //Accessing components owned by UI
                    {
                        //throw;
                        this.Logger?.Warn("ConcurrentObservableCollection - UI Access failed, thread related issue", ex);
                        DispatcherHelper.UIDispatcher.BeginInvoke(DispatcherPriority.DataBind, handler, this, e);
                    }
                    catch(IndexOutOfRangeException ex)
                    {
                        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    catch (Exception ex) //Accessing components owned by UI
                    {
                        //throw;
                        this.Logger?.Error($"ConcurrentObservableCollection - {ex.Message}", ex);
                        Debugger.Break();
                        throw;
                    }
                }
            }
        }
        #endregion

        #region Cunccurency fasade
        private object _lock = new object();

        protected override void InsertItem(int index, T item)
        {
            lock(this._lock) { this.Items.Insert(index, item); }
            if(this.IsCollectionChangeNotificationEnabled)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                //TODO Slow on 2 cores, test on server
                //ThreadPool.UnsafeQueueUserWorkItem((a) =>
                //{
                //    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item,
                //        index));
                //},null);
            }
            this.OnPropertyChanged(nameof(this.Count));
            //this.OnPropertyChanged("Item[]");
        }

        protected override void RemoveItem(int index)
        {
            object obj;
            lock(this._lock)
            {
                obj = this.Items[index];
                this.Items.RemoveAt(index);
            }
            this.OnPropertyChanged(nameof(this.Count));
            //.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, obj, index));
        }

        protected override void ClearItems() { lock(this._lock) { base.ClearItems(); } }

        //protected override void SetItem(int index, T item)
        //{
        //    lock (_lock)
        //    {
        //        base.SetItem(index, item);
        //    }
        //}

        //protected virtual void MoveItem(int oldIndex, int newIndex)
        //{
        //    lock (_lock)
        //    {
        //        base.MoveItem(oldIndex, newIndex);
        //    }
        //}
        #endregion

        #region Extenstions
        public void AddItems(IEnumerable<T> items)
        {
            lock(this._lock)
            {
                this.SuspendCollectionChangeNotification();
                (this.Items as List<T>).AddRange(items);
                this.ResumeCollectionChangeNotification();
                this.OnPropertyChanged(nameof(this.Count));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private Timer _bulkAddTimer;
        private TimeSpan _bulkAddExpiryTimeSpan;

        private int _bulkTimerRunning;
        private object _bulkTimerOpLock = new object();

        private ConcurrentBag<T> bulkBag = new ConcurrentBag<T>();

        public void AddBulk(T item, int interval)
        {
            if(Interlocked.CompareExchange(ref this._bulkTimerRunning, 1, 0) == 0)
            {
                lock(this._bulkTimerOpLock)
                {
                    this._bulkAddExpiryTimeSpan = new TimeSpan(0, 0, 0, 0, interval);
                    this._bulkAddTimer = new Timer(o => this.BulkAddTimeout(), null, this._bulkAddExpiryTimeSpan, this._bulkAddExpiryTimeSpan);
                }
            }

            this.bulkBag.Add(item);
        }

        private void BulkAddTimeout()
        {
            if(Interlocked.CompareExchange(ref this._bulkTimerRunning, 0, 1) == 1)
            {
                lock(this._bulkTimerOpLock)
                {
                    if(this._bulkAddTimer != null)
                    {
                        this._bulkAddTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.Zero);
                        this._bulkAddTimer.Dispose();
                        this._bulkAddTimer = null;

                        lock(this._lock)
                        {
                            //List<T> added = new List<T>();
                            while(!this.bulkBag.IsEmpty)
                            {
                                T item;
                                if(this.bulkBag.TryTake(out item)) { this.Add(item); }
                                else
                                { break; }
                            }
                        }
                    }
                }

                this.OnPropertyChanged(nameof(this.Count));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        #endregion

        public new IEnumerator<T> GetEnumerator()
        {
            List<T> immutable;
            lock (this._lock) {
                immutable = base.Items.ToList();
            }
            return immutable.GetEnumerator();
        }
         IEnumerator IEnumerable.GetEnumerator()
        {
            List<T> immutable;
            lock (this._lock)
            {
                immutable = base.Items.ToList();
            }
            return immutable.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            List<T> immutable;
            lock (this._lock)
            {
                immutable = base.Items.ToList();
            }
            return immutable.GetEnumerator();
        }

        #region Implementation of ILoggable
        public ILogger Logger { get; set; }
        #endregion
    }
}