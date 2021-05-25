// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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
using System.Collections.Generic;
using System.Threading;

namespace Netfox.Snoopers.SnooperWebmails.Models.Spotters
{
    public class SpotterPool
    {
        private readonly List<KeyValuePair<Type,SpotterBase>> _free = new List<KeyValuePair<Type, SpotterBase>>();

        private readonly List<SpotterBase> _inUse = new List< SpotterBase>();

        private int _capacity;

        private int _total;

        private object _lock = new object();

        private object _lockFree = new object();

        /// <summary>
        /// Creates new spotter pool with unbound capacity.
        /// </summary>
        public SpotterPool() : this(0) { }

        /// <summary>
        /// Creates new spotter pool with selected capacity.
        /// </summary>
        /// <param name="capacity">Pool capacity.</param>
        public SpotterPool(int capacity)
        {
            this._capacity = capacity;
            this._total = 0;

            /* initialize with spotters of all kind if capacity is not limitless */
            if(capacity > 0)
            {
                foreach(var contentType in SpotterFactory.SupportedContent)
                {
                    SpotterBase i = null;
                    // have to create multipart spotter manualy because it has to be initialized with this pool but it is yet uninitialized in SpotterFactory
                    i = contentType.StartsWith("multipart/form-data")? new SpotterMultipart(this) : SpotterFactory.GetSpotter(contentType);
                    this._total++;
                    this._free.Add(new KeyValuePair<Type, SpotterBase>(i.GetType(), i));
                }
            }
        }


        /// <summary>
        /// Returns spotter from pool or waits if no free spotter is available.
        /// </summary>
        /// <param name="contentType">Content type defining wanted spotter.</param>
        /// <returns></returns>
        public SpotterBase GetSpotterOrWait(string contentType)
        {
            lock(this._lock)
            {
                if(this.IsFreeOf(SpotterFactory.GetSpotterType(contentType)))
                {
                    // Free instances exists
                    return this.GetFreeSpotter(contentType);
                }
                else if(this._total < this._capacity || this._capacity == 0)
                {
                    // No free instances but free capacity
                    return this.CreateNextSpotter(contentType);
                }
                else
                {
                    do { Monitor.Wait(this._lock); } while(this.IsFreeOf(SpotterFactory.GetSpotterType(contentType)));
                    return this.GetFreeSpotter(contentType);
                }
            }

        }

        /// <summary>
        /// Gets spotter from pool. If no spotter is available and capacity is reached exeption is raised.
        /// </summary>
        /// <param name="contentType">Content type defining wanted spotter.</param>
        /// <returns></returns>
        public SpotterBase GetSpotter(string contentType)
        {
            lock(this._lock)
            {
                if(this.IsFreeOf(SpotterFactory.GetSpotterType(contentType))) { return this.GetFreeSpotter(contentType); }
                else
                { return this.CreateNextSpotter(contentType); }
            }
        }

        /// <summary>
        /// Gets free spotter or null if no free spotters are available. Does not try to create new spotter even if there is free capacity.
        /// </summary>
        /// <param name="contentType">Content type defining wanted spotter.</param>
        /// <returns></returns>
        public SpotterBase GetFreeSpotter(string contentType)
        {
            lock(this._lock)
            {
                if(this.IsFreeOf(SpotterFactory.GetSpotterType(contentType)))
                {
                    return this.GetUsed(SpotterFactory.GetSpotterType(contentType));
                }
                else
                { return null; }
            }
        }

        /// <summary>
        /// Returns spotter to the pool. If Returning spotter that was not flaged as used exeption is raised.
        /// </summary>
        /// <param name="spotter">Returning spotter.</param>
        public void ReturningSpotter(SpotterBase spotter)
        {
            lock(this._lock)
            {
                if(!this._inUse.Contains(spotter)) { throw new ArgumentException("Can not return spotter that is not in use."); }
                this._inUse.Remove(spotter);
                this._free.Add(new KeyValuePair<Type, SpotterBase>(spotter.GetType(), spotter));
                Monitor.Pulse(this._lock);
            }
        }

        private SpotterBase GetUsed(Type spotterType)
        {
            foreach(var spotterPair in this._free)
            {
                if(spotterPair.Key == spotterType)
                {
                    this._free.Remove(spotterPair);
                    this._inUse.Add(spotterPair.Value);
                    spotterPair.Value.Clean();
                    return spotterPair.Value;
                }
            }

            return null;
        }

        private SpotterBase CreateNextSpotter(string contentType)
        {
            if(this._capacity != 0 && this._total >= this._capacity)
            {
                throw new Exception("Out of pool capacity.");
            }

            var i = SpotterFactory.GetSpotter(contentType);
            this._total++;
            this._inUse.Add(i);
            return i;
            

        }

        private bool IsFreeOf(Type spotterType)
        {
            foreach(var spotterPair in this._free) { if(spotterPair.Key == spotterType) return true; }

            return false;
        }
    }
}
