﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: HashingStrategyExtensions.cs
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
using System.Collections.Generic;

using KGySoft.CoreLibraries;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Collections
{
    internal static class HashingStrategyExtensions
    {
        #region Methods

        internal static bool PreferBitwiseAndHash<TKey>(this HashingStrategy strategy, IEqualityComparer<TKey>? comparer)
        {
            Type keyType = typeof(TKey);
            return strategy == HashingStrategy.And
                || (strategy == HashingStrategy.Auto
                    && (keyType == Reflector.StringType || (comparer == null && keyType.IsDefaultGetHashCode())));
        }

        #endregion
    }
}