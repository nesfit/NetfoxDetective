using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using NUnit.Framework;

namespace VirtualizingObservableCollectionTests
{

        [TestFixture]
        public class ConcurrentIObservableVirtualizingObservableCollectionPerformanceTest
    {
            private const int Iterations = 10000000;
            private readonly TestStopwatch _testStopwatch = new TestStopwatch();
            private bool IsRandomCanceled;
            private List<TestObject> RemovedObjects = new List<TestObject>();
            public void Add(ConcurrentIObservableVirtualizingObservableCollection<TestObject> collection) { for (var i = 0; i < Iterations; i++) { collection.Add(_data[i]); } }
     
            [Test]
            public void AsyncVirtualizingObservableCollection()
            {
                var collection = new ConcurrentIObservableVirtualizingObservableCollection<TestObject>();
                collection.CollectionChanged += this.collection_CollectionChanged;
                //collection.PropertyChanged += collection_PropertyChanged;
                this._testStopwatch.Start("AsyncVirtualizingObservableCollection iterative add with notifications");
                for (var i = 0; i < Iterations; i++) { collection.Add(_data[i]); }
                this._testStopwatch.Stop();
                this.CheckDataConsistency(collection);
                collection.Clear();

                this._testStopwatch.Start("AsyncVirtualizingObservableCollection iterative add without notifications");
                collection.SuspendCollectionChangeNotification();
                for (var i = 0; i < Iterations; i++) { collection.Add(_data[i]); }
                collection.ResumeCollectionChangeNotification();
                this._testStopwatch.Stop();
                this.CheckDataConsistency(collection);
                collection.Clear();

                this._testStopwatch.Start("AsyncVirtualizingObservableCollection bulk add");
                collection.AddRange(_data);
                this._testStopwatch.Stop();
                collection.Clear();

                this._testStopwatch.Start("AsyncVirtualizingObservableCollection parallel add with notifications");
                Parallel.ForEach(_data, item => { collection.Add(item); });
                this._testStopwatch.Stop();
                this.CheckDataConsistency(collection);
                collection.Clear();

                this._testStopwatch.Start("AsyncVirtualizingObservableCollection parallel add without notifications");
                collection.SuspendCollectionChangeNotification();
                Parallel.ForEach(_data, item => { collection.Add(item); });
                collection.ResumeCollectionChangeNotification();
                this._testStopwatch.Stop();
                this.CheckDataConsistency(collection);
                collection.Clear();

                collection.Clear();
                this._testStopwatch.Start("AsyncVirtualizingObservableCollection parallel add int without interlocked");
                Parallel.ForEach(_data, item => { collection.Add(item); });
                this._testStopwatch.Stop();
                this.CheckDataConsistency(collection);
                collection.Clear();

                this._testStopwatch.Start("AsyncVirtualizingObservableCollection Iterative add  with parallel read");
                Parallel.Invoke(() => this.Add(collection), () => this.Iteration(collection));
                this.CheckDataConsistency(collection);
                this._testStopwatch.Stop();
                this.CheckDataConsistency(collection);
                collection.Clear();

                this._testStopwatch.Start("AsyncVirtualizingObservableCollection Iterative add  with parallel read and remove");
                var task = Task.Factory.StartNew(() => this.RemoveRandom(collection));
                Parallel.Invoke(() => this.Add(collection), () => this.Iteration(collection));
                this._testStopwatch.Stop();
                this.IsRandomCanceled = true;
                task.Wait();
                this.CheckDataConsistency(collection);
                this.RemovedObjects.Clear();
                collection.Clear();

                this._testStopwatch.Start("AsyncVirtualizingObservableCollection Iterative add  with parallel long read and remove");
                task = Task.Factory.StartNew(() => this.RemoveRandom(collection));
                Parallel.Invoke(() => this.Add(collection), () => this.IterationLong(collection));
                this._testStopwatch.Stop();
                this.IsRandomCanceled = true;
                task.Wait();
                this.CheckDataConsistency(collection);
                this.RemovedObjects.Clear();
                collection.Clear();
            }

      

            public void Iteration(ConcurrentIObservableVirtualizingObservableCollection<TestObject> collection)
            {
                var j = 0;
                foreach (var item in collection)
                {
                    j++;
                    if (j > 5) { break; }
                }
            }

            public void IterationLong(ConcurrentIObservableVirtualizingObservableCollection<TestObject> collection)
            {
                var j = 0;
                foreach (var item in collection)
                {
                    j++;
                    if (j > 1000) { break; }
                }
            }
     

            private void CheckDataConsistency(IEnumerable<TestObject> collection)
            {
                var refe = _data.ToArray();
                foreach (var item in refe) { refe[item.Value] = null; }
                var errors = 0;
                foreach (var item in refe) { if (item != null) { errors++; } }
                var removedCount = this.RemovedObjects.Count;

                Console.WriteLine("Collection errors: {0}, removed errors {1}, removed {2}", errors, Iterations - removedCount - collection.Count(), removedCount);
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

            private static TestObject[] PrepareData()
            {
                var bulkAddList = new List<TestObject>();
                for (var i = 0; i < Iterations; i++) { bulkAddList.Add(new TestObject(i)); }
                return bulkAddList.ToArray();
            }

     
            private void RemoveRandom(ConcurrentIObservableVirtualizingObservableCollection<TestObject> collection)
            {
                while (!this.IsRandomCanceled)
                {
                    Thread.Sleep(10);
                    try
                    {
                        var removed = collection[1];
                        this.RemovedObjects.Add(removed);
                        collection.Remove(removed);
                    }
                    catch (ArgumentOutOfRangeException) { }
                }
            }
        


            private static TestObject[] _data = PrepareData();
        }

    public class TestObject : INotifyPropertyChanged, IComparable
    {
        public readonly int Value;
        public TestObject(int value)
        {
            this.Value = value;
            this.Guid = Guid.NewGuid();
        }

        public Guid Guid { get; }

        public int CompareTo(object obj)
        {
            if (!(obj is TestObject)) { return -1; }
            return (obj as TestObject).Value - this.Value;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null) { return false; }

            // Return true if the fields match:
            return (obj as TestObject).Value == this.Value;
        }

        public bool Equals(TestObject obj)
        {
            // If parameter is null return false:
            if (obj == null) { return false; }

            // Return true if the fields match:
            return obj.Value == this.Value;
        }

        public override int GetHashCode() { return this.Guid.GetHashCode(); }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }

    public class TestStopwatch
    {
        private Stopwatch _stopwatch = new Stopwatch();
        private string _testName;

        public void Start(string testName)
        {
            this._stopwatch.Reset();
            this._stopwatch.Start();
            this._testName = testName;
        }

        public void Stop()
        {
            this._stopwatch.Stop();
            Console.WriteLine("{0} = {1}", this._testName, this._stopwatch.Elapsed);
        }
    }
}

