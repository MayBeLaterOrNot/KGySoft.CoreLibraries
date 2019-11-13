﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EnumComparer.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if NETFRAMEWORK && !NET35
using System.Linq.Expressions;
#endif
using System.Runtime.Serialization;
using System.Security;
#if NETFRAMEWORK && !NET35
using System.Threading;
#endif

using KGySoft.Collections;

#endregion

namespace KGySoft.CoreLibraries
{
    /// <summary>
    /// Provides an efficient <see cref="IEqualityComparer{T}"/> and <see cref="IComparer{T}"/> implementation for <see cref="Enum"/> types.
    /// Can be used for example in <see cref="Dictionary{TKey,TValue}"/>, <see cref="SortedList{TKey,TValue}"/> or <see cref="Cache{TKey,TValue}"/> instances with <see langword="enum"/>&#160;key,
    /// or as a comparer for <see cref="List{T}.Sort(IComparer{T})"><![CDATA[List<T>.Sort(IComparer<T>)]]></see> method to sort <see langword="enum"/>&#160;elements.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumeration. Must be an <see cref="Enum"/> type.</typeparam>
    /// <remarks>
    /// Using dictionaries with <see langword="enum"/>&#160;key and finding elements in an <see langword="enum"/>&#160;array works without using <see cref="EnumComparer{TEnum}"/>, too.
    /// But unlike <see cref="int"/> or the other possible underlying types, <see langword="enum"/>&#160;types does not implement the generic <see cref="IEquatable{T}"/> and
    /// <see cref="IComparable{T}"/> interfaces. This causes that using an <see langword="enum"/>&#160;as key in a dictionary, for example, can be very ineffective (depends on the used framework, see the note below)
    /// due to heavy boxing and unboxing to and from <see cref="object"/> type. This comparer generates the type specific <see cref="IEqualityComparer{T}.Equals(T,T)"><![CDATA[IEqualityComparer<TEnum>.Equals]]></see>,
    /// <see cref="IEqualityComparer{T}.GetHashCode(T)"><![CDATA[IEqualityComparer<T>.GetHashCode]]></see> and <see cref="IComparer{T}.Compare"><![CDATA[IComparer<T>.Compare]]></see> methods for any <see langword="enum"/>&#160;type.
    /// <note>
    /// The optimization of <see cref="EqualityComparer{T}"/> and <see cref="Comparer{T}"/> instances for <see langword="enum"/>&#160;types may differ in different target frameworks.
    /// <list type="bullet">
    /// <item>In .NET Framework 3.5 and earlier versions they are not optimized at all.</item>
    /// <item>In .NET 4.0 Framework <see cref="EqualityComparer{T}"/> was optimized for <see cref="int"/>-based <see langword="enum"/>s. (Every .NET 4.0 assembly is executed on the latest 4.x runtime though, so this is might be relevant
    /// only on Windows XP where no newer than the 4.0 runtime can be installed.)</item>
    /// <item>In latest .NET 4.x Framework versions <see cref="EqualityComparer{T}"/> is optimized for any <see langword="enum"/>&#160;type but <see cref="Comparer{T}"/> is not.</item>
    /// <item>In .NET Core both <see cref="EqualityComparer{T}"/> and <see cref="Comparer{T}"/> are optimized for any <see langword="enum"/>&#160;types. In fact, <see cref="Compare"><![CDATA[EnumComparer<T>.Compare]]></see>
    /// and <see cref="Equals(TEnum,TEnum)"><![CDATA[EnumComparer<T>.Equals]]></see> are still slightly faster than <see cref="Comparer{T}.Compare"><![CDATA[Comparer<T>.Compare]]></see>
    /// and <see cref="EqualityComparer{T}.Equals(T,T)"><![CDATA[EqualityComparer<T>.Equals]]></see> methods, while <see cref="GetHashCode(TEnum)"><![CDATA[EnumComparer<T>.GetHashCode]]></see> is slightly slower than
    /// <see cref="EqualityComparer{T}.GetHashCode(T)"><![CDATA[EqualityComparer<T>.GetHashCode]]></see>. (A <see cref="Dictionary{TKey,TValue}"/> uses exactly one <see cref="IEqualityComparer{T}.GetHashCode(T)">GetHashCode</see> and
    /// at least one <see cref="IEqualityComparer{T}.Equals(T,T)">Equals</see> call for each lookup, for example).</item>
    /// </list>
    /// </note>
    /// <note>In .NET Standard 2.0 building dynamic assembly is not supported so the .NET Standard 2.0 version falls back to using <see cref="EqualityComparer{T}"/> and <see cref="Comparer{T}"/> classes.</note>
    /// </remarks>
    /// <example>
    /// Example for initializing of a <see cref="Dictionary{TKey,TValue}"/> with <see cref="EnumComparer{TEnum}"/>:
    /// <code lang="C#">
    /// <![CDATA[Dictionary<MyEnum, string> myDict = new Dictionary<MyEnum, string>(EnumComparer<MyEnum>.Comparer);]]>
    /// </code>
    /// </example>
    [Serializable]
    [SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "False alarm, SerializationUnityHolder will be deserialized.")]
    public abstract class EnumComparer<TEnum> : IEqualityComparer<TEnum>, IComparer<TEnum>, ISerializable
    {
        #region Nested Classes
        
        #region SerializationUnityHolder class

        /// <summary>
        /// This class is needed in order not to serialize the generated type.
        /// </summary>
        [Serializable]
        private sealed class SerializationUnityHolder : IObjectReference
        {
            #region Methods

            [SecurityCritical]
            public object GetRealObject(StreamingContext context) => Comparer;

            #endregion
        }

        #endregion

        #region FallbackEnumComparer
#if NETSTANDARD2_0

        [Serializable]
        private sealed class FallbackEnumComparer : EnumComparer<TEnum>
        {
            #region Methods
            
            public override bool Equals(TEnum x, TEnum y) => EqualityComparer<TEnum>.Default.Equals(x, y);
            public override int GetHashCode(TEnum obj) => EqualityComparer<TEnum>.Default.GetHashCode(obj);
            public override int Compare(TEnum x, TEnum y) => Comparer<TEnum>.Default.Compare(x, y); 
            
            #endregion
        }

#endif

        #endregion

        #region PartiallyTrustedEnumComparer class
#if NETFRAMEWORK && !NET35

        /// <summary>
        /// Similar to the DynamicEnumComparer generated by <see cref="EnumComparerBuilder"/> but can be used
        /// even from partially trusted domains. Not using this from .NET Standard 2.0 because the actual platform
        /// can be faster than this.
        /// </summary>
        [Serializable]
        private sealed class PartiallyTrustedEnumComparer : EnumComparer<TEnum>
        {
            #region Fields

            private static Func<TEnum, TEnum, bool> equals;
            private static Func<TEnum, int> getHashCode;
            private static Func<TEnum, TEnum, int> compare;

            #endregion

            #region Methods

            #region Static Methods

            /// <summary>
            /// return x == y
            /// </summary>
            private static Func<TEnum, TEnum, bool> GenerateEquals()
            {
                ParameterExpression xParameter = Expression.Parameter(typeof(TEnum), "x");
                ParameterExpression yParameter = Expression.Parameter(typeof(TEnum), "y");
                BinaryExpression equalExpression = Expression.Equal(xParameter, yParameter);

                return Expression.Lambda<Func<TEnum, TEnum, bool>>(equalExpression, xParameter, yParameter).Compile();
            }

            /// <summary>
            /// return ((underlyingType)obj).GetHashCode();
            /// </summary>
            private static Func<TEnum, int> GenerateGetHashCode()
            {
                ParameterExpression objParameter = Expression.Parameter(typeof(TEnum), "obj");
                Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
                UnaryExpression enumCastToUnderlyingType = Expression.Convert(objParameter, underlyingType);
                // ReSharper disable once AssignNullToNotNullAttribute - the constructor ensures TEnum has an underlying enum type
                MethodCallExpression getHashCodeCall = Expression.Call(enumCastToUnderlyingType, underlyingType.GetMethod(nameof(Object.GetHashCode)));

                return Expression.Lambda<Func<TEnum, int>>(getHashCodeCall, objParameter).Compile();
            }

            /// <summary>
            /// return ((underlyingType)x).CompareTo((underlyingType)y);
            /// </summary>
            private static Func<TEnum, TEnum, int> GenerateCompare()
            {
                Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
                ParameterExpression xParameter = Expression.Parameter(typeof(TEnum), "x");
                ParameterExpression yParameter = Expression.Parameter(typeof(TEnum), "y");
                UnaryExpression xAsUnderlyingType = Expression.Convert(xParameter, underlyingType);
                UnaryExpression yAsUnderlyingType = Expression.Convert(yParameter, underlyingType);
                // ReSharper disable once AssignNullToNotNullAttribute - the constructor ensures TEnum has is a real enum with a comparable underlying type
                MethodCallExpression compareToCall = Expression.Call(xAsUnderlyingType, underlyingType.GetMethod(nameof(IComparable<_>.CompareTo), new Type[] { underlyingType }), yAsUnderlyingType);

                return Expression.Lambda<Func<TEnum, TEnum, int>>(compareToCall, xParameter, yParameter).Compile();
            }

            #endregion

            #region Instance Methods

            public override bool Equals(TEnum x, TEnum y)
            {
                if (equals == null)
                    Interlocked.CompareExchange(ref equals, GenerateEquals(), null);
                return equals.Invoke(x, y);
            }

            public override int GetHashCode(TEnum obj)
            {
                if (getHashCode == null)
                    Interlocked.CompareExchange(ref getHashCode, GenerateGetHashCode(), null);
                return getHashCode.Invoke(obj);
            }

            public override int Compare(TEnum x, TEnum y)
            {
                if (compare == null)
                    Interlocked.CompareExchange(ref compare, GenerateCompare(), null);
                return compare.Invoke(x, y);
            }

            #endregion

            #endregion
        }

#endif
        #endregion

        #endregion

        #region Fields

        private static EnumComparer<TEnum> comparer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the comparer instance for <typeparamref name="TEnum"/> type.
        /// </summary>
        public static EnumComparer<TEnum> Comparer => comparer ??=
#if NETSTANDARD2_0
            new FallbackEnumComparer();
#elif NETFRAMEWORK && !NET35
            AppDomain.CurrentDomain.IsFullyTrusted
                ? EnumComparerBuilder.GetComparer<TEnum>()
                : new PartiallyTrustedEnumComparer();
#else
            EnumComparerBuilder.GetComparer<TEnum>();
#endif

        #endregion

        #region Constructors

        /// <summary>
        /// Protected constructor to prevent direct instantiation.
        /// </summary>
        protected EnumComparer()
        {
            // this could be in static ctor but that would throw a TypeInitializationException at unexpected place
            if (!typeof(TEnum).IsEnum)
                throw new InvalidOperationException(Res.EnumTypeParameterInvalid);
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Determines whether the specified <paramref name="obj"/> is the same type of <see cref="EnumComparer{TEnum}"/> as the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns><see langword="true"/>&#160;if the specified <see cref="object" /> is equal to this instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => obj is EnumComparer<TEnum>;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => typeof(EnumComparer<TEnum>).GetHashCode();

        /// <summary>
        /// Determines whether two <typeparamref name="TEnum"/> instances are equal.
        /// </summary>
        /// <returns>
        /// <see langword="true"/>&#160;if the specified values are equal; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="x">The first <typeparamref name="TEnum"/> value to compare.</param>
        /// <param name="y">The second <typeparamref name="TEnum"/> value to compare.</param>
        public abstract bool Equals(TEnum x, TEnum y);

        /// <summary>
        /// Returns a hash code for the specified <typeparamref name="TEnum"/> instance.
        /// </summary>
        /// <returns>
        /// A hash code for the specified <typeparamref name="TEnum"/> instance.
        /// </returns>
        /// <param name="obj">The <typeparamref name="TEnum"/> for which a hash code is to be returned.</param>
        /// <remarks>Returned hash code is not necessarily equals with own hash code of an <see langword="enum"/>&#160;value but provides a fast and well-spread value.</remarks>
        public abstract int GetHashCode(TEnum obj);

        /// <summary>
        /// Compares two <typeparamref name="TEnum"/> instances and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// <list type="table">
        /// <listheader><term>Value</term>&#160;<description>Condition</description></listheader>
        /// <item><term>Less than zero</term>&#160;<description><paramref name="x"/> is less than <paramref name="y"/>.</description></item>
        /// <item><term>Zero</term>&#160;<description><paramref name="x"/> equals <paramref name="y"/>.</description></item>
        /// <item><term>Greater than zero</term>&#160;<description><paramref name="x"/> is greater than <paramref name="y"/>.</description></item>
        /// </list>
        /// </returns>
        /// <param name="x">The first <typeparamref name="TEnum"/> instance to compare.</param>
        /// <param name="y">The second <typeparamref name="TEnum"/> instance to compare.</param>
        public abstract int Compare(TEnum x, TEnum y);

        #endregion

        #region Explicitly Implemented Interface Methods

        [SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "False alarm, SecurityCriticalAttribute is applied.")]
        [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly", Justification = "It MUST NOT be overridable. Every derived type must return the Comparer singleton instance.")]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) => info.SetType(typeof(SerializationUnityHolder));

        #endregion

        #endregion
    }
}
