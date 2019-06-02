﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestTypeResolver.cs
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
using System.ComponentModel.Design;
using System.Reflection;

using KGySoft.Reflection;

#endregion

namespace KGySoft.CoreLibraries
{
    internal sealed class TestTypeResolver : ITypeResolutionService
    {
        #region Methods

        public Type GetType(string name, bool throwOnError) => Reflector.ResolveType(name, true, true);
        public Type GetType(string name) => Reflector.ResolveType(name, true, true);

        public Assembly GetAssembly(AssemblyName name, bool throwOnError) => throw new NotImplementedException();
        public Assembly GetAssembly(AssemblyName name) => throw new NotImplementedException();
        public string GetPathOfAssembly(AssemblyName name) => throw new NotImplementedException();
        public Type GetType(string name, bool throwOnError, bool ignoreCase) => throw new NotImplementedException();
        public void ReferenceAssembly(AssemblyName name) => throw new NotImplementedException();

        #endregion
    }
}