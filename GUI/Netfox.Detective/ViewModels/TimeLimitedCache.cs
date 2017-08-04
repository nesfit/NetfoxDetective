// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Netfox.Detective.ViewModels
{
    public class TimeLimitedCachee<TKey, TValue> where TValue : class
    {
        private readonly ConcurrentDictionary<TKey, TimeLimitedCacheValue> _cache = new ConcurrentDictionary<TKey, TimeLimitedCacheValue>();
        private readonly object _timerOpLock = new object();
        private int _ExpiryInProgress;
        private int _ExpiryRunning;
        private Timer _ExpiryTimer;
        private TimeSpan _ExpiryTimeSpan;
        public TimeLimitedCachee(TimeSpan expiryTimeSpan) { this._ExpiryTimeSpan = expiryTimeSpan; }

        public TValue GetValueOrSet(TKey key, Func<TValue> func, object lockToken)
        {
            TimeLimitedCacheValue timeLimitedValue;
            if(this._cache.TryGetValue(key, out timeLimitedValue))
            {
                timeLimitedValue.LastAccess = DateTime.Now;
                return timeLimitedValue.Value;
            }
            lock(lockToken)
            {
                if(this._cache.TryGetValue(key, out timeLimitedValue))
                {
                    timeLimitedValue.LastAccess = DateTime.Now;
                    return timeLimitedValue.Value;
                }
                var value = func();
                var chacheValue = new TimeLimitedCacheValue(value);
                this._cache.TryAdd(key, chacheValue);
                return value;
            }
        }

        public TValue GetValueOrSetAsync(TKey key, Func<TValue> func, Action<Task> continueWith, TValue defaultValue, object lockToken, TimeSpan? funcTimespanLimit = null)
        {
            TimeLimitedCacheValue timeLimitedValue;
            if(this._cache.TryGetValue(key, out timeLimitedValue))
            {
                timeLimitedValue.LastAccess = DateTime.Now;
                return timeLimitedValue.Value;
            }
            lock(lockToken)
            {
                if(this._cache.TryGetValue(key, out timeLimitedValue))
                {
                    timeLimitedValue.LastAccess = DateTime.Now;
                    return timeLimitedValue.Value;
                }
                try
                {
                    this._cache.AddOrUpdate(key, k => timeLimitedValue = new TimeLimitedCacheValue(defaultValue), (k, v) => timeLimitedValue = v);

                    if(funcTimespanLimit == null)
                    {
                        var waitedTask = Task.Factory.StartNew(() =>
                        {
                            timeLimitedValue.Value = func();
                            timeLimitedValue.LastAccess = DateTime.Now;
                            this.CreateTimer();
                        }).ContinueWith(continueWith);
                    }
                    else
                    {
                        var waitedTask = Task.Factory.StartNew(() =>
                        {
                            timeLimitedValue.Value = func();
                            timeLimitedValue.LastAccess = DateTime.Now;
                            this.CreateTimer();
                        });
                        var waitingTask = Task.Factory.StartNew(() => { waitedTask.Wait((TimeSpan) funcTimespanLimit); }).ContinueWith(continueWith);
                    }


                    return defaultValue;
                }
                catch(Exception) {
                    throw;
                }
            }
        }

        public void Invalidate(TKey key)
        {
            TimeLimitedCacheValue val;
            this._cache.TryRemove(key, out val);

            if(this._cache.IsEmpty) { this.DisposeTimer(); }
        }

        public void InvalidateAll()
        {
            this._cache.Clear();
            this.DisposeTimer();
        }

        public void SetValue(TKey key, TValue value)
        {
            this.CreateTimer();
            this._cache.AddOrUpdate(key, kk => new TimeLimitedCacheValue(value)
            {
                LastAccess = DateTime.Now
            }, (k, v) => new TimeLimitedCacheValue(value)
            {
                LastAccess = DateTime.Now
            });
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            TimeLimitedCacheValue timeLimitedValue;
            if(this._cache.TryGetValue(key, out timeLimitedValue))
            {
                timeLimitedValue.LastAccess = DateTime.Now;
                value = timeLimitedValue.Value;
                return true;
            }

            value = null;
            return false;
        }

        private void CreateTimer()
        {
            if(Interlocked.CompareExchange(ref this._ExpiryRunning, 1, 0) == 0) {
                lock(this._timerOpLock) { this._ExpiryTimer = new Timer(o => this.Expire(), null, this._ExpiryTimeSpan, this._ExpiryTimeSpan); }
            }
        }

        private void DisposeTimer()
        {
            if(Interlocked.CompareExchange(ref this._ExpiryRunning, 0, 1) != 1) { return; }

            lock(this._timerOpLock)
            {
                if(!this._cache.IsEmpty) { return; }
                if(this._ExpiryTimer == null) { return; }

                this._ExpiryTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.Zero);
                this._ExpiryTimer.Dispose();
                this._ExpiryTimer = null;
            }
        }

        private void Expire()
        {
            if(Interlocked.CompareExchange(ref this._ExpiryInProgress, 1, 0) == 1) { return; }

            try
            {
                var maxAge = DateTime.Now - this._ExpiryTimeSpan;
                var toRemove = this._cache.Where(pair => pair.Value.LastAccess != DateTime.MinValue && pair.Value.LastAccess < maxAge).ToArray();
                foreach(var keyValuePair in toRemove)
                {
                    TimeLimitedCacheValue value;
                    this._cache.TryRemove(keyValuePair.Key, out value);
                }

                if(this._cache.IsEmpty) { this.DisposeTimer(); }
            }
            finally {
                this._ExpiryInProgress = 0;
            }
        }

        private sealed class TimeLimitedCacheValue
        {
            public int createLock = 0;
            public DateTime LastAccess;
            public TValue Value;

            public TimeLimitedCacheValue(TValue value)
            {
                this.Value = value;
                this.LastAccess = DateTime.MinValue;
                //LastAccess = DateTime.Now;
            }
        }
    }
}