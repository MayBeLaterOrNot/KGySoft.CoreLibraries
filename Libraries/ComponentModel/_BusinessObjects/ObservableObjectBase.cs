﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ObservableObjectBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2018 - All Rights Reserved
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

using KGySoft.Collections;
using KGySoft.Libraries;
using KGySoft.Reflection;

#endregion

namespace KGySoft.ComponentModel
{
    /// <summary>
    /// Provides a base class for component model classes, which can notify their consumer about property changes.
    /// <br/>See the <strong>Remarks</strong> section for details.
    /// </summary>
    /// <remarks>
    /// <para>Implementers can use the <see cref="Get{T}(T,string)">Get</see> and <see cref="Set">Set</see> methods in the property accessors to manage event raising automatically.</para>
    /// <para>Consumers can subscribe the <see cref="PropertyChanged"/> event to get notification about the property changes.</para>
    /// <para>Accessing properties can be fine tuned by overriding the <see cref="CanGetProperty">CanGetProperty</see> and <see cref="CanSetProperty">CanSetProperty</see> methods. By default they allow
    /// accessing the instance properties in the implementer class.
    /// <note>To be able to validate property values consider to use the <see cref="ValidatingObjectBase"/> class.</note>
    /// </para>
    /// <example>
    /// The following example shows a possible implementation of a derived class.
    /// <code lang="C#"><![CDATA[
    /// class MyModel : ObservableObjectBase
    /// {
    ///     // A simple integer property (with zero default value). Until the property is set no value is stored internally.
    ///     public int IntProperty { get => Get<int>(); set => Set(value); }
    ///
    ///     // An int property with default value. Until the property is set the default will be returned.
    ///     public int IntPropertyCustomDefault { get => Get(-1); set => Set(value); }
    ///
    ///     // If the default value is a complex one, which should not be evaluated each time you can provide a factory for it:
    ///     // When this property is read for the first time without setting it before, the provided delegate will be invoked
    ///     // and the returned default value is stored without triggering the PropertyChanged event.
    ///     public MyComplexType ComplexProperty { get => Get(() => new MyComplexType()); set => Set(value); }
    /// 
    ///     // You can use regular properties to prevent raising the events and not to store the value in the internal storage.
    ///     // The OnPropertyChanged method still can be called to raise the PropertyChanged event.
    ///     public int UntrackedProperty { get; set; }
    /// }
    /// ]]></code>
    /// </example>
    /// </remarks>
    /// <threadsafety instance="true" static="true"/>
    /// <seealso cref="INotifyPropertyChanged" />
    /// <seealso cref="PersistableObjectBase" />
    /// <seealso cref="UndoableObjectBase" />
    /// <seealso cref="EditableObjectBase" />
    /// <seealso cref="ValidatingObjectBase" />
    /// <seealso cref="ModelBase" />
    public abstract class ObservableObjectBase : INotifyPropertyChanged, IDisposable, ICloneable
    {
        #region MissingPropertyReference class

        [Serializable]
        private sealed class MissingPropertyReference : IObjectReference
        {
            #region Properties

            internal static MissingPropertyReference Value { get; } = new MissingPropertyReference();

            #endregion

            #region Methods

            public object GetRealObject(StreamingContext context) => Value;
            public override string ToString() => Res.Get(Res.MissingPropertyReference);
            public override bool Equals(object obj) => obj is MissingPropertyReference;
            public override int GetHashCode() => 0;

            #endregion
        }

        #endregion

        #region Fields

        #region Static Fields

        private static readonly IThreadSafeCacheAccessor<Type, Dictionary<string, PropertyInfo>> reflectedProperties = new Cache<Type, Dictionary<string, PropertyInfo>>(GetProperties).GetThreadSafeAccessor();

        #endregion

        #region Instance Fields

        private LockingDictionary<string, object> properties = new LockingDictionary<string, object>();

        [NonSerialized]
        private object writeLock; // read lock is the dictionary itself
        [NonSerialized]
        private int suspendCounter;

        private PropertyChangedEventHandler propertyChanged;
        private bool isModified;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changed. The actual type of the event argument is <see cref="PropertyChangedExtendedEventArgs"/>.
        /// </summary>
        /// <remarks>
        /// <note>The <see cref="PropertyChanged"/> event uses the <see cref="PropertyChangedEventHandler"/> delegate in order to consumers, which rely on the conventional property
        /// changed notifications can use it in a compatible way. To get the old and new values in an event handler you can cast the argument to <see cref="PropertyChangedExtendedEventArgs"/>
        /// or call the <see cref="PropertyChangedEventArgsExtensions.TryGetOldPropertyValue">TryGetOldPropertyValue</see> and <see cref="PropertyChangedEventArgsExtensions.TryGetNewPropertyValue">TryGetNewPropertyValue</see> extension methods on it.</note>
        /// </remarks>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add => propertyChanged += value;
            remove => propertyChanged -= value;
        }

        #endregion

        #region Properties

        #region Static Properties

        /// <summary>
        /// Represents the value of a missing property value. Can be returned in <see cref="PropertyChangedExtendedEventArgs"/> by the <see cref="PropertyChanged"/> event
        /// if the stored value of the property has just been created and had no previous value, or when a property has been removed from the inner storage.
        /// </summary>
        /// <remarks><note>Reading the property when it has no value may return a default value or can cause to recreate a value.</note></remarks>
        public static object MissingProperty { get; } = MissingPropertyReference.Value;

        #endregion

        #region Instance Properties

        #region Public Properties

        /// <summary>
        /// Gets whether this instance has been modified.
        /// Modified state can be set by the <see cref="SetModified">SetModified</see> method.
        /// </summary>
        public bool IsModified => isModified;

        #endregion

        #region Internal Properties

        internal LockingDictionary<string, object> PropertiesInternal => properties;

        internal /*private protected*/ object WriteLock
        {
            get
            {
                if (writeLock == null)
                    Interlocked.CompareExchange(ref writeLock, new object(), null);
                return writeLock;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        private static Dictionary<string, PropertyInfo> GetProperties(Type type) => new Dictionary<string, PropertyInfo>(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToDictionary(pi => pi.Name, pi => pi));

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Sets the modified state of this <see cref="ObservableObjectBase"/> instance represented by the <see cref="IsModified"/> property.
        /// </summary>
        /// <param name="value"><see langword="true"/> to mark the object as modified; <see langword="false"/> to mark it unmodified.</param>
        public void SetModified(bool value)
        {
            if (isModified == value)
                return;

            isModified = value;
            OnPropertyChanged(new PropertyChangedExtendedEventArgs(!value, value, nameof(IsModified)));
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// The base implementation clones the internal property storage, the <see cref="IsModified" /> property and the subscribers of the <see cref="PropertyChanged"/> event.
        /// In order to release the old or the cloned instance call the <see cref="Dispose()">Dispose</see> method to clear the subscriptions of the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual object Clone()
        {
            Type type = GetType();
            if (type.GetDefaultConstructor() == null)
                throw new InvalidOperationException(Res.Get(Res.ObservableObjectHasNoDefaultCtor, type));
            ObservableObjectBase clone = (ObservableObjectBase)Reflector.Construct(type);
            clone.properties = CloneProperties().AsThreadSafe();
            clone.isModified = isModified;
            //clone.propertyChanged = propertyChanged;
            return clone;
        }

        /// <summary>
        /// Releases the resources held by this instance.
        /// The base implementation removes the subscribers of the <see cref="PropertyChanged"/> event.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Internal Methods

        internal /*private protected*/ bool TryGetPropertyValue(string propertyName, out object value)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            if (!CanGetProperty(propertyName))
                throw new InvalidOperationException(Res.Get(Res.CannotGetProperty, propertyName));
            return properties.TryGetValue(propertyName, out value);
        }

        internal /*private protected*/ bool TryReplaceProperty(string propertyName, object originalValue, object newValue, bool invokeChangedEvent)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (MissingProperty.Equals(newValue))
                return ResetProperty(propertyName, invokeChangedEvent);

            // including reading of the old value in the lock ensuring we really replace the original value
            lock (WriteLock)
            {
                bool exists = properties.TryGetValue(propertyName, out object currentValue);
                bool tryAddNew = MissingProperty.Equals(originalValue);
                if (exists && tryAddNew || !exists || !Equals(originalValue, currentValue))
                    return false;

                Set(newValue, invokeChangedEvent, propertyName);
            }

            return true;
        }

        internal void ReplaceProperties(IDictionary<string, object> newProperties, bool invokeChangedEvent)
        {
            // Firstly remove the properties, which are not among the new ones. We accept that it can raise some unnecessary events but we cannot set the property if we cannot be sure about the default value.
            lock (WriteLock)
            {
                IEnumerable<string> toRemove = properties.Keys.Except(newProperties.Select(p => p.Key));
                foreach (var propertyName in toRemove)
                    ResetProperty(propertyName, invokeChangedEvent);

                foreach (var property in newProperties)
                    Set(property.Value, invokeChangedEvent, property.Key);
            }
        }

        internal IDictionary<string, object> CloneProperties()
        {
            var result = new Dictionary<string, object>();

            // Locking to prevent reading the properties until they are cloned. This not prevents modifying their content
            // if they are already cached somewhere but at least blocks their access through this class.
            properties.Lock();
            try
            {
                // iterating the underlying dictionary to prevent copying because we are already in a lock
                foreach (KeyValuePair<string, object> property in properties.Collection)
                {
                    // Deep cloning classes only. It could be needed also for structs containing references but there is
                    // no fast way to determine if a struct is blittable and we don't want lose more performance than needed.
                    object clonedValue;
                    if (property.Value is ICloneable cloneable)
                        clonedValue = cloneable.Clone();
                    else if (property.Value?.GetType().IsClass == true)
                        clonedValue = property.Value.DeepClone();
                    else
                        clonedValue = property.Value;
                    result.Add(property.Key, clonedValue);
                }
            }
            finally
            {
                properties.Unlock();
            }

            return result;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the value of a property, or - if it was not set before -, then creates its initial value.
        /// The created initial value will be stored in the internal property storage without triggering the <see cref="PropertyChanged"/> event.
        /// For constant or simple expressions, or to return a default value for a non-existing property without storing it internally use the other <see cref="Get{T}(T,string)">Get</see> overload.
        /// <br/>For an example, see the <strong>Remarks</strong> section of the <see cref="ObservableObjectBase"/> class.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="createInitialValue">A delegate, which creates the initial value if the property does not exist. If <see langword="null"/>,
        /// then an exception is thrown for an uninitialized property.</param>
        /// <param name="propertyName">The name of the property to get. This parameter is optional.
        /// <br/>Default value: The name of the caller member.</param>
        /// <returns>The value of the property, or the created initial value returned by the <paramref name="createInitialValue"/> parameter.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="propertyName"/> cannot be get.
        /// <br/>-or-
        /// <br/>The stored value of the property is not compatible with <typeparamref name="T"/>.
        /// <br/>-or-
        /// <br/><paramref name="propertyName"/> value does not exist and <paramref name="createInitialValue"/> is <see langword="null"/>.
        /// <br/>-or-
        /// <br/>The created default value of the property cannot be set.
        /// <br/>-or-
        /// <br/><see cref="CanGetProperty">CanGetProperty</see> is not overridden and <paramref name="propertyName"/> is not an actual instance property in this instance.
        /// </exception>
        protected T Get<T>(Func<T> createInitialValue, [CallerMemberName] string propertyName = null)
        {
            if (TryGetPropertyValue(propertyName, out object value))
            {
                if (!typeof(T).CanAcceptValue(value))
                    throw new InvalidOperationException(Res.Get(Res.ReturnedTypeInvalid, typeof(T)));
                return (T)value;
            }

            if (createInitialValue == null)
                throw new InvalidOperationException(Res.Get(Res.PropertyValueNotExist, propertyName));
            T result = createInitialValue.Invoke();
            Set(result, false, propertyName);
            return result;
        }

        /// <summary>
        /// Gets the value of a property or <paramref name="defaultValue"/> if no value is stored for it. No new value will be stored
        /// if the property does not exist. If the default initial value is too complex and should not be evaluated every time when the property is get,
        /// or to throw an exception for an uninitialized property use the other <see cref="Get{T}(Func{T},string)">Get</see> overload.
        /// <br/>For an example, see the <strong>Remarks</strong> section of the <see cref="ObservableObjectBase"/> class.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="defaultValue">The value to return if property does not exist. This parameter is optional.
        /// <br/>Default value: The default value of <typeparamref name="T"/> type.</param>
        /// <param name="propertyName">The name of the property to get. This parameter is optional.
        /// <br/>Default value: The name of the caller member.</param>
        /// <returns>The value of the property, or the specified <paramref name="defaultValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="propertyName"/> cannot be get.
        /// <br/>-or-
        /// <br/><see cref="CanGetProperty">CanGetProperty</see> is not overridden and <paramref name="propertyName"/> is not an actual instance property in this instance.
        /// </exception>
        protected T Get<T>(T defaultValue = default, [CallerMemberName] string propertyName = null) 
            => TryGetPropertyValue(propertyName, out object value) && typeof(T).CanAcceptValue(value) ? (T)value : defaultValue;

        /// <summary>
        /// Sets the value of a property.
        /// <br/>For an example, see the <strong>Remarks</strong> section of the <see cref="ObservableObjectBase"/> class.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="invokeChangedEvent">If <see langword="true"/>, and the <paramref name="value"/> is different to the previously stored value, then invokes the <see cref="PropertyChanged"/> event.</param>
        /// <param name="propertyName">Name of the property to set. This parameter is optional.
        /// <br/>Default value: The name of the caller member.</param>
        /// <returns><see langword="true"/> if property has been set (change occurred); otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="propertyName"/> cannot be set.
        /// <br/>-or-
        /// <br/><see cref="CanSetProperty">CanSetProperty</see> is not overridden and <paramref name="propertyName"/> is not an actual instance property in this instance, or <paramref name="value"/> is not compatible with the property type.
        /// </exception>
        protected bool Set(object value, bool invokeChangedEvent = true, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (MissingProperty.Equals(value))
                return ResetProperty(propertyName, invokeChangedEvent);

            if (!CanSetProperty(propertyName, value))
                throw new InvalidOperationException(Res.Get(Res.CannotSetProperty, propertyName));

            bool exists = properties.TryGetValue(propertyName, out object oldValue);
            if (exists && Equals(value, oldValue))
                return false;
            if (!exists)
                oldValue = MissingProperty;

            lock (WriteLock)
                properties[propertyName] = value;
            if (invokeChangedEvent)
                OnPropertyChanged(new PropertyChangedExtendedEventArgs(oldValue, value, propertyName));

            return true;
        }

        /// <summary>
        /// Resets the property of the specified name, meaning, it will be removed from the underlying storage so the getter methods will return the default value again.
        /// </summary>
        /// <param name="propertyName">The name of the property to reset.</param>
        /// <param name="invokeChangedEvent"><see langword="true"/> to allow raising the <see cref="PropertyChanged"/> event; otherwise, <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if property has been reset (it existed previously); otherwise, <see langword="false"/>.</returns>
        protected bool ResetProperty(string propertyName, bool invokeChangedEvent = true)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (!properties.TryGetValue(propertyName, out object oldValue))
                return false;

            lock (WriteLock)
                properties.Remove(propertyName);
            if (invokeChangedEvent)
                OnPropertyChanged(new PropertyChangedExtendedEventArgs(oldValue, MissingProperty, propertyName));

            return true;
        }

        /// <summary>
        /// Gets whether the specified property can be get. The base implementation allows to get the actual instance properties in this instance.
        /// </summary>
        /// <param name="propertyName">Name of the property to get.</param>
        /// <returns><see langword="true"/> if the specified property can be get; otherwise, <see langword="false"/>.</returns>
        protected virtual bool CanGetProperty(string propertyName)
        {
            Dictionary<string, PropertyInfo> props;
            props = reflectedProperties[GetType()];
            return props.ContainsKey(propertyName);
        }

        /// <summary>
        /// Gets whether the specified property can be set. The base implementation allows to set the actual instance properties in this instance if the specified <paramref name="value"/> is compatible with the property type.
        /// </summary>
        /// <param name="propertyName">Name of the property to set.</param>
        /// <param name="value">The property value to set.</param>
        /// <returns><see langword="true"/> if the specified property can be set; otherwise, <see langword="false"/>.</returns>
        protected virtual bool CanSetProperty(string propertyName, object value)
        {
            Dictionary<string, PropertyInfo> props = reflectedProperties[GetType()];
            return props.TryGetValue(propertyName, out PropertyInfo pi) && pi.PropertyType.CanAcceptValue(value);
        }

        /// <summary>
        /// Suspends the raising of the <see cref="PropertyChanged"/> event until <see cref="ResumeChangedEvent">ResumeChangeEvents</see>
        /// method is called. Supports nested calls.
        /// </summary>
        protected void SuspendChangedEvent() => Interlocked.Increment(ref suspendCounter);

        /// <summary>
        /// Resumes the raising of the <see cref="PropertyChanged"/> event suspended by the <see cref="SuspendChangedEvent">SuspendChangeEvents</see> method.
        /// </summary>
        protected void ResumeChangedEvent() => Interlocked.Decrement(ref suspendCounter);

        /// <summary>
        /// Gets whether the change of the specified <paramref name="propertyName"/> affects the <see cref="IsModified"/> property.
        /// <br/>The <see cref="ObservableObjectBase"/> implementation excludes the <see cref="IsModified"/> property itself.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        /// <returns><see langword="true"/> if changing of the specified <paramref name="propertyName"/> affects the value of the <see cref="IsModified"/> property; otherwise, <see langword="false"/>.</returns>
        protected virtual bool AffectsModifiedState(string propertyName) => propertyName != nameof(IsModified);

        /// <summary>
        /// Releases the resources held by this instance.
        /// The base implementation removes the subscribers of the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) => propertyChanged = null;

        #endregion

        #region Protected-Internal Methods

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedExtendedEventArgs" /> instance containing the event data.</param>
        protected internal virtual void OnPropertyChanged(PropertyChangedExtendedEventArgs e)
        {
            if (AffectsModifiedState(e.PropertyName))
                SetModified(true);
            if (suspendCounter <= 0)
                propertyChanged?.Invoke(this, e);
        }

        #endregion

        #endregion

        #endregion
    }
}