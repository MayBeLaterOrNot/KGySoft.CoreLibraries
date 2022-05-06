﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MethodAccessor.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;

using KGySoft.Annotations;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Reflection
{
    /// <summary>
    /// Provides an efficient way for invoking methods via dynamically created delegates.
    /// <br/>See the <strong>Remarks</strong> section for details and an example.
    /// </summary>
    /// <remarks>
    /// <para>You can obtain a <see cref="MethodAccessor"/> instance by the static <see cref="GetAccessor">GetAccessor</see> method.</para>
    /// <para>The <see cref="Invoke">Invoke</see> method can be used to invoke the method.
    /// The first call of this method is slow because the delegate is generated on the first access, but further calls are much faster.</para>
    /// <para>The already obtained accessors are cached so subsequent <see cref="GetAccessor">GetAccessor</see> calls return the already created accessors unless
    /// they were dropped out from the cache, which can store about 8000 elements.</para>
    /// <note>If you want to invoke a method by name rather then by a <see cref="MethodInfo"/>, then you can use the <see cref="O:KGySoft.Reflection.Reflector.InvokeMethod">InvokeMethod</see>
    /// methods in the <see cref="Reflector"/> class, which have some overloads with a <c>methodName</c> parameter.</note>
    /// <note type="warning">The .NET Standard 2.0 version of the <see cref="Invoke">Invoke</see> method does not return the ref/out parameters.
    /// Furthermore, if an instance method of a value type (<see langword="struct"/>) mutates the instance,
    /// then the changes will not be applied to the instance on which the method is invoked.
    /// <br/>If you reference the .NET Standard 2.0 version of the <c>KGySoft.CoreLibraries</c> assembly, then use the
    /// <see cref="O:KGySoft.Reflection.Reflector.InvokeMethod">Reflector.InvokeMethod</see> overloads to invoke methods with ref/out parameters without losing the returned parameter values
    /// and to preserve changes the of the mutated value type instances.</note>
    /// </remarks>
    /// <example>
    /// <code lang="C#"><![CDATA[
    /// using System;
    /// using System.Reflection;
    /// using KGySoft.Diagnostics;
    /// using KGySoft.Reflection;
    /// 
    /// class Example
    /// {
    ///     private class TestClass
    ///     {
    ///         public int TestMethod(int i) => i;
    ///     }
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         var instance = new TestClass();
    ///         MethodInfo method = instance.GetType().GetMethod(nameof(TestClass.TestMethod));
    ///         MethodAccessor accessor = MethodAccessor.GetAccessor(method);
    /// 
    ///         new PerformanceTest { Iterations = 1_000_000 }
    ///             .AddCase(() => instance.TestMethod(1), "Direct call")
    ///             .AddCase(() => method.Invoke(instance, new object[] { 1 }), "MethodInfo.Invoke")
    ///             .AddCase(() => accessor.Invoke(instance, 1), "MethodAccessor.Invoke")
    ///             .DoTest()
    ///             .DumpResults(Console.Out);
    ///     }
    /// }
    /// 
    /// // This code example produces a similar output to this one:
    /// // ==[Performance Test Results]================================================
    /// // Iterations: 1,000,000
    /// // Warming up: Yes
    /// // Test cases: 3
    /// // Calling GC.Collect: Yes
    /// // Forced CPU Affinity: No
    /// // Cases are sorted by time (quickest first)
    /// // --------------------------------------------------
    /// // 1. Direct call: average time: 2.87 ms
    /// // 2. MethodAccessor.Invoke: average time: 26.02 ms (+23.15 ms / 906.97 %)
    /// // 3. MethodInfo.Invoke: average time: 241.47 ms (+238.60 ms / 8,416.44 %)]]></code>
    /// </example>
    public abstract class MethodAccessor : MemberAccessor
    {
        #region Fields

        private Delegate? invoker;
        private Delegate? genericInvoker;

        #endregion

        #region Properties

        private protected MethodBase Method => (MethodBase)MemberInfo;
        private protected Delegate Invoker => invoker ??= CreateInvoker();
        private protected Delegate GenericInvoker => genericInvoker ??= CreateGenericInvoker();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodAccessor"/> class.
        /// </summary>
        /// <param name="method">The method for which the accessor is to be created.</param>
        protected MethodAccessor(MethodBase method) :
            // ReSharper disable once ConstantConditionalAccessQualifier - null check is in base so it is needed here
            base(method, method?.GetParameters().Select(p => p.ParameterType).ToArray())
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        #region Public Methods

        /// <summary>
        /// Gets a <see cref="MemberAccessor"/> for the specified <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The method for which the accessor should be retrieved.</param>
        /// <returns>A <see cref="MethodAccessor"/> instance that can be used to invoke the method.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static MethodAccessor GetAccessor(MethodInfo method)
        {
            if (method == null)
                Throw.ArgumentNullException(Argument.method);
            return (MethodAccessor)GetCreateAccessor(method);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Creates an accessor for a method without caching.
        /// </summary>
        /// <param name="method">The method for which the accessor should be retrieved.</param>
        /// <returns>A <see cref="MethodAccessor"/> instance that can be used to invoke the method.</returns>
        internal static MethodAccessor CreateAccessor(MethodInfo method) => method.ReturnType == Reflector.VoidType
            ? (MethodAccessor)new ActionMethodAccessor(method)
            : new FunctionMethodAccessor(method);

        #endregion

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Invokes the method. The return value of <see cref="Void"/> methods are <see langword="null"/>.
        /// For static methods the <paramref name="instance"/> parameter is omitted (can be <see langword="null"/>).
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="instance">The instance that the method belongs to. Can be <see langword="null"/>&#160;for static methods.</param>
        /// <param name="parameters">The parameters to be used for invoking the method.</param>
        /// <returns>The return value of the method, or <see langword="null"/>&#160;for <see cref="Void"/> methods.</returns>
        /// <remarks>
        /// <para>Invoking the method for the first time is slower than the <see cref="MethodBase.Invoke(object,object[])">System.Reflection.MethodBase.Invoke</see>
        /// method but further calls are much faster.</para>
        /// <note type="caller">The .NET Standard 2.0 version of this method does not assign back the ref/out parameters in the <paramref name="parameters"/> argument.
        /// Furthermore, if an instance method of a value type (<see langword="struct"/>) mutates the instance,
        /// then the changes will not be applied to the <paramref name="instance"/> parameter in the .NET Standard 2.0 version.
        /// <br/>If you reference the .NET Standard 2.0 version of the <c>KGySoft.CoreLibraries</c> assembly, then use the
        /// <see cref="O:KGySoft.Reflection.Reflector.InvokeMethod">Reflector.InvokeMethod</see> overloads to invoke methods with ref/out parameters without losing the returned parameter values
        /// and to preserve changes the of the mutated value type instances.</note>
        /// </remarks>
        public abstract object? Invoke(object? instance, params object?[]? parameters);

        public void InvokeStaticAction()
        {
            if (GenericInvoker is Action action)
                action.Invoke();
            else
                ThrowStatic<_>();
        }

        public void InvokeStaticAction<T>(T param)
        {
            if (GenericInvoker is Action<T> action)
                action.Invoke(param);
            else
                ThrowStatic<_>();
        }

        public void InvokeStaticAction<T1, T2>(T1 param1, T2 param2)
        {
            if (GenericInvoker is Action<T1, T2> action)
                action.Invoke(param1, param2);
            else
                ThrowStatic<_>();
        }

        public void InvokeStaticAction<T1, T2, T3>(T1 param1, T2 param2, T3 param3)
        {
            if (GenericInvoker is Action<T1, T2, T3> action)
                action.Invoke(param1, param2, param3);
            else
                ThrowStatic<_>();
        }

        public void InvokeStaticAction<T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4)
        {
            if (GenericInvoker is Action<T1, T2, T3, T4> action)
                action.Invoke(param1, param2, param3, param4);
            else
                ThrowStatic<_>();
        }

        public TResult InvokeStaticFunction<TResult>()
            => GenericInvoker is Func<TResult> func ? func.Invoke() : ThrowStatic<TResult>();

        public TResult InvokeStaticFunction<T, TResult>(T param)
            => GenericInvoker is Func<T, TResult> func ? func.Invoke(param) : ThrowStatic<TResult>();

        public TResult InvokeStaticFunction<T1, T2, TResult>(T1 param1, T2 param2)
            => GenericInvoker is Func<T1, T2, TResult> func ? func.Invoke(param1, param2) : ThrowStatic<TResult>();

        public TResult InvokeStaticFunction<T1, T2, T3, TResult>(T1 param1, T2 param2, T3 param3)
            => GenericInvoker is Func<T1, T2, T3, TResult> func ? func.Invoke(param1, param2, param3) : ThrowStatic<TResult>();

        public TResult InvokeStaticFunction<T1, T2, T3, T4, TResult>(T1 param1, T2 param2, T3 param3, T4 param4)
            => GenericInvoker is Func<T1, T2, T3, T4, TResult> func ? func.Invoke(param1, param2, param3, param4) : ThrowStatic<TResult>();

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public void InvokeInstanceAction<TInstance>(TInstance instance) where TInstance : class
        {
            if (GenericInvoker is ReferenceTypeAction<TInstance> action)
                action.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance));
            else
                ThrowInstance<_>();
        }

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public void InvokeInstanceAction<TInstance, T>(TInstance instance, T param) where TInstance : class
        {
            if (GenericInvoker is ReferenceTypeAction<TInstance, T> action)
                action.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance), param);
            else
                ThrowInstance<_>();
        }

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public void InvokeInstanceAction<TInstance, T1, T2>(TInstance instance, T1 param1, T2 param2) where TInstance : class
        {
            if (GenericInvoker is ReferenceTypeAction<TInstance, T1, T2> action)
                action.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance), param1, param2);
            else
                ThrowInstance<_>();
        }

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public void InvokeInstanceAction<TInstance, T1, T2, T3>(TInstance instance, T1 param1, T2 param2, T3 param3) where TInstance : class
        {
            if (GenericInvoker is ReferenceTypeAction<TInstance, T1, T2, T3> action)
                action.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance), param1, param2, param3);
            else
                ThrowInstance<_>();
        }

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public void InvokeInstanceAction<TInstance, T1, T2, T3, T4>(TInstance instance, T1 param1, T2 param2, T3 param3, T4 param4) where TInstance : class
        {
            if (GenericInvoker is ReferenceTypeAction<TInstance, T1, T2, T3, T4> action)
                action.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance), param1, param2, param3, param4);
            else
                ThrowInstance<_>();
        }

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public TResult InvokeInstanceFunction<TInstance, TResult>(TInstance instance) where TInstance : class
            => GenericInvoker is ReferenceTypeFunction<TInstance, TResult> func
                ? func.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance))
                : ThrowInstance<TResult>();

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public TResult InvokeInstanceFunction<TInstance, T, TResult>(TInstance instance, T param) where TInstance : class
            => GenericInvoker is ReferenceTypeFunction<TInstance, T, TResult> func
                ? func.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance), param)
                : ThrowInstance<TResult>();

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public TResult InvokeInstanceFunction<TInstance, T1, T2, TResult>(TInstance instance, T1 param1, T2 param2) where TInstance : class
            => GenericInvoker is ReferenceTypeFunction<TInstance, T1, T2, TResult> func
                ? func.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance), param1, param2)
                : ThrowInstance<TResult>();

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public TResult InvokeInstanceFunction<TInstance, T1, T2, T3, TResult>(TInstance instance, T1 param1, T2 param2, T3 param3) where TInstance : class
            => GenericInvoker is ReferenceTypeFunction<TInstance, T1, T2, T3, TResult> func
                ? func.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance), param1, param2, param3)
                : ThrowInstance<TResult>();

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "False alarm, instance can be null.")]
        public TResult InvokeInstanceFunction<TInstance, T1, T2, T3, T4, TResult>(TInstance instance, T1 param1, T2 param2, T3 param3, T4 param4) where TInstance : class
            => GenericInvoker is ReferenceTypeFunction<TInstance, T1, T2, T3, T4, TResult> func
                ? func.Invoke(instance ?? Throw.ArgumentNullException<TInstance>(Argument.instance), param1, param2, param3, param4)
                : ThrowInstance<TResult>();

        public void InvokeInstanceAction<TInstance>(in TInstance instance) where TInstance : struct
        {
            if (GenericInvoker is ValueTypeAction<TInstance> action)
                action.Invoke(instance);
            else
                ThrowInstance<_>();
        }

        public void InvokeInstanceAction<TInstance, T>(in TInstance instance, T param) where TInstance : struct
        {
            if (GenericInvoker is ValueTypeAction<TInstance, T> action)
                action.Invoke(instance, param);
            else
                ThrowInstance<_>();
        }

        public void InvokeInstanceAction<TInstance, T1, T2>(in TInstance instance, T1 param1, T2 param2) where TInstance : struct
        {
            if (GenericInvoker is ValueTypeAction<TInstance, T1, T2> action)
                action.Invoke(instance, param1, param2);
            else
                ThrowInstance<_>();
        }

        public void InvokeInstanceAction<TInstance, T1, T2, T3>(in TInstance instance, T1 param1, T2 param2, T3 param3) where TInstance : struct
        {
            if (GenericInvoker is ValueTypeAction<TInstance, T1, T2, T3> action)
                action.Invoke(instance, param1, param2, param3);
            else
                ThrowInstance<_>();
        }

        public void InvokeInstanceAction<TInstance, T1, T2, T3, T4>(in TInstance instance, T1 param1, T2 param2, T3 param3, T4 param4) where TInstance : struct
        {
            if (GenericInvoker is ValueTypeAction<TInstance, T1, T2, T3, T4> action)
                action.Invoke(instance, param1, param2, param3, param4);
            else
                ThrowInstance<_>();
        }

        public TResult InvokeInstanceFunction<TInstance, TResult>(in TInstance instance) where TInstance : struct
            => GenericInvoker is ValueTypeFunction<TInstance, TResult> func ? func.Invoke(instance) : ThrowInstance<TResult>();

        public TResult InvokeInstanceFunction<TInstance, T, TResult>(in TInstance instance, T param) where TInstance : struct
            => GenericInvoker is ValueTypeFunction<TInstance, T, TResult> func ? func.Invoke(instance, param) : ThrowInstance<TResult>();

        public TResult InvokeInstanceFunction<TInstance, T1, T2, TResult>(in TInstance instance, T1 param1, T2 param2) where TInstance : struct
            => GenericInvoker is ValueTypeFunction<TInstance, T1, T2, TResult> func ? func.Invoke(instance, param1, param2) : ThrowInstance<TResult>();

        public TResult InvokeInstanceFunction<TInstance, T1, T2, T3, TResult>(in TInstance instance, T1 param1, T2 param2, T3 param3) where TInstance : struct
            => GenericInvoker is ValueTypeFunction<TInstance, T1, T2, T3, TResult> func ? func.Invoke(instance, param1, param2, param3) : ThrowInstance<TResult>();

        public TResult InvokeInstanceFunction<TInstance, T1, T2, T3, T4, TResult>(in TInstance instance, T1 param1, T2 param2, T3 param3, T4 param4) where TInstance : struct
            => GenericInvoker is ValueTypeFunction<TInstance, T1, T2, T3, T4, TResult> func ? func.Invoke(instance, param1, param2, param3, param4) : ThrowInstance<TResult>();

        #endregion

        #region Private Protected Methods

        private protected abstract Delegate CreateInvoker();
        private protected abstract Delegate CreateGenericInvoker();

        [MethodImpl(MethodImplOptions.NoInlining)]
        [ContractAnnotation("=> halt"), DoesNotReturn]
        private protected void PostValidate(object? instance, object?[]? parameters, Exception exception)
        {
            if (!Method.IsStatic)
            {
                if (instance == null)
                    Throw.ArgumentNullException(Argument.instance, Res.ReflectionInstanceIsNull);
                if (!Method.DeclaringType!.CanAcceptValue(instance))
                    Throw.ArgumentException(Argument.instance, Res.NotAnInstanceOfType(Method.DeclaringType!));
            }

            if (ParameterTypes.Length > 0)
            {
                if (parameters == null)
                    Throw.ArgumentNullException(Argument.parameters, Res.ArgumentNull);
                if (parameters.Length != ParameterTypes.Length)
                    Throw.ArgumentException(Argument.parameters, Res.ReflectionParametersInvalid);
                for (int i = 0; i < ParameterTypes.Length; i++)
                {
                    if (!ParameterTypes[i].CanAcceptValue(parameters[i]))
                        Throw.ArgumentException(Argument.parameters, Res.ReflectionParametersInvalid);
                }
            }

            ThrowIfSecurityConflict(exception);

            // exceptions from the method itself: re-throwing the original exception
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        #endregion

        #region Private Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T ThrowStatic<T>() => !Method.IsStatic
            ? Throw.InvalidOperationException<T>(Res.ReflectionStaticMethodExpectedGeneric(Method.Name, Method.DeclaringType!))
            : Throw.ArgumentException<T>(Res.ReflectionCannotInvokeMethodGeneric(Method.Name, Method.DeclaringType));

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T ThrowInstance<T>() => Method.IsStatic
            ? Throw.InvalidOperationException<T>(Res.ReflectionInstanceMethodExpectedGeneric(Method.Name, Method.DeclaringType))
            : Throw.ArgumentException<T>(Res.ReflectionCannotInvokeMethodGeneric(Method.Name, Method.DeclaringType));

        #endregion

        #endregion

        #endregion
    }
}
