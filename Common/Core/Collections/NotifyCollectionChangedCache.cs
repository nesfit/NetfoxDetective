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

//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using AlphaChiTech.Virtualization;

//namespace Netfox.Core.Collections
//{
//    public class NotifyCollectionChangedCache<T>
//    {
//        public IPagedSourceProvider<T> Provider { get; }
//        public CancellationToken CancellationToken { get; }

//        public List<T> Cache { get; } = new List<T>(); 

//        /// <summary>
//        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
//        /// </summary>
//        public NotifyCollectionChangedCache(IPagedSourceObservableProvider<T> provider) {
//            this.Provider = provider;
//            this.CancellationToken = new CancellationToken();
//            Task.Run(()=> this.PeriodicalCheck());
//            this.CheckTimePeriod = new TimeSpan(0,0,0,1);

//            provider.CollectionChanged +=ProviderOnCollectionChanged;
//        }

//        private void ProviderOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
//        {
//            switch(notifyCollectionChangedEventArgs.Action)
//            {
//                case NotifyCollectionChangedAction.Add:
//                    lock(this.Cache) {
//                        this.Cache.AddRange(notifyCollectionChangedEventArgs.NewItems.Cast<T>());
//                    }
//                    break;
//                case NotifyCollectionChangedAction.Remove:
//                    break;
//                case NotifyCollectionChangedAction.Replace:
//                    break;
//                case NotifyCollectionChangedAction.Move:
//                    break;
//                case NotifyCollectionChangedAction.Reset:
//                    break;
//            }
           
//        }

//        public TimeSpan CheckTimePeriod { get; }

//        private void PeriodicalCheck()
//        {
//            while(true)
//            {
//                Task.Delay(this.CheckTimePeriod);
                
//                Provider.Count
//            }
//        }

//    }
//}
