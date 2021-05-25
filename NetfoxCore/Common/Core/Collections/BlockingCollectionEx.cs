using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Netfox.Core.Collections
{
    public static class BlockingCollectionEx
    {
        public static Partitioner<T> GetConsumingPartitioner<T>(this BlockingCollection<T> collection)
        {
            return new BlockingCollectionPartitioner<T>(collection);
        }

        private class BlockingCollectionPartitioner<T> : Partitioner<T>
        {
            private readonly BlockingCollection<T> _collection;

            internal BlockingCollectionPartitioner(BlockingCollection<T> collection)
            {
                this._collection = collection ?? throw new ArgumentNullException(nameof(collection));
            }

            public override bool SupportsDynamicPartitions => true;
            public override IEnumerable<T> GetDynamicPartitions() => this._collection.GetConsumingEnumerable();

            public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
            {
                if (partitionCount < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(partitionCount));
                }

                var dynamicPartitioner = this.GetDynamicPartitions();
                return Enumerable.Range(0, partitionCount)
                    .Select(_ => dynamicPartitioner.GetEnumerator())
                    .ToArray();
            }
        }
    }
}