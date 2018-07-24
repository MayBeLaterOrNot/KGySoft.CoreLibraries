﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RandomExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2018 - All Rights Reserved
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
using System.Collections.Generic;
using System.Linq;

using KGySoft.Libraries.Resources;

#endregion

namespace KGySoft.Libraries
{
    /// <summary>
    /// Contains extension methods for the <see cref="Random"/> type.
    /// </summary>
    public static class RandomExtensions
    {
        #region Methods

        #region Boolean

        /// <summary>
        /// Returns a random <see cref="bool"/> value.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>A <see cref="bool"/> value that is either <c>true</c> or <c>false</c>.</returns>
        public static bool NextBoolean(this Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            return (random.Next() & 1) == 0;
        }

        #endregion

        #region Integers

        /// <summary>
        /// Returns a random <see cref="sbyte"/> value.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>An 8-bit signed integer that is greater than or equal to <see cref="sbyte.MinValue">SByte.MinValue</see> and less or equal to <see cref="sbyte.MaxValue">SByte.MaxValue</see>.</returns>
        public static sbyte NextSByte(this Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            byte[] buf = new byte[1];
            random.NextBytes(buf);
            return (sbyte)buf[0];
        }

        /// <summary>
        /// Returns a random <see cref="sbyte"/> value that is less or equal to the specified maximum.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="maxValue">The upper bound of the random number returned.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>An 8-bit signed integer that is greater than or equal to 0 and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="maxValue"/> equals 0, then 0 is returned.</returns>
        public static sbyte NextSByte(this Random random, sbyte maxValue, bool inclusiveUpperBound = false)
            => (sbyte)random.NextInt64(0L, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="sbyte"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The upper bound of the random number returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>An 8-bit signed integer that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="maxValue"/> is returned.</returns>
        public static sbyte NextSByte(this Random random, sbyte minValue, sbyte maxValue, bool inclusiveUpperBound = false)
            => (sbyte)random.NextInt64(minValue, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="byte"/> value.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>An 8-bit unsigned integer that is greater than or equal to 0 and less or equal to <see cref="byte.MaxValue">Byte.MaxValue</see>.</returns>
        public static byte NextByte(this Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            byte[] buf = new byte[1];
            random.NextBytes(buf);
            return buf[0];
        }

        /// <summary>
        /// Returns a random <see cref="byte"/> value that is less or equal to the specified maximum.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="maxValue">The upper bound of the random number returned.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>An 8-bit unsigned integer that is greater than or equal to 0 and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="maxValue"/> equals 0, then 0 is returned.</returns>
        public static byte NextByte(this Random random, byte maxValue, bool inclusiveUpperBound = false)
            => (byte)random.NextUInt64(0UL, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="byte"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The upper bound of the random number returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>An 8-bit unsigned integer that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="maxValue"/> is returned.</returns>
        public static byte NextByte(this Random random, byte minValue, byte maxValue, bool inclusiveUpperBound = false)
            => (byte)random.NextUInt64(minValue, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="short"/> value.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>A 16-bit signed integer that is greater than or equal to <see cref="short.MinValue">Int16.MinValue</see> and less or equal to <see cref="short.MaxValue">Int16.MaxValue</see>.</returns>
        /// <remarks>Similarly to the <see cref="Random.Next()">Random.Next()</see> method this one returns an <see cref="int"/> value; however, the result can be negative and
        /// the maximum possible value can be <see cref="int.MaxValue">Int32.MaxValue</see>.</remarks>
        public static short NextInt16(this Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            byte[] buf = new byte[2];
            random.NextBytes(buf);
            return BitConverter.ToInt16(buf, 0);
        }

        /// <summary>
        /// Returns a random <see cref="short"/> value that is less or equal to the specified maximum.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="maxValue">The upper bound of the random number returned.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 16-bit signed integer that is greater than or equal to 0 and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="maxValue"/> equals 0, then 0 is returned.</returns>
        public static short NextInt16(this Random random, short maxValue, bool inclusiveUpperBound = false)
            => (short)random.NextInt64(0L, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="short"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The upper bound of the random number returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 16-bit signed integer that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="maxValue"/> is returned.</returns>
        public static short NextInt16(this Random random, short minValue, short maxValue, bool inclusiveUpperBound = false)
            => (short)random.NextInt64(minValue, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="ushort"/> value.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>A 16-bit unsigned integer that is greater than or equal to 0 and less or equal to <see cref="ushort.MaxValue">UInt16.MaxValue</see>.</returns>
        public static ushort NextUInt16(this Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            byte[] buf = new byte[2];
            random.NextBytes(buf);
            return BitConverter.ToUInt16(buf, 0);
        }

        /// <summary>
        /// Returns a random <see cref="ushort"/> value that is less or equal to the specified maximum.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="maxValue">The upper bound of the random number returned.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 16-bit unsigned integer that is greater than or equal to 0 and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="maxValue"/> equals 0, then 0 is returned.</returns>
        public static ushort NextUInt16(this Random random, ushort maxValue, bool inclusiveUpperBound = false)
            => (ushort)random.NextUInt64(0UL, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="ushort"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The upper bound of the random number returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 16-bit unsigned integer that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="maxValue"/> is returned.</returns>
        public static ushort NextUInt16(this Random random, ushort minValue, ushort maxValue, bool inclusiveUpperBound = false)
            => (ushort)random.NextUInt64(minValue, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="int"/> value.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>A 32-bit signed integer that is greater than or equal to <see cref="int.MinValue">Int32.MinValue</see> and less or equal to <see cref="int.MaxValue">Int32.MaxValue</see>.</returns>
        /// <remarks>Similarly to the <see cref="Random.Next()">Random.Next()</see> method this one returns an <see cref="int"/> value; however, the result can be negative and
        /// the maximum possible value can be <see cref="int.MaxValue">Int32.MaxValue</see>.</remarks>
        public static int NextInt32(this Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            byte[] buf = new byte[4];
            random.NextBytes(buf);
            return BitConverter.ToInt32(buf, 0);
        }

        /// <summary>
        /// Returns a random <see cref="int"/> value that is less or equal to the specified maximum.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="maxValue">The upper bound of the random number returned.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="maxValue"/> equals 0, then 0 is returned.</returns>
        public static int NextInt32(this Random random, int maxValue, bool inclusiveUpperBound = false)
            => (int)random.NextInt64(0L, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="int"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The upper bound of the random number returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 64-bit signed integer that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="maxValue"/> is returned.</returns>
        public static int NextInt32(this Random random, int minValue, int maxValue, bool inclusiveUpperBound = false)
            => (int)random.NextInt64(minValue, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="uint"/> value.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to 0 and less or equal to <see cref="uint.MaxValue">UInt32.MaxValue</see>.</returns>
        public static uint NextUInt32(this Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            byte[] buf = new byte[4];
            random.NextBytes(buf);
            return BitConverter.ToUInt32(buf, 0);
        }

        /// <summary>
        /// Returns a random <see cref="uint"/> value that is less or equal to the specified maximum.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="maxValue">The upper bound of the random number returned.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to 0 and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="maxValue"/> equals 0, then 0 is returned.</returns>
        public static uint NextUInt32(this Random random, uint maxValue, bool inclusiveUpperBound = false)
            => (uint)random.NextUInt64(0UL, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="uint"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The upper bound of the random number returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="maxValue"/> is returned.</returns>
        public static uint NextUInt32(this Random random, uint minValue, uint maxValue, bool inclusiveUpperBound = false)
            => (uint)random.NextUInt64(minValue, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="long"/> value.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to <see cref="long.MinValue">Int64.MinValue</see> and less or equal to <see cref="long.MaxValue">Int64.MaxValue</see>.</returns>
        public static long NextInt64(this Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }

        /// <summary>
        /// Returns a random <see cref="long"/> value that is less or equal to the specified maximum.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="maxValue">The upper bound of the random number returned.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="maxValue"/> equals 0, then 0 is returned.</returns>
        public static long NextInt64(this Random random, long maxValue, bool inclusiveUpperBound = false) 
            => random.NextInt64(0L, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="long"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The upper bound of the random number returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 64-bit signed integer that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="maxValue"/> is returned.</returns>
        public static long NextInt64(this Random random, long minValue, long maxValue, bool inclusiveUpperBound = false)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            if (minValue == maxValue)
                return minValue;

            if (maxValue < minValue)
                throw new ArgumentOutOfRangeException(nameof(maxValue), Res.Get(Res.ArgumentOutOfRange));

            ulong range = (ulong)(maxValue - minValue);
            if (inclusiveUpperBound)
            {
                if (range == ulong.MaxValue)
                    return random.NextInt64();
                range++;
            }

            ulong limit = ulong.MaxValue - (ulong.MaxValue % range);
            ulong r;
            do
            {
                r = random.NextUInt64();
            }
            while (r > limit);
            return (long)((r % range) + (ulong)minValue);
        }

        /// <summary>
        /// Returns a random <see cref="ulong"/> value.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to 0 and less or equal to <see cref="ulong.MaxValue">UInt64.MaxValue</see>.</returns>
        public static ulong NextUInt64(this Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            return BitConverter.ToUInt64(buf, 0);
        }

        /// <summary>
        /// Returns a random <see cref="ulong"/> value that is less or equal to the specified maximum.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="maxValue">The upper bound of the random number returned.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to 0 and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="maxValue"/> equals 0, then 0 is returned.</returns>
        public static ulong NextUInt64(this Random random, ulong maxValue, bool inclusiveUpperBound = false)
            => random.NextUInt64(0UL, maxValue, inclusiveUpperBound);

        /// <summary>
        /// Returns a random <see cref="ulong"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The upper bound of the random number returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <param name="inclusiveUpperBound"><c>true</c> to allow that the generated value is equal to <paramref name="maxValue"/>; otherwise, <c>false</c>. This parameter is optional.
        /// <br/>Default value: <c>false</c>.</param>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.
        /// If <paramref name="inclusiveUpperBound"/> if <c>false</c>, then <paramref name="maxValue"/> is an exclusive upper bound; however, if <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="maxValue"/> is returned.</returns>
        public static ulong NextUInt64(this Random random, ulong minValue, ulong maxValue, bool inclusiveUpperBound = false)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            if (minValue == maxValue)
                return minValue;

            if (maxValue < minValue)
                throw new ArgumentOutOfRangeException(nameof(maxValue), Res.Get(Res.ArgumentOutOfRange));

            ulong range = maxValue - minValue;
            if (inclusiveUpperBound)
            {
                if (range == ulong.MaxValue)
                    return random.NextUInt64();
                range++;
            }

            ulong limit = ulong.MaxValue - (ulong.MaxValue % range);
            ulong r;
            do
            {
                r = random.NextUInt64();
            }
            while (r > limit);

            return (r % range) + minValue;
        }

        #endregion

        #region Floating-point types

        public static double NextDouble(this Random random, double max)
            => random.NextDouble(0d, max);

        /// <summary>
        /// Returns a random <see cref="double"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The lower bound of the random number returned.</param>
        /// <param name="maxValue">The upper bound of the random number returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <returns>A double-precision floating point number that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.</returns>
        /// <remarks>
        /// <para>In most cases return value is less than <paramref name="maxValue"/>. Return value can be equal to <paramref name="maxValue"/> in very edge cases such as
        /// when <paramref name="minValue"/> is equal to <paramref name="maxValue"/> or when integer parts of both values are beyond the precision of the <see cref="double"/> type.</para>
        /// </remarks>
        public static double NextDouble(this Random random, double minValue, double maxValue, RandomScale scale = RandomScale.Auto)
        {
            double AdjustValue(double value) => Double.IsNegativeInfinity(value) ? Double.MinValue : (Double.IsPositiveInfinity(value) ? Double.MaxValue : value);

            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));

            // both are the same infinity
            if (Double.IsPositiveInfinity(minValue) && Double.IsPositiveInfinity(maxValue)
                || Double.IsNegativeInfinity(minValue) && Double.IsNegativeInfinity(maxValue))
                throw new ArgumentOutOfRangeException(nameof(minValue), Res.Get(Res.ArgumentOutOfRange));

            double range = maxValue - minValue;
            if (maxValue < minValue || Double.IsNaN(range))
                throw new ArgumentOutOfRangeException(nameof(maxValue), Res.Get(Res.ArgumentOutOfRange));

            if (!Enum<RandomScale>.IsDefined(scale))
                throw new ArgumentOutOfRangeException(nameof(scale), Res.Get(Res.ArgumentOutOfRange));

            minValue = AdjustValue(minValue);
            maxValue = AdjustValue(maxValue);
            if (minValue.Equals(maxValue))
                return minValue;

            bool posAndNeg = minValue < 0d && maxValue > 0d;
            double minAbs = Math.Min(Math.Abs(minValue), Math.Abs(maxValue));
            double maxAbs = Math.Max(Math.Abs(minValue), Math.Abs(maxValue));

            // if linear scaling is forced...
            if (scale == RandomScale.ForceLinear
                // or we use auto scaling and maximum is UInt16 or when the order of magnitude is smaller than 4...
                || (scale == RandomScale.Auto && !Double.IsInfinity(range) && (maxAbs <= ushort.MaxValue || maxAbs < minAbs * 16))
                // or order of magnitude is smaller than 2 (even if log scale would be preferred)
                || (scale == RandomScale.PreferLogarithmic && !posAndNeg && maxAbs < minAbs * 4))
            {
                range = AdjustValue(range);
                return random.NextDouble() * range + minValue;
            }

            // Possible double exponents are -1022..1023 but we don't generate negative exponents for big ranges because
            // that would cause too many almost zero results, which are much smaller than the original NextDouble values.
            int minExponent = posAndNeg || minAbs.Equals(0d) ? 0 : (int)Math.Log(minAbs, 2d);
            int maxExponent = (int)Math.Ceiling(Math.Log(maxAbs, 2d));

            // We go only below zero exponent for truly small ranges.
            if (maxExponent < minExponent || (maxExponent < 0 && minExponent > maxExponent - 4))
                minExponent = maxExponent - 4;

            double result;
            do
            {
                // worst case: very imbalanced range eg. -0.1 .. ulong.maxvalue
                double mantissa = random.NextDouble();
                if (posAndNeg)
                    mantissa *= 2d;
                if (minValue < 0d)
                    mantissa -= 1d;

                result = mantissa * Math.Pow(2d, random.Next(minExponent, maxExponent + 1));
            } while (result < minValue || result > maxValue);

            return result;
        }

        #endregion

        #region Char/String

        /// <summary>
        /// Returns a random <see cref="char"/> value that is within a specified range.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random character returned.</param>
        /// <param name="maxValue">The inclusive upper bound of the random character returned. Must be greater or equal to <paramref name="minValue"/>.</param>
        /// <returns>A <see cref="char"/> value that is greater than or equal to <paramref name="minValue"/> and less or equal to <paramref name="maxValue"/>.</returns>
        public static char NextChar(this Random random, char minValue = Char.MinValue, char maxValue = Char.MaxValue)
            => (char)random.NextUInt64(minValue, maxValue, true);

        #endregion

        public static float NextFloat(this Random random)
            => (float)random.NextDouble();

        public static float NextFloat(this Random random, float max)
            => random.NextFloat(0f, max);

        public static float NextFloat(this Random random, float min, float max)
        {
            if (max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(max));
            }

            float range = max - min;
            if (float.IsInfinity(range) || range >= float.MaxValue)
            {
                float result;
                do
                {
                    byte[] buf = new byte[4];
                    random.NextBytes(buf);
                    result = BitConverter.ToSingle(buf, 0);
                }
                while (result < min || result > max || float.IsInfinity(result) || float.IsNaN(result));
                return result;
            }

            return ((float)random.NextDouble() * range) + min;
        }

        public static decimal NextDecimal(this Random random)
        {
            var result = 1m;
            while (result >= 1)
            {
                var a = random.Next();
                var b = random.Next();

                // The high bits of 0.9999999999999999999999999999m are 542101086.
                var c = random.Next(542101087);
                result = new decimal(a, b, c, false, 28);
            }

            return result;
        }

        public static decimal NextDecimal(this Random random, decimal max)
            => NextDecimal(random, decimal.Zero, max);

        public static decimal NextDecimal(this Random random, decimal min, decimal max)
        {
            var rand = NextDecimal(random);
            return (max * rand) + (min * (1 - rand));
        }

        /// <summary>
        /// Shuffles an enumerable <paramref name="collection"/> (randomizes its elements).
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> to shuffle its elements.</param>
        /// <param name="random">The <see cref="Random"/> instance to use.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> which contains the elements of the <paramref name="collection"/> in randomized order.</returns>
        public static IEnumerable<T> Shuffle<T>(this Random random, IEnumerable<T> collection)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Res.Get(Res.ArgumentNull));
            if (collection == null)
                throw new ArgumentNullException(nameof(collection), Res.Get(Res.ArgumentNull));

            return collection.Select(item => new { Index = random.Next(), Value = item }).OrderBy(i => i.Index).Select(i => i.Value);
        }

        #endregion
    }
}
