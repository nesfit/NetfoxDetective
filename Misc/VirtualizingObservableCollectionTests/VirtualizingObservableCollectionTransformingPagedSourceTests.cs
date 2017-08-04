using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using AlphaChiTech.Virtualization.Pageing;
using NUnit.Framework;

namespace VirtualizingObservableCollectionTests
{
    [TestFixture()]
    public class VirtualizingObservableCollectionTransformingPagedSourceTests : ConcurrencyTestsBase
    {
        [Test]
        [Explicit]
        [NUnit.Framework.Category("Explicit")]
        public void VirtualizingObservableCollectionTransformingPagedSourceConcurrencyTest()
        {
            var currentMethodName = this.GetCurrentMethodName();
            var sourceCollection = new VirtualizingObservableCollection<Item>(new VirtualizingObservableCollectionSource<Item>());
            var collectionSource = new VirtualizingObservableCollectionTransformingPagedSource<Item, Item>(sourceCollection, sItem => new Item(sItem.I), sItem => new Item(sItem.I));
            var collection = new VirtualizingObservableCollection<Item>(collectionSource);

            collection.CollectionChanged += this.collection_CollectionChanged;
            collection.PropertyChanged += this.collection_PropertyChanged;

            this._testStopwatch.Start($"{currentMethodName} iterative add with notifications");
            for (var i = 0; i < Iterations; i++) { sourceCollection.Add(_data[i]); }
            this._testStopwatch.Stop();
            //this.CheckDataConsistency(collection);
            this.CheckDataConsistency(sourceCollection);
            Assert.AreEqual(Iterations, this.RemovedObjects.Count - collection.Count());


            sourceCollection.Clear();

            this._testStopwatch.Start($"{currentMethodName} parallel add with notifications");
            Parallel.ForEach(_data, item => { sourceCollection.Add(item); });
            this._testStopwatch.Stop();
            //this.CheckDataConsistency(collection);
            this.CheckDataConsistency(sourceCollection);
            Assert.AreEqual(Iterations, this.RemovedObjects.Count - collection.Count());
            sourceCollection.Clear();


            collection.Clear();
            this._testStopwatch.Start($"{currentMethodName} parallel add int without interlocked");
            Parallel.ForEach(_data, item => { sourceCollection.Add(item); });
            this._testStopwatch.Stop();
            //this.CheckDataConsistency(collection);
            this.CheckDataConsistency(sourceCollection);
            Assert.AreEqual(Iterations, this.RemovedObjects.Count - collection.Count());
            sourceCollection.Clear();

            this._testStopwatch.Start($"{currentMethodName} Iterative add  with parallel read");
            Parallel.Invoke(() => this.Add(sourceCollection), () => this.Iteration(collection));
            this._testStopwatch.Stop();
            //this.CheckDataConsistency(collection);
            this.CheckDataConsistency(sourceCollection);
            Assert.AreEqual(Iterations, this.RemovedObjects.Count - collection.Count());

            sourceCollection.Clear();

            this._testStopwatch.Start($"{currentMethodName} Iterative add  with parallel read and remove");
            var task = Task.Factory.StartNew(() => this.RemoveRandom(sourceCollection));
            Parallel.Invoke(() => this.Add(sourceCollection), () => this.Iteration(collection));
            this._testStopwatch.Stop();
            this.IsRandomCanceled = true;
            task.Wait();
            //this.CheckDataConsistency(collection);
            this.CheckDataConsistency(sourceCollection);
            Assert.AreEqual(Iterations, this.RemovedObjects.Count - collection.Count());
            this.RemovedObjects.Clear();

            sourceCollection.Clear();

            this._testStopwatch.Start($"{currentMethodName} Iterative add  with parallel long read and remove");
            task = Task.Factory.StartNew(() => this.RemoveRandom(sourceCollection));
            Parallel.Invoke(() => this.Add(sourceCollection), () => this.IterationLong(collection));
            this._testStopwatch.Stop();
            this.IsRandomCanceled = true;
            task.Wait();
            //this.CheckDataConsistency(collection);
            this.CheckDataConsistency(sourceCollection);
            Assert.AreEqual(Iterations, this.RemovedObjects.Count - collection.Count());
            this.RemovedObjects.Clear();

            sourceCollection.Clear();


        }
        [Test]
        public void TransformingConstructorTest()
        {
            var currentMethodName = this.GetCurrentMethodName();
            var sourceCollection = new VirtualizingObservableCollection<Item>(new VirtualizingObservableCollectionSource<Item>());
            var collectionSource = new VirtualizingObservableCollectionTransformingPagedSource<ItemTransformed, Item>(sourceCollection, sItem => new ItemTransformed(sItem), itemTransformed => itemTransformed.Item);
            var collection = new VirtualizingObservableCollection<ItemTransformed>(new PaginationManager<ItemTransformed>(collectionSource));

            collection.CollectionChanged += this.collection_CollectionChanged;
            collection.PropertyChanged += this.collection_PropertyChanged;

            var iterations = 3;

            this._testStopwatch.Start($"{currentMethodName} ");
            for (var i = 0; i < iterations; i++) { sourceCollection.Add(_data[i]); }
            this._testStopwatch.Stop();
            Assert.AreEqual(iterations, ItemTransformed.ObjectsConstructed);
        }

  
        public void Add(VirtualizingObservableCollection<Item> collection) { for (var i = 0; i < Iterations; i++) { collection.Add(_data[i]); } }

        public class ItemTransformed
        {
            public static int ObjectsConstructed { get; set; } = 0;
            public Item Item { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public ItemTransformed(Item i)
            {
                this.Item = i;
                ItemTransformed.ObjectsConstructed ++;
            }

            #region Overrides of Object
            /// <summary>
            /// Serves as the default hash function. 
            /// </summary>
            /// <returns>
            /// A hash code for the current object.
            /// </returns>
            public override int GetHashCode() { return this.Item.GetHashCode(); }

            #region Overrides of Object
            /// <summary>
            /// Determines whether the specified object is equal to the current object.
            /// </summary>
            /// <returns>
            /// true if the specified object  is equal to the current object; otherwise, false.
            /// </returns>
            /// <param name="obj">The object to compare with the current object. </param>
            public override bool Equals(object obj)
            {
                var objT = obj as ItemTransformed;
                return objT?.Item == this.Item;
            }
            #endregion

            #endregion
        }

        public void Iteration(VirtualizingObservableCollection<Item> collection)
        {
            var j = 0;
            foreach (var item in collection)
            {
                j++;
                if (j > 5) { break; }
            }
        }

        public void IterationLong(VirtualizingObservableCollection<Item> collection)
        {
            var j = 0;
            foreach (var item in collection)
            {
                j++;
                if (j > 1000) { break; }
            }
        }

        private void RemoveRandom(VirtualizingObservableCollection<Item> collection)
        {
            while (!this.IsRandomCanceled)
            {
                Task.Delay(10);
                try
                {
                    if(collection.Count < 1) continue;
                    var removed = collection[1] as Item;
                    this.RemovedObjects.Add(removed);
                    collection.Remove(removed);
                }
                catch(ArgumentOutOfRangeException)
                {
                    Debugger.Break();
                }
            }
        }


        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var a = e.Action;
            var b = a;
            for (var i = 0; i < 10; i++) { b = a; }
            a = b;
            if (e == null) { return; }
            if (e.NewItems == null) { return; }
            var testObject = e.NewItems[0] as TestObject;
            //if (testObject != null && testObject.Value % 1000000 == 0) { Console.WriteLine(testObject.Value); }
            //Console.WriteLine(testObject.Value);
        }

        private void collection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var a = e.PropertyName;
            var b = a;
            for (var i = 0; i < 10; i++) { b = a; }
            a = b;
        }
    }
}