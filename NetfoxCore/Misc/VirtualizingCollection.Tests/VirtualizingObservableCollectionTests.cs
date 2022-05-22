using System;
using System.Collections.Generic;
using System.Linq;
using AlphaChiTech.VirtualizingCollection;
using AlphaChiTech.VirtualizingCollection.Actions;
using NUnit.Framework;

namespace VirtualizingCollection.Tests
{
    [TestFixture]
    public class VirtualizingObservableCollectionTests
    {
        private const string ITEM = "data";
        private const string F_ITEM = "f_" + ITEM;
        private const string M_ITEM = "m_" + ITEM;
        private const string L_ITEM = "l_" + ITEM;
        private static readonly string[] ITEMS = {F_ITEM, M_ITEM, L_ITEM};

        [Test]
        public void AddTest()
        {
            var voc = new VirtualizingObservableCollection<string>(new TestListProvider<string>());
            var mirror = new MirrorCollection<string>(voc);
            voc.Add(ITEM);

            Assert.AreEqual(ITEM, voc[0]);
            Assert.AreEqual(1, voc.Count);
            CollectionAssert.AreEqual(new List<string> {ITEM}, mirror,
                "CollectionChanged event was not raised correctly!");
        }

        [Test]
        public void RemoveTest()
        {
            var voc = new VirtualizingObservableCollection<string>(
                new TestListProvider<string>(new List<string> {ITEM}));
            var mirror = new MirrorCollection<string>(voc);

            Assert.IsFalse(voc.Remove(null));
            Assert.IsTrue(voc.Remove(ITEM));
            Assert.AreEqual(0, voc.Count);
            CollectionAssert.AreEqual(Enumerable.Empty<string>(), mirror, "CollectionChanged event was not raised!");
        }

        [Test]
        public void AddRangeTest()
        {
            var voc = new VirtualizingObservableCollection<string>(new TestListProvider<string>());
            var mirror = new MirrorCollection<string>(voc);

            Assert.AreEqual(ITEMS.Length - 1, voc.AddRange(ITEMS));
            Assert.AreEqual(ITEMS.Length, voc.Count);
            CollectionAssert.AreEqual(ITEMS, mirror, "CollectionChanged event was not raised correctly!");
        }

        [Test]
        public void ClearTest()
        {
            var voc = new VirtualizingObservableCollection<string>(
                new TestListProvider<string>(new List<string>(ITEMS)));
            var mirror = new MirrorCollection<string>(voc);

            voc.Clear();
            Assert.AreEqual(0, voc.Count);
            CollectionAssert.AreEqual(Enumerable.Empty<string>(), mirror,
                "CollectionChanged event was not raised correctly!");
        }

        [Test]
        public void ResetTest()
        {
            var voc = new VirtualizingObservableCollection<string>(
                new TestListProvider<string>(new List<string>(ITEMS)));
            var mirror = new MirrorCollection<string>(voc);

            voc.Reset();
            Assert.AreEqual(0, voc.Count);
            CollectionAssert.AreEqual(Enumerable.Empty<string>(), mirror,
                "CollectionChanged event was not raised correctly!");
        }

        [Test]
        public void InsertTest()
        {
            var voc = new VirtualizingObservableCollection<string>(new TestListProvider<string>());
            var mirror = new MirrorCollection<string>(voc);

            Assert.IsFalse(voc.Contains(L_ITEM));
            voc.Insert(0, L_ITEM); // insert at the end of the list
            Assert.IsTrue(voc.Contains(L_ITEM));
            
            Assert.IsFalse(voc.Contains(F_ITEM));
            voc.Insert(0, F_ITEM); // insert at the beginning of the list
            Assert.IsTrue(voc.Contains(F_ITEM));
            
            Assert.IsFalse(voc.Contains(M_ITEM));
            voc.Insert(1, M_ITEM); // insert into the middle of the list
            Assert.IsTrue(voc.Contains(M_ITEM));
            
            Assert.AreEqual(3, voc.Count);
            CollectionAssert.AreEqual(ITEMS, mirror, "CollectionChanged event was not raised correctly!");
        }
        
        [Test]
        public void RemoveAtTest()
        {
            var voc = new VirtualizingObservableCollection<string>(new TestListProvider<string>(new List<string>(ITEMS)));
            var mirror = new MirrorCollection<string>(voc);

            Assert.IsTrue(voc.Contains(M_ITEM));
            voc.RemoveAt(1); // remove from the middle of the list
            Assert.IsFalse(voc.Contains(M_ITEM));

            Assert.IsTrue(voc.Contains(L_ITEM));
            voc.RemoveAt(1); // remove from the end of the list
            Assert.IsFalse(voc.Contains(L_ITEM));

            Assert.IsTrue(voc.Contains(F_ITEM));
            voc.RemoveAt(0); // remove from the beginning of the list
            Assert.IsFalse(voc.Contains(F_ITEM));

            Assert.AreEqual(0, voc.Count);
            CollectionAssert.AreEqual(Enumerable.Empty<string>(), mirror, "CollectionChanged event was not raised correctly!");
        }

        [Test]
        public void ReplaceAtTest()
        {
            var voc = new VirtualizingObservableCollection<string>(new TestListProvider<string>(new List<string>{F_ITEM}));
            var mirror = new MirrorCollection<string>(voc);
            
            new PlaceholderReplaceWA<string>(voc, F_ITEM, L_ITEM, 0).DoAction();
            
            Assert.AreEqual(1, voc.Count);
            CollectionAssert.AreEqual(new List<string>{L_ITEM}, mirror,"CollectionChanged event was not raised correctly!" );
        }

        [Test]
        public void IndexerGetTest()
        {
            var voc = new VirtualizingObservableCollection<string>(new TestListProvider<string>(new List<string>(ITEMS)));
            for (int i = 0; i < ITEMS.Length; i++)
                Assert.AreEqual(ITEMS[i], voc[i]);
            
            Assert.AreEqual(3, voc.Count);
        }
        
        [Test]
        public void IndexerSetTest()
        {
            var voc = new VirtualizingObservableCollection<string>(new TestListProvider<string>(new List<string>(ITEMS.Reverse())));
            var mirror = new MirrorCollection<string>(voc);

            for (int i = 0; i < ITEMS.Length; i++)
                voc[i] = ITEMS[i];
            
            Assert.AreEqual(3, voc.Count);
            CollectionAssert.AreEqual(ITEMS, mirror,"CollectionChanged event was not raised correctly!" );
        }
    }
}