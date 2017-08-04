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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Netfox.Core.Collections
{
    public static class BlockingCollectionEx
    {
        public static Partitioner<T> GetConsumingPartitioner<T>(this BlockingCollection<T> collection) { return new BlockingCollectionPartitioner<T>(collection); }

        private class BlockingCollectionPartitioner<T> : Partitioner<T>

        {
            private readonly BlockingCollection<T> _collection;

            internal BlockingCollectionPartitioner(BlockingCollection<T> collection)

            {
                if(collection == null) { throw new ArgumentNullException("collection"); }

                this._collection = collection;
            }

            public override bool SupportsDynamicPartitions => true;

            public override IList<IEnumerator<T>> GetPartitions(int partitionCount)

            {
                if(partitionCount < 1) { throw new ArgumentOutOfRangeException("partitionCount"); }

                var dynamicPartitioner = this.GetDynamicPartitions();

                return Enumerable.Range(0, partitionCount).Select(_ => dynamicPartitioner.GetEnumerator()).ToArray();
            }

            public override IEnumerable<T> GetDynamicPartitions() { return this._collection.GetConsumingEnumerable(); }
        }
    }
}