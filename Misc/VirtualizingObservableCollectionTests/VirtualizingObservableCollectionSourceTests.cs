using System;
using AlphaChiTech.Virtualization;
using NUnit.Framework;

namespace VirtualizingObservableCollectionTests
{
    [TestFixture()]
    public class VirtualizingObservableCollectionSourceTests
    {


        [Test()]
        public void OnResetTest()
        {
            var i0 = new Item(0);
            var i1 = new Item(1);
            var voc = new VirtualizingObservableCollectionSource<Item>
            {
                i0,i1
            };
            Assert.IsTrue(voc.Count == 2);
            voc.OnReset(0);
            Assert.IsTrue(voc.Count == 0);
        }

        [Test()]
        public void GetAtTest()
        {
            var i0 = new Item(0);
            var i1 = new Item(1);
            var voc = new VirtualizingObservableCollectionSource<Item>
            {
                i0,i1
            };
            Assert.IsTrue(voc.Count == 2);
            var r0 = voc.GetAt(0,voc,false);
            var r1 = voc.GetAt(1,voc,false);
            Assert.IsTrue(i0==r0);
            Assert.IsTrue(i1==r1);
        }

        [Test()]
        public void GetCountTest()
        {
            var i0 = new Item(0);
            var i1 = new Item(1);
            var voc = new VirtualizingObservableCollectionSource<Item>();
            Assert.IsTrue(voc.Count == 0);
            voc.Add(i0);
            Assert.IsTrue(voc.Count == 1);
            voc.Add(i1);
            Assert.IsTrue(voc.Count == 2);
        }

        [Test()]
        public void OnAppendTest()
        {
            var i0 = new Item(0);
            var i1 = new Item(1);
            var voc = new VirtualizingObservableCollectionSource<Item>();
            var r0 = voc.OnAppend(i0,DateTime.Now);
            Assert.IsTrue(r0 == 0);
            var r1 = voc.OnAppend(i1, DateTime.Now);
            Assert.IsTrue(r1 == 1);
        }

        [Test()]
        public void OnInsertTest()
        {
            var i0 = new Item(0);
            var i1 = new Item(1);
            var i2 = new Item(2);
            var voc = new VirtualizingObservableCollectionSource<Item>() {i0,i1};
             voc.OnInsert(0,i2,DateTime.Now);

            Assert.IsTrue(voc.GetAt(0, voc, false) == i2);
            Assert.IsTrue(voc.GetAt(1, voc, false) == i0);
            Assert.IsTrue(voc.GetAt(2, voc, false) == i1);
        }

        [Test()]
        public void OnRemoveTest()
        {
            var i0 = new Item(0);
            var i1 = new Item(1);
            var voc = new VirtualizingObservableCollectionSource<Item>
            {
                i0,i1
            };
            Assert.IsTrue(voc.Count == 2);
            voc.OnRemove(0,i0,DateTime.Now);
            Assert.IsTrue(voc.Count == 1);
        }

        [Test()]
        public void OnReplaceTest()
        {
            var i0 = new Item(0);
            var i1 = new Item(1);
            var i2 = new Item(2);
            var voc = new VirtualizingObservableCollectionSource<Item>() { i0, i1 };
            
            Assert.IsTrue(voc.GetAt(0, voc, false) == i0);
            Assert.IsTrue(voc.GetAt(1, voc, false) == i1);

            voc.OnReplace(0,i0,i2,DateTime.Now);
            Assert.IsTrue(voc.GetAt(0, voc, false) == i2);
        }

        public class Item
        {
            public int I { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public Item(int i) { this.I = i; }
        }
    }
}