using System;
using System.Threading;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using NUnit.Framework;

namespace VirtualizingObservableCollectionTests
{
    [TestFixture()]
    public class ConcurrentIObservableVirtualizingObservableCollectionConcurrencyTests : ConcurrencyTestsBase
    {
        [Test]
        public void ConcurrentIObservableVirtualizingObservableCollection()
        {
            var currentMethodName = this.GetCurrentMethodName();
            var collection = new VirtualizingObservableCollectionSource<Item>();
            this._testStopwatch.Start($"{currentMethodName} iterative add with notifications");
            for (var i = 0; i < Iterations; i++) { collection.Add(_data[i]); }
            this._testStopwatch.Stop();
            this.CheckDataConsistency(collection);
            collection.Clear();

            this._testStopwatch.Start($"{currentMethodName} iterative add without notifications");
            //collection.SuppressChangeNotifications();
            for (var i = 0; i < Iterations; i++) { collection.Add(_data[i]); }
            //collection.Reset();
            this._testStopwatch.Stop();
            this.CheckDataConsistency(collection);
            collection.Clear();

            //this._testStopwatch.Start($"{currentMethodName} bulk add");
            //collection.AddRange(_data);
            //this._testStopwatch.Stop();
            //collection.Clear();

            this._testStopwatch.Start($"{currentMethodName} parallel add with notifications");
            Parallel.ForEach(_data, item => { collection.Add(item); });
            this._testStopwatch.Stop();
            this.CheckDataConsistency(collection);
            collection.Clear();

            this._testStopwatch.Start($"{currentMethodName} parallel add without notifications");
            //collection.SuppressChangeNotifications();
            Parallel.ForEach(_data, item => { collection.Add(item); });
            //collection.Reset();
            this._testStopwatch.Stop();
            this.CheckDataConsistency(collection);
            collection.Clear();

            collection.Clear();
            this._testStopwatch.Start($"{currentMethodName} parallel add int without interlocked");
            Parallel.ForEach(_data, item => { collection.Add(item); });
            this._testStopwatch.Stop();
            this.CheckDataConsistency(collection);
            collection.Clear();

            this._testStopwatch.Start($"{currentMethodName} Iterative add  with parallel read");
            Parallel.Invoke(() => this.Add(collection), () => this.Iteration(collection));
            this.CheckDataConsistency(collection);
            this._testStopwatch.Stop();
            this.CheckDataConsistency(collection);
            collection.Clear();

            this._testStopwatch.Start($"{currentMethodName} Iterative add  with parallel read and remove");
            var task = Task.Factory.StartNew(() => this.RemoveRandom(collection));
            Parallel.Invoke(() => this.Add(collection), () => this.Iteration(collection));
            this._testStopwatch.Stop();
            this.IsRandomCanceled = true;
            task.Wait();
            this.CheckDataConsistency(collection);
            this.RemovedObjects.Clear();
            collection.Clear();

            this._testStopwatch.Start($"{currentMethodName} Iterative add  with parallel long read and remove");
            task = Task.Factory.StartNew(() => this.RemoveRandom(collection));
            Parallel.Invoke(() => this.Add(collection), () => this.IterationLong(collection));
            this._testStopwatch.Stop();
            this.IsRandomCanceled = true;
            task.Wait();
            this.CheckDataConsistency(collection);
            this.RemovedObjects.Clear();
            collection.Clear();
        }

        public void Add(VirtualizingObservableCollectionSource<Item> collection) { for (var i = 0; i < Iterations; i++) { collection.Add(_data[i]); } }

        public void Iteration(VirtualizingObservableCollectionSource<Item> collection)
        {
            var j = 0;
            foreach (var item in collection)
            {
                j++;
                if (j > 5) { break; }
            }
        }

        public void IterationLong(VirtualizingObservableCollectionSource<Item> collection)
        {
            var j = 0;
            foreach (var item in collection)
            {
                j++;
                if (j > 1000) { break; }
            }
        }

        private void RemoveRandom(VirtualizingObservableCollectionSource<Item> collection)
        {
            while (!this.IsRandomCanceled)
            {
                Thread.Sleep(10);
                try
                {
                    var removed = collection[1] as Item;
                    this.RemovedObjects.Add(removed);
                    collection.Remove(removed);
                }
                catch (ArgumentOutOfRangeException) { }
            }
        }
    }
}