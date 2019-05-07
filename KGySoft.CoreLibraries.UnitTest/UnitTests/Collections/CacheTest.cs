﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CacheTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Linq;

using KGySoft.Collections;

using NUnit.Framework;

#endregion

namespace KGySoft.CoreLibraries.UnitTests.Collections
{
    [TestFixture]
    public class CacheTest
    {
        #region Methods

        [Test]
        public void SimpleUsage()
        {
            var cache = new Cache<string, string>(s => s.ToUpperInvariant());
            Assert.AreEqual("ALPHA", cache["alpha"]);
        }

        [Test]
        public void CacheFullDropOldest()
        {
            var cache = new Cache<string, string>(s => s.ToUpperInvariant(), 2) { Behavior = CacheBehavior.RemoveOldestElement };
            Console.WriteLine(cache["alpha"]);
            Console.WriteLine(cache["beta"]);
            Console.WriteLine(cache["alpha"]);
            Console.WriteLine(cache["gamma"]);

            Assert.IsFalse(cache.ContainsKey("alpha")); // alpha was the oldest
            Assert.AreEqual(2, cache.Count);
            Assert.AreEqual(2, cache.Count());
            Assert.AreEqual(2, cache.Keys.Count());
            Assert.AreEqual(2, cache.Values.Count());

            // reloading gamma
            Console.WriteLine(cache.GetValueUncached("gamma"));
        }

        [Test]
        public void CacheFullDropLeastRecentUsed()
        {
            var cache = new Cache<string, string>(s => s.ToUpperInvariant(), 2) { Behavior = CacheBehavior.RemoveLeastRecentUsedElement };
            Console.WriteLine(cache["alpha"]);
            Console.WriteLine(cache["beta"]);
            Console.WriteLine(cache["alpha"]);
            Console.WriteLine(cache["gamma"]);

            Assert.IsFalse(cache.ContainsKey("beta")); // beta was the least recent used

            Assert.AreEqual(2, cache.Count);
            Assert.AreEqual(2, cache.Count());
            Assert.AreEqual(2, cache.Keys.Count());
            Assert.AreEqual(2, cache.Values.Count());
        }

        [Test]
        public void RemoveTest()
        {
            var cache = new Cache<string, string>(s => s.ToUpperInvariant());
            Console.WriteLine(cache["alpha"]);
            Console.WriteLine(cache["beta"]);
            Console.WriteLine(cache["gamma"]);
            Console.WriteLine(cache["delta"]);
            Console.WriteLine(cache["epsilon"]);

            // remove middle
            cache.Remove("gamma");
            Assert.AreEqual(4, cache.Count);
            Assert.AreEqual(4, cache.Count());
            Assert.AreEqual(4, cache.Keys.Count());
            Assert.AreEqual(4, cache.Values.Count());

            // remove first
            cache.Remove("alpha");
            Assert.AreEqual(3, cache.Count);
            Assert.AreEqual(3, cache.Count());
            Assert.AreEqual(3, cache.Keys.Count());
            Assert.AreEqual(3, cache.Values.Count());

            // remove last
            cache.Remove("epsilon");
            Assert.AreEqual(2, cache.Count);
            Assert.AreEqual(2, cache.Count());
            Assert.AreEqual(2, cache.Keys.Count());
            Assert.AreEqual(2, cache.Values.Count());

            // remove first, when there are 2 elements
            cache.Remove("beta");
            Assert.AreEqual(1, cache.Count);
            Assert.AreEqual(1, cache.Count());
            Assert.AreEqual(1, cache.Keys.Count());
            Assert.AreEqual(1, cache.Values.Count());
        }

        [Test]
        public void TouchTest()
        {
            var cache = new Cache<string, string>(s => s.ToUpperInvariant()) { Behavior = CacheBehavior.RemoveOldestElement };
            Console.WriteLine(cache["alpha"]);
            Console.WriteLine(cache["beta"]);
            Console.WriteLine(cache["gamma"]);
            Console.WriteLine(cache["delta"]);
            Console.WriteLine(cache["epsilon"]);

            // touch middle
            cache.Touch("gamma");
            Assert.AreEqual(5, cache.Count);
            Assert.AreEqual(5, cache.Count());
            Assert.AreEqual(5, cache.Keys.Count());
            Assert.AreEqual(5, cache.Values.Count());
            Assert.AreEqual("alpha", cache.First().Key);
            Assert.AreEqual("gamma", cache.Last().Key);

            // touch first
            cache.Touch("alpha");
            Assert.AreEqual(5, cache.Count);
            Assert.AreEqual(5, cache.Count());
            Assert.AreEqual(5, cache.Keys.Count());
            Assert.AreEqual(5, cache.Values.Count());
            Assert.AreEqual("beta", cache.First().Key);
            Assert.AreEqual("alpha", cache.Last().Key);

            // touch last
            cache.Touch("alpha");
            Assert.AreEqual(5, cache.Count);
            Assert.AreEqual(5, cache.Count());
            Assert.AreEqual(5, cache.Keys.Count());
            Assert.AreEqual(5, cache.Values.Count());
            Assert.AreEqual("beta", cache.First().Key);
            Assert.AreEqual("alpha", cache.Last().Key);

            cache = new Cache<string, string>(s => s.ToUpperInvariant()) { Behavior = CacheBehavior.RemoveLeastRecentUsedElement };
            Console.WriteLine(cache["alpha"]);
            Console.WriteLine(cache["beta"]);

            // touch first, when there are 2 elements
            cache.Touch("alpha");
            Assert.AreEqual(2, cache.Count);
            Assert.AreEqual(2, cache.Count());
            Assert.AreEqual(2, cache.Keys.Count());
            Assert.AreEqual(2, cache.Values.Count());
            Assert.AreEqual("beta", cache.First().Key);
            Assert.AreEqual("alpha", cache.Last().Key);
        }

        [Test]
        public void KeysValuesTest()
        {
            var cache = new Cache<string, string>(s => s.ToUpperInvariant());
            Console.WriteLine(cache["alpha"]);
            Console.WriteLine(cache["beta"]);
            Console.WriteLine(cache["gamma"]);
            Console.WriteLine(cache["delta"]);
            Console.WriteLine(cache["epsilon"]);

            var keys = cache.Select(c => c.Key);
            Assert.IsTrue(keys.SequenceEqual(cache.Keys));

            var values = cache.Select(c => c.Value);
            Assert.IsTrue(values.SequenceEqual(cache.Values));
        }

        #endregion
    }
}
