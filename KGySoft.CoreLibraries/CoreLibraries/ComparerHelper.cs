﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ComparerHelper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace KGySoft.CoreLibraries
{
    internal static class ComparerHelper<T>
    {
        #region Properties

        internal static IEqualityComparer<T> EqualityComparer { get; } =
#if NETFRAMEWORK
            typeof(T).IsEnum ? EnumComparer<T>.Comparer : EqualityComparer<T>.Default;
#elif NETSTANDARD2_0
            EqualityComparer<T>.Default;
#else
            Environment.OSVersion.Platform == PlatformID.Win32NT && typeof(T).IsEnum ? EnumComparer<T>.Comparer : EqualityComparer<T>.Default;
#endif

        internal static IComparer<T> Comparer { get; } =
#if NETFRAMEWORK
            typeof(T).IsEnum ? EnumComparer<T>.Comparer : Comparer<T>.Default;
#elif NETSTANDARD2_0
            Comparer<T>.Default;
#else
            Environment.OSVersion.Platform == PlatformID.Win32NT && typeof(T).IsEnum ? EnumComparer<T>.Comparer : Comparer<T>.Default;
#endif

        #endregion

        #region Methods

        internal static IEqualityComparer<T>? GetNonDefaultEqualityComparerOrNull(IEqualityComparer<T>? comparer) => IsDefaultComparer(comparer) ? null : comparer;

        internal static bool IsDefaultComparer(IEqualityComparer<T>? comparer)
            // Last part can be optimized away by JIT but only if we use typeof(string) and not Reflector.StringType
            => comparer == null || comparer == EqualityComparer || typeof(T) == typeof(string) && comparer == StringComparer.Ordinal;

        #endregion
    }
}