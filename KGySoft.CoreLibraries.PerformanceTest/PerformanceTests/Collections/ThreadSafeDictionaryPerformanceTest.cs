﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ThreadSafeDictionaryPerformanceTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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
#if !NET35
using System.Collections.Concurrent; 
#endif
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#if !(NET35 || NET40)
using System.Threading; 
#endif
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.Collections;

using NUnit.Framework;

#endregion

namespace KGySoft.CoreLibraries.PerformanceTests.Collections
{
    [TestFixture]
    public class ThreadSafeDictionaryPerformanceTest
    {
        #region Nested Classes

        private sealed class PoorHashTest
        {
            #region Properties

            private readonly int value;

            #endregion

            #region Constructors

            internal PoorHashTest(int i) => value = i;

            #endregion

            #region Methods

            public override int GetHashCode() => 0;

            public override bool Equals(object obj) => obj is PoorHashTest pht && pht.value == value;

            #endregion
        }

        #endregion

        #region Fields

        private static readonly TimeSpan infiniteTimeout =
#if NET35 || NET40
            TimeSpan.FromMilliseconds(Timeout.Infinite);
#else
            Timeout.InfiniteTimeSpan;
#endif

        #endregion

        #region Methods

        [Test]
        public void AddTest()
        {
            const int count = 10_000_000;
            var dict = new Dictionary<int, object>();
            var lDict = new LockingDictionary<int, object>();
#if !NET35
            var cDict = new ConcurrentDictionary<int, object>(dict);
#endif
            var tDict = new ThreadSafeDictionary<int, object>(dict, strategy: HashingStrategy.And);
            var gDict = new LockFreeCache<int, object>.GrowOnlyDictionary(count, null, true);

            new IteratorPerformanceTest { Iterations = count/*, Repeat = 5*/, WarmUp = false, TestName = "Sequential Add" }
                .AddCase(i => lDict.Add(i, null), "LockingDictionary")
#if !NET35
                .AddCase(i => cDict.TryAdd(i, null), "ConcurrentDictionary")
#endif
                .AddCase(i => tDict.Add(i, null), "ThreadSafeDictionary")
                .AddCase(i => gDict.TryAdd(i, null), "GrowOnlyDictionary")
                .DoTest()
                .DumpResults(Console.Out);
        }

#if !NET35
        [Test]
        public void AddTestParallel()
        {
            const int count = 10_000_000;
            var dict = new Dictionary<int, object>();
            var lDict = new LockingDictionary<int, object>();
            var cDict = new ConcurrentDictionary<int, object>(dict);
            var tDict = new ThreadSafeDictionary<int, object>(dict, strategy: HashingStrategy.And);
            var gDict = new LockFreeCache<int, object>.GrowOnlyDictionary(count, null, true);

            new PerformanceTest { Iterations = 1, CpuAffinity = null, TestName = "Parallel Add", WarmUp = false }
                .AddCase(() => Parallel.For(0, count, i => lDict.Add(i, i)), "LockingDictionary")
                .AddCase(() => Parallel.For(0, count, i => cDict.TryAdd(i, i)), "ConcurrentDictionary")
                .AddCase(() => Parallel.For(0, count, i => tDict.Add(i, i)), "ThreadSafeDictionary")
                .AddCase(() => Parallel.For(0, count, i => gDict.TryAdd(i, i)), "GrowOnlyDictionary")
                .DoTest()
                .DumpResults(Console.Out);
        }
#endif

        [Test]
        public void AccessWellDistributedKeysTest()
        {
            const int count = 1_000_000;
            var seq = Enumerable.Range(0, count);
            var dict = seq.ToDictionary(i => i, i => (object)i);
            var lDict = new LockingDictionary<int, object>(new Dictionary<int, object>(dict));
#if !NET35
            var cDict = new ConcurrentDictionary<int, object>(dict);
#endif
            var tDict = new ThreadSafeDictionary<int, object>(dict, strategy: HashingStrategy.And);
            var gDict = new LockFreeCache<int, object>.GrowOnlyDictionary(count, null, true);
            for (int i = 0; i < count; i++)
                gDict[i] = null;

            new IteratorPerformanceTest<object> { Iterations = count, Repeat = 5, TestName = "Sequential" }
                .AddCase(i => lDict[i], "LockingDictionary")
#if !NET35
                .AddCase(i => cDict[i], "ConcurrentDictionary")
#endif
                .AddCase(i => tDict[i], "ThreadSafeDictionary")
                .AddCase(i => gDict[i], "GrowOnlyDictionary")
                .DoTest()
                .DumpResults(Console.Out);

#if !NET35
            new PerformanceTest { Iterations = 1, Repeat = 5, CpuAffinity = null, TestName = "Parallel" }
                .AddCase(() => Parallel.For(0, count, i => { var _ = lDict[i]; }), "LockingDictionary")
                .AddCase(() => Parallel.For(0, count, i => { var _ = cDict[i]; }), "ConcurrentDictionary")
                .AddCase(() => Parallel.For(0, count, i => { var _ = tDict[i]; }), "ThreadSafeDictionary")
                .AddCase(() => Parallel.For(0, count, i => { var _ = gDict[i]; }), "GrowOnlyDictionary")
                .DoTest()
                .DumpResults(Console.Out);
#endif
        }

        [Test]
        public void AccessCollidingKeysTest()
        {
            const int count = 1000;
            const int iterations = 10_000;

            var seq = Enumerable.Range(0, count);
            var dict = seq.ToDictionary(i => new PoorHashTest(i), i => i);
            var lDict = new LockingDictionary<PoorHashTest, int>(new Dictionary<PoorHashTest, int>(dict));
#if !NET35
            var cDict = new ConcurrentDictionary<PoorHashTest, int>(dict);
#endif
            var tDict = new ThreadSafeDictionary<PoorHashTest, int>(dict, strategy: HashingStrategy.And);
            var gDict = new LockFreeCache<PoorHashTest, int>.GrowOnlyDictionary(count, null, true);
            foreach (var item in dict)
                gDict[item.Key] = item.Value;

            // some dictionaries insert colliding items at the first position, others at the last so making lookup equally hard all for them
            var key = new PoorHashTest(count / 2);
            new IteratorPerformanceTest<int> { Iterations = iterations, Repeat = 5, TestName = "Sequential" }
                .AddCase(i => lDict[key], "LockingDictionary")
#if !NET35
                .AddCase(i => cDict[key], "ConcurrentDictionary")
#endif
                .AddCase(i => tDict[key], "ThreadSafeDictionary")
                .AddCase(i => gDict[key], "GrowOnlyDictionary")
                .DoTest()
                .DumpResults(Console.Out);

#if !NET35
            new PerformanceTest { Iterations = 1, Repeat = 5, CpuAffinity = null, TestName = "Parallel" }
                .AddCase(() => Parallel.For(0, iterations, i => { var _ = lDict[key]; }), "LockingDictionary")
                .AddCase(() => Parallel.For(0, iterations, i => { var _ = cDict[key]; }), "ConcurrentDictionary")
                .AddCase(() => Parallel.For(0, iterations, i => { var _ = tDict[key]; }), "ThreadSafeDictionary")
                .AddCase(() => Parallel.For(0, iterations, i => { var _ = gDict[key]; }), "GrowOnlyDictionary")
                .DoTest()
                .DumpResults(Console.Out);
#endif
        }

        [Test]
        public void UpdateTest()
        {
            const int count = 1_000_000;
            var seq = Enumerable.Range(0, count);
            var dict = seq.ToDictionary(i => i, i => (object)null);
            var lDict = new LockingDictionary<int, object>(new Dictionary<int, object>(dict));
#if !NET35
            var cDict = new ConcurrentDictionary<int, object>(dict);
#endif
            var tDict = new ThreadSafeDictionary<int, object>(dict, strategy: HashingStrategy.And);

            new IteratorPerformanceTest { Iterations = count, Repeat = 5, TestName = "Sequential Update" }
                //.AddCase(i => dict[i] = null, "Dictionary")
                .AddCase(i => lDict[i] = null, "LockingDictionary")
#if !NET35
                .AddCase(i => cDict[i] = null, "ConcurrentDictionary")
#endif
                .AddCase(i => tDict[i] = null, "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);

#if !NET35
            new PerformanceTest { Iterations = 1, CpuAffinity = null, TestName = "Parallel Update", Repeat = 5 }
                .AddCase(() => Parallel.For(0, count, i => lDict[i] = i), "LockingDictionary")
                .AddCase(() => Parallel.For(0, count, i => cDict[i] = i), "ConcurrentDictionary")
                .AddCase(() => Parallel.For(0, count, i => tDict[i] = i), "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);
#endif
        }

        [Test]
        public void SetAndGetTest()
        {
            const int count = 1_000_000;
            var seq = Enumerable.Range(0, count);
            var dict = seq.ToDictionary(i => i, i => (object)null);
            var lDict = new LockingDictionary<int, object>(new Dictionary<int, object>(dict));
#if !NET35
            var cDict = new ConcurrentDictionary<int, object>(dict);
#endif
            var tDict = new ThreadSafeDictionary<int, object>(dict, strategy: HashingStrategy.And);

            new IteratorPerformanceTest<object> { Iterations = count, Repeat = 5, TestName = "Sequential Update" }
                .AddCase(i =>
                {
                    lDict[i] = null;
                    return lDict[i] = null;
                }, "LockingDictionary")
#if !NET35
                .AddCase(i =>
                {
                    cDict[i] = null;
                    return cDict[i];
                }, "ConcurrentDictionary")
#endif
                .AddCase(i =>
                {
                    tDict[i] = null;
                    return tDict[i];
                }, "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);

#if !NET35
            new PerformanceTest { Iterations = 1, CpuAffinity = null, TestName = "Parallel Update", Repeat = 5 }
                .AddCase(() => Parallel.For(0, count, i =>
                {
                    lDict[i] = i;
                    lDict.TryGetValue(i, out var _);
                }), "LockingDictionary")
                .AddCase(() => Parallel.For(0, count, i =>
                {
                    cDict[i] = i;
                    cDict.TryGetValue(i, out var _);
                }), "ConcurrentDictionary")
                .AddCase(() => Parallel.For(0, count, i =>
                {
                    tDict[i] = i;
                    tDict.TryGetValue(i, out var _);
                }), "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);
#endif
        }

        [Test]
        public void SetAndGetNonAtomicTest()
        {
            const int count = 1_000_000;
            var seq = Enumerable.Range(0, count);
            Dictionary<int, decimal> dict = seq.ToDictionary(i => i, i => (decimal)i);
            var lDict = new LockingDictionary<int, decimal>(new Dictionary<int, decimal>(dict));
#if !NET35
            var cDict = new ConcurrentDictionary<int, decimal>(dict);
#endif
            var tDict = new ThreadSafeDictionary<int, decimal>(dict);

            new IteratorPerformanceTest<object> { Iterations = count, Repeat = 5, TestName = "Sequential Access" }
                .AddCase(i =>
                {
                    lDict[i] = i;
                    return lDict[i] = i;
                }, "LockingDictionary")
#if !NET35
                .AddCase(i =>
                {
                    cDict[i] = i;
                    return cDict[i];
                }, "ConcurrentDictionary")
#endif
                .AddCase(i =>
                {
                    tDict[i] = i;
                    return tDict[i];
                }, "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);

#if !NET35
            new PerformanceTest { Iterations = 1, CpuAffinity = null, TestName = "Parallel Access", Repeat = 5 }
                .AddCase(() => Parallel.For(0, count, i =>
                {
                    lDict[i] = i;
                    lDict.TryGetValue(i, out var _);
                }), "LockingDictionary")
                .AddCase(() => Parallel.For(0, count, i =>
                {
                    cDict[i] = i;
                    cDict.TryGetValue(i, out var _);
                }), "ConcurrentDictionary")
                .AddCase(() => Parallel.For(0, count, i =>
                {
                    tDict[i] = i;
                    tDict.TryGetValue(i, out var _);
                }), "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);
#endif
        }

        [Test]
        public void CountTest()
        {
            const int count = 1_000;
            var seq = Enumerable.Range(0, count);
            var dict = seq.ToDictionary(i => i, i => (object)null);
            var lDict = new LockingDictionary<int, object>(new Dictionary<int, object>(dict));
#if !NET35
            var cDict = new ConcurrentDictionary<int, object>(dict);
#endif
            var tDict = new ThreadSafeDictionary<int, object>(dict, strategy: HashingStrategy.And);

            new PerformanceTest<int>{ Iterations = 1_000_000, Repeat = 5 }
                .AddCase(() => dict.Count, "Dictionary")
                .AddCase(() => lDict.Count, "LockingDictionary")
#if !NET35
                .AddCase(() => cDict.Count, "ConcurrentDictionary")
#endif
                .AddCase(() => tDict.Count, "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        [SuppressMessage("Performance", "CA1829:Use Length/Count property instead of Count() when available",
            Justification = "Intended for testing enumerators")]
        public void EnumerationTest()
        {
            const int count = 1_000;
            var seq = Enumerable.Range(0, count);
            var dict = seq.ToDictionary(i => i, i => (object)null);
            var lDict = new LockingDictionary<int, object>(new Dictionary<int, object>(dict));
#if !NET35
            var cDict = new ConcurrentDictionary<int, object>(dict);
#endif
            var tDict = new ThreadSafeDictionary<int, object>(dict, strategy: HashingStrategy.And);

            new PerformanceTest<int> { TestName = "Enumerating dictionary", Iterations = 100_000, Repeat = 5 }
                .AddCase(() => dict.Count(), "Dictionary")
                .AddCase(() => lDict.Count(), "LockingDictionary")
#if !NET35
                .AddCase(() => cDict.Count(), "ConcurrentDictionary")
#endif
                .AddCase(() => tDict.Count(), "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);

            new PerformanceTest<int> { TestName = "Enumerating Keys", Iterations = 100_000, Repeat = 5 }
                .AddCase(() => dict.Keys.Count(), "Dictionary")
                .AddCase(() => lDict.Keys.Count(), "LockingDictionary")
#if !NET35
                .AddCase(() => cDict.Keys.Count(), "ConcurrentDictionary")
#endif
                .AddCase(() => tDict.Keys.Count(), "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void ToArrayTest()
        {
            const int count = 1_000;
            var seq = Enumerable.Range(0, count);
            var dict = seq.ToDictionary(i => i, i => (object)null);
            var lDict = new LockingDictionary<int, object>(new Dictionary<int, object>(dict));
#if !NET35
            var cDict = new ConcurrentDictionary<int, object>(dict);
#endif
            var tDict = new ThreadSafeDictionary<int, object>(dict, strategy: HashingStrategy.And);

            new PerformanceTest<KeyValuePair<int, object>[]> { Iterations = 100_000, Repeat = 5 }
                .AddCase(() => dict.ToArray(), "Dictionary (as extension)")
                .AddCase(() => lDict.ToArray(), "LockingDictionary (as extension)")
#if !NET35
                .AddCase(() => cDict.ToArray(), "ConcurrentDictionary")
                .AddCase(() => ((IDictionary<int, object>)cDict).ToArray(), "ConcurrentDictionary (as extension)")
#endif
                .AddCase(() => tDict.ToArray(), "ThreadSafeDictionary")
                .AddCase(() => ((IDictionary<int, object>)tDict).ToArray(), "ThreadSafeDictionary (as extension)")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void AddOrUpdateTest()
        {
            static int Update(int key, int orig) => orig + 1;

            const int count = 1_000;
            var seq = Enumerable.Range(0, count);
            var dict = seq.ToDictionary(i => i, i => 0);
            var lDict = new LockingDictionary<int, int>(new Dictionary<int, int>(dict));
#if !NET35
            var cDict = new ConcurrentDictionary<int, int>(dict);
#endif
            var tDict = new ThreadSafeDictionary<int, int>(dict, strategy: HashingStrategy.And);

            new RandomizedPerformanceTest<int> { Iterations = 10_000_000, Repeat = 5 }
                .AddCase(rnd => dict.AddOrUpdate(rnd.Next(count), 1, Update), "Dictionary (as extension)")
                .AddCase(rnd => lDict.AddOrUpdate(rnd.Next(count), 1, Update), "LockingDictionary (as extension)")
#if !NET35
                .AddCase(rnd => cDict.AddOrUpdate(rnd.Next(count), 1, Update), "ConcurrentDictionary")
#endif
                .AddCase(rnd => tDict.AddOrUpdate(rnd.Next(count), 1, Update), "ThreadSafeDictionary")
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }
}
