﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EnumTest.cs
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

using NUnit.Framework;

#endregion

namespace KGySoft.CoreLibraries.UnitTests.CoreLibraries
{
    [TestFixture]
    public class EnumTest : TestBase
    {
        #region Enumerations

        [Flags]
        private enum TestLongEnum : long
        {
            None,
            Alpha = 1,
            Beta = 2,
            Gamma = 4,
            Delta = 8,

            AlphaRedefined = 1,
            Negative = -1,

            Alphabet = Alpha | Beta,

            Min = Int64.MinValue,
            Max = Int64.MaxValue,
        }

        private enum TestUlongEnum : ulong
        {
            Max = UInt64.MaxValue
        }

        [Flags]
        private enum TestIntEnum
        {
            None = 0,
            Simple = 1,
            Normal = 1 << 5,
            Risky = 1 << 31 // This is a negative value. Converting to Int64, this is not a single bit any more.
        }

        private enum EmptyEnum { }

        #endregion

        #region Methods

        [Test]
        public void GetNamesValuesTest()
        {
            Type enumType = typeof(TestLongEnum);
            Assert.IsTrue(Enum.GetNames(enumType).SequenceEqual(Enum<TestLongEnum>.GetNames()));
            Assert.IsTrue(Enum.GetValues(enumType).Cast<TestLongEnum>().SequenceEqual(Enum<TestLongEnum>.GetValues()));

            Assert.AreEqual(Enum.GetName(enumType, TestLongEnum.Alpha), Enum<TestLongEnum>.GetName(TestLongEnum.Alpha));
            Assert.AreEqual(Enum.GetName(enumType, TestLongEnum.AlphaRedefined), Enum<TestLongEnum>.GetName(TestLongEnum.AlphaRedefined));
            Assert.AreEqual(Enum.GetName(enumType, 1), Enum<TestLongEnum>.GetName(1));
            Assert.AreEqual(Enum.GetName(enumType, Int64.MinValue), Enum<TestLongEnum>.GetName(Int64.MinValue));

            enumType = typeof(TestIntEnum);
            Assert.AreEqual(Enum.GetName(enumType, TestIntEnum.Risky), Enum<TestIntEnum>.GetName(TestIntEnum.Risky));
            Assert.AreEqual(Enum.GetName(enumType, 1 << 31), Enum<TestIntEnum>.GetName(1 << 31));
        }

        [Test]
        public void IsDefinedTest()
        {
            Assert.IsTrue(Enum<TestLongEnum>.IsDefined(TestLongEnum.Gamma));
            Assert.IsFalse(Enum<TestLongEnum>.IsDefined(TestLongEnum.Gamma | TestLongEnum.Min));

            Assert.IsTrue(Enum<TestLongEnum>.IsDefined("Gamma"));
            Assert.IsFalse(Enum<TestLongEnum>.IsDefined("Omega"));

            Assert.IsTrue(Enum<TestLongEnum>.IsDefined((long)TestLongEnum.Max));
            Assert.IsTrue(Enum<TestLongEnum>.IsDefined((long)TestLongEnum.Min));
            Assert.IsTrue(Enum<TestLongEnum>.IsDefined((ulong)TestLongEnum.Max));
            Assert.IsTrue(Enum<TestLongEnum>.IsDefined(-1));
            Assert.IsFalse(Enum<TestLongEnum>.IsDefined(unchecked((ulong)(TestLongEnum.Min))));
            Assert.IsFalse(Enum<TestLongEnum>.IsDefined(UInt64.MaxValue));

            Assert.IsTrue(Enum<TestIntEnum>.IsDefined(TestIntEnum.Risky));
            Assert.IsTrue(Enum<TestIntEnum>.IsDefined("Risky"));
            Assert.IsTrue(Enum<TestIntEnum>.IsDefined(1 << 31)); // -2147483648
            Assert.IsFalse(Enum<TestIntEnum>.IsDefined(1U << 31)); // 2147483648
        }

        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual("Max", Enum<TestUlongEnum>.ToString(TestUlongEnum.Max));
            Assert.AreEqual("0", Enum<EmptyEnum>.ToString(default(EmptyEnum)));
            Assert.AreEqual("None", Enum<TestLongEnum>.ToString(default(TestLongEnum)));

            Assert.AreNotEqual("-10", Enum<TestUlongEnum>.ToString(unchecked((TestUlongEnum)(-10))));
            Assert.AreEqual("-10", Enum<TestLongEnum>.ToString((TestLongEnum)(-10)));
            Assert.AreEqual("-10", Enum<TestIntEnum>.ToString((TestIntEnum)(-10)));

            TestLongEnum e = TestLongEnum.Gamma | TestLongEnum.Alphabet;
            Assert.AreEqual("Alphabet, Gamma", e.ToString(EnumFormattingOptions.Auto));
            Assert.AreEqual("7", e.ToString(EnumFormattingOptions.NonFlags));
            Assert.AreEqual("Alpha, Beta, Gamma", e.ToString(EnumFormattingOptions.DistinctFlags));
            Assert.AreEqual("Alphabet, Gamma", e.ToString(EnumFormattingOptions.CompoundFlagsOrNumber));
            Assert.AreEqual("Alphabet, Gamma", e.ToString(EnumFormattingOptions.CompoundFlagsAndNumber));

            e += 16;
            Assert.AreEqual("23", e.ToString(EnumFormattingOptions.Auto));
            Assert.AreEqual("23", e.ToString(EnumFormattingOptions.NonFlags));
            Assert.AreEqual("Alpha, Beta, Gamma, 16", e.ToString(EnumFormattingOptions.DistinctFlags));
            Assert.AreEqual("23", e.ToString(EnumFormattingOptions.CompoundFlagsOrNumber));
            Assert.AreEqual("16, Alphabet, Gamma", e.ToString(EnumFormattingOptions.CompoundFlagsAndNumber));

            TestIntEnum ie = TestIntEnum.Simple | TestIntEnum.Normal | TestIntEnum.Risky;
            Assert.AreEqual("Simple, Normal, Risky", ie.ToString(EnumFormattingOptions.Auto));
        }

        [Test]
        public void EnumParseTest()
        {
            Assert.AreEqual(default(EmptyEnum), Enum<EmptyEnum>.Parse("0"));
            Assert.AreEqual(TestUlongEnum.Max, Enum<TestUlongEnum>.Parse("Max"));
            Assert.AreEqual(TestUlongEnum.Max, Enum<TestUlongEnum>.Parse(UInt64.MaxValue.ToString()));
            Assert.AreEqual(TestLongEnum.Min, Enum<TestLongEnum>.Parse("Min"));
            Assert.AreEqual(TestLongEnum.Min, Enum<TestLongEnum>.Parse(Int64.MinValue.ToString()));

            Assert.AreEqual(TestLongEnum.Alpha, Enum<TestLongEnum>.Parse("Alpha"));
            Assert.AreEqual(TestLongEnum.Alpha, Enum<TestLongEnum>.Parse("AlphaRedefined"));
            Assert.AreEqual(TestLongEnum.AlphaRedefined, Enum<TestLongEnum>.Parse("AlphaRedefined"));
            Assert.AreEqual(TestLongEnum.AlphaRedefined, Enum<TestLongEnum>.Parse("Alpha"));
            Assert.AreEqual(TestLongEnum.Alpha, Enum<TestLongEnum>.Parse("alpha", true));
            Assert.AreEqual(TestLongEnum.Alpha, Enum<TestLongEnum>.Parse("ALPHAREDEFINED", true));

            TestLongEnum e = TestLongEnum.Gamma | TestLongEnum.Alphabet;
            Assert.AreEqual(e, Enum<TestLongEnum>.Parse("Gamma, Alphabet"));
            Assert.AreEqual(e, Enum<TestLongEnum>.Parse("7"));
            Assert.AreEqual(e, Enum<TestLongEnum>.Parse("Alpha, Beta, Gamma"));
            Assert.AreEqual(e, Enum<TestLongEnum>.Parse("Alpha Beta Gamma", " "));
            Assert.AreEqual(e, Enum<TestLongEnum>.Parse("Alpha | Beta | Gamma", "|"));

            e += 16;
            Assert.AreEqual(e, Enum<TestLongEnum>.Parse("23"));
            Assert.AreEqual(e, Enum<TestLongEnum>.Parse("Alpha, Beta, Gamma, 16"));
            Assert.AreEqual(e, Enum<TestLongEnum>.Parse("16, Gamma, Alphabet"));

            Assert.IsFalse(Enum<TestLongEnum>.TryParse(UInt64.MaxValue.ToString(), out e));
            Assert.IsFalse(Enum<TestLongEnum>.TryParse("Beta, Gamma, , Delta, 16", out e));

            TestIntEnum ie = TestIntEnum.Simple | TestIntEnum.Normal | TestIntEnum.Risky;
            Assert.AreEqual(ie, Enum<TestIntEnum>.Parse(ie.ToString(EnumFormattingOptions.Auto)));
        }

        [Test]
        public void GetFlagsTest()
        {
            ulong max = UInt64.MaxValue;
            Assert.AreEqual(0, Enum<TestLongEnum>.GetFlags(TestLongEnum.None, true).Count());
            Assert.AreEqual(2, Enum<TestLongEnum>.GetFlags(TestLongEnum.Alphabet, true).Count());
            Assert.AreEqual(5, Enum<TestLongEnum>.GetFlags((TestLongEnum)max, true).Count());
            Assert.AreEqual(64, Enum<TestLongEnum>.GetFlags((TestLongEnum)max, false).Count());
            Assert.AreEqual(1, Enum<TestLongEnum>.GetFlags(TestLongEnum.Min, true).Count());
            Assert.AreEqual(0, Enum<TestUlongEnum>.GetFlags((TestUlongEnum)max, true).Count());
            Assert.AreEqual(64, Enum<TestUlongEnum>.GetFlags((TestUlongEnum)max, false).Count());

            Assert.AreEqual(1, Enum<TestIntEnum>.GetFlags(TestIntEnum.Risky, true).Count());
            Assert.AreEqual(3, Enum<TestIntEnum>.GetFlags(unchecked((TestIntEnum)(int)UInt32.MaxValue), true).Count());
            Assert.AreEqual(32, Enum<TestIntEnum>.GetFlags(unchecked((TestIntEnum)(int)UInt32.MaxValue), false).Count());

            AssertItemsEqual(new[] { TestLongEnum.Alpha, TestLongEnum.Beta, TestLongEnum.Gamma, TestLongEnum.Delta, TestLongEnum.Min }.OrderBy(e => e), Enum<TestLongEnum>.GetFlags().OrderBy(e => e));
            AssertItemsEqual(new TestUlongEnum[0], Enum<TestUlongEnum>.GetFlags());
            AssertItemsEqual(new[] { TestIntEnum.Simple, TestIntEnum.Normal, TestIntEnum.Risky }.OrderBy(e => e), Enum<TestIntEnum>.GetFlags().OrderBy(e => e));
            AssertItemsEqual(new EmptyEnum[0], Enum<EmptyEnum>.GetFlags());
        }

        [Test]
        public void AllFlagsDefinedTest()
        {
            Assert.IsTrue(Enum<TestLongEnum>.AllFlagsDefined(TestLongEnum.None));
            Assert.IsTrue(Enum<TestLongEnum>.AllFlagsDefined(TestLongEnum.Alphabet));
            Assert.IsFalse(Enum<TestLongEnum>.AllFlagsDefined(TestLongEnum.Max));
            Assert.IsTrue(Enum<TestLongEnum>.AllFlagsDefined(TestLongEnum.Min));

            Assert.IsTrue(Enum<TestIntEnum>.AllFlagsDefined(TestIntEnum.None)); // Zero is defined in TestIntEnum
            Assert.IsTrue(Enum<TestIntEnum>.AllFlagsDefined(TestIntEnum.Risky));
            Assert.IsTrue(Enum<TestIntEnum>.AllFlagsDefined(1 << 31)); // -2147483648: This is the value of Risky
            Assert.IsFalse(Enum<TestIntEnum>.AllFlagsDefined(1U << 31)); // 2147483648: This is not defined (cannot be represented in int)
            Assert.IsFalse(Enum<TestIntEnum>.AllFlagsDefined(1L << 31)); // 2147483648
            Assert.IsFalse(Enum<TestIntEnum>.AllFlagsDefined(1UL << 31)); // 2147483648

            Assert.IsFalse(Enum<TestUlongEnum>.AllFlagsDefined(0UL)); // Zero is not defined in TestUlongEnum
        }

        [Test]
        public void HasFlagTest()
        {
            TestLongEnum e64 = TestLongEnum.Alpha | TestLongEnum.Beta;
            Assert.IsTrue(Enum<TestLongEnum>.HasFlag(e64, TestLongEnum.None));
            Assert.IsTrue(Enum<TestLongEnum>.HasFlag(e64, TestLongEnum.Beta));
            Assert.IsFalse(Enum<TestLongEnum>.HasFlag(e64, TestLongEnum.Gamma));
            Assert.IsTrue(Enum<TestLongEnum>.HasFlag(e64, TestLongEnum.Alphabet));
            Assert.IsFalse(Enum<TestLongEnum>.HasFlag(e64, TestLongEnum.Alpha | TestLongEnum.Gamma));

            TestIntEnum e32 = TestIntEnum.Simple | TestIntEnum.Risky;
            Assert.IsTrue(Enum<TestIntEnum>.HasFlag(e32, TestIntEnum.None)); // Zero -> true
            Assert.IsFalse(Enum<TestIntEnum>.HasFlag(e32, TestIntEnum.Normal));
            Assert.IsTrue(Enum<TestIntEnum>.HasFlag(e32, TestIntEnum.Risky));
            Assert.IsTrue(Enum<TestIntEnum>.HasFlag(e32, (int)TestIntEnum.Risky));
            Assert.IsTrue(Enum<TestIntEnum>.HasFlag(e32, (long)TestIntEnum.Risky));
            Assert.IsTrue(Enum<TestIntEnum>.HasFlag(e32, 1 << 31)); // -2147483648: This is the value of Risky
            Assert.IsFalse(Enum<TestIntEnum>.HasFlag(e32, 1U << 31)); //  2147483648: This is not defined (cannot be represented in int)
            Assert.IsFalse(Enum<TestIntEnum>.HasFlag(e32, 1L << 31)); //  2147483648: This is not defined
            Assert.IsFalse(Enum<TestIntEnum>.HasFlag(e32, 1UL << 31)); //  2147483648: This is not defined

            TestUlongEnum eu64 = TestUlongEnum.Max;
            Assert.IsTrue(Enum<TestUlongEnum>.HasFlag(eu64, 0UL)); // Zero -> true
            Assert.IsTrue(Enum<TestUlongEnum>.HasFlag(eu64, TestUlongEnum.Max));
        }

        [Test]
        public void IsSingleFlagTest()
        {
            Assert.IsFalse(Enum<TestLongEnum>.IsSingleFlag(TestLongEnum.None));
            Assert.IsTrue(Enum<TestLongEnum>.IsSingleFlag(TestLongEnum.Delta));
            Assert.IsTrue(Enum<TestLongEnum>.IsSingleFlag(TestLongEnum.Beta));
            Assert.IsFalse(Enum<TestLongEnum>.IsSingleFlag(TestLongEnum.Alphabet));
            Assert.IsFalse(Enum<TestLongEnum>.IsSingleFlag(Int64.MaxValue));
            Assert.IsFalse(Enum<TestLongEnum>.IsSingleFlag(1 << 63)); // this is -2147483648, which is not a single bit as a long value
            Assert.IsTrue(Enum<TestLongEnum>.IsSingleFlag(1L << 63)); // this is a single bit negative value, which is valid
            Assert.IsFalse(Enum<TestLongEnum>.IsSingleFlag(1UL << 63)); // single bit but out of range

            Assert.IsFalse(Enum<TestIntEnum>.IsSingleFlag(1L << 63)); // out of range
            Assert.IsFalse(Enum<TestUlongEnum>.IsSingleFlag(1L << 63)); // this is a negative value: out of range
        }

        #endregion
    }
}
