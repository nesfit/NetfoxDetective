using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace VirtualizingObservableCollectionTests
{
    public class ConcurrencyTestsBase {
        protected readonly TestStopwatch _testStopwatch = new TestStopwatch();
        protected bool IsRandomCanceled;
        protected List<Item> RemovedObjects = new List<Item>();
        protected static Item[] _data = PrepareData();
        protected const int Iterations = 1000000;

        protected void CheckDataConsistency(IEnumerable<Item> collection)
        {
            var refe = _data.ToArray();
            foreach (var item in collection) { refe[item.I] = null; }
            foreach (var item in this.RemovedObjects) { refe[item.I] = null; }
            var errors = 0;
            foreach (var item in refe) { if (item != null) { errors++; } }
            var removedCount = this.RemovedObjects.Count;

            var removeErrors = Iterations - removedCount - collection.Count();
            Console.WriteLine("Collection errors: {0}, removed errors {1}, removed {2}", errors, removeErrors, removedCount);

            Assert.AreEqual(0,errors);
            Assert.AreEqual(0, removeErrors);
        }

        private static Item[] PrepareData()
        {
            var bulkAddList = new List<Item>();
            for (var i = 0; i < Iterations; i++) { bulkAddList.Add(new Item(i)); }
            return bulkAddList.ToArray();
        }

        public class Item
        {
            public int I { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public Item(int i) { this.I = i; }

            #region Overrides of Object
            /// <summary>
            /// Serves as the default hash function. 
            /// </summary>
            /// <returns>
            /// A hash code for the current object.
            /// </returns>
            public override int GetHashCode() { return this.I.GetHashCode(); }

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
                var objT = obj as Item;
                return objT?.I == this.I;
            }
            #endregion

            #endregion
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethodName()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }
    }
}