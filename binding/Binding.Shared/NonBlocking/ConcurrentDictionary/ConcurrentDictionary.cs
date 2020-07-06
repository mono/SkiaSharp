// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

//
// Doc comments match, where possible, System.Collection.Concurrent.ConcurrentDictionary in 
// https://github.com/dotnet/corefx to ensure compatibility.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static NonBlocking.DictionaryImpl;

namespace NonBlocking
{
    /// <summary>
    /// Represents a thread-safe and lock-free collection of keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <remarks>
    /// All public and protected members of <see cref="ConcurrentDictionary{TKey,TValue}"/> are thread-safe and may be used
    /// concurrently from multiple threads.
    /// </remarks>
    public class ConcurrentDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IDictionary,
        ICollection
    {
        internal DictionaryImpl<TKey, TValue> _table;
        internal uint _lastResizeTickMillis;

        internal readonly bool valueIsValueType = typeof(TValue).GetTypeInfo().IsValueType;

        internal object _sweeperInstance;
        internal int _sweepRequests;

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the default initial capacity, and uses the default comparer for the key type.</summary>
        public ConcurrentDictionary() : this(31)
        {
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.</param>
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty and uses the default comparer for the key type.</summary>
        public ConcurrentDictionary(int capacity) : this(capacity, null)
        {
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the specified capacity, and uses the default comparer for the key type.</summary>
        /// <param name="concurrencyLevel">Any number greater than '0'. This parameter is allowed for compatibility reasons and has no effect on the dictionary.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="concurrencyLevel" /> is less than 1.-or-<paramref name="capacity" /> is less than 0.</exception>
        public ConcurrentDictionary(int concurrencyLevel, int capacity) : this(capacity)
        {
            if (concurrencyLevel < 1)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel");
            }
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" />, has the default initial capacity, and uses the default comparer for the key type.</summary>
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="collection" /> or any of its keys is a null reference (Nothing in Visual Basic)</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="collection" /> contains one or more duplicate keys.</exception>
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this()
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.InitializeFromCollection(collection);
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the default capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.</summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" /> implementation to use when comparing keys.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="comparer" /> is a null reference (Nothing in Visual Basic).</exception>
        public ConcurrentDictionary(IEqualityComparer<TKey> comparer) : this(31, comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable" />, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.</summary>
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" /> implementation to use when comparing keys.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="collection" /> is a null reference (Nothing in Visual Basic). -or- <paramref name="comparer" /> is a null reference (Nothing in Visual Basic).</exception>
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this(comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this.InitializeFromCollection(collection);
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable" />, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.</summary>
        /// <param name="concurrencyLevel">Any number greater than '0'. This parameter is allowed for compatibility reasons and has no effect on the dictionary.</param>
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" /> implementation to use when comparing keys.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="collection" /> is a null reference (Nothing in Visual Basic). -or- <paramref name="comparer" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="concurrencyLevel" /> is less than 1.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="collection" /> contains one or more duplicate keys.</exception>
        public ConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this(comparer)
        {
            if (concurrencyLevel < 1)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel");
            }
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this.InitializeFromCollection(collection);
        }

        private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (KeyValuePair<TKey, TValue> current in collection)
            {
                if (!this.TryAdd(current.Key, current.Value))
                {
                    throw new ArgumentException("Collection contains duplicate keys");
                }
            }
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.</summary>
        /// <param name="concurrencyLevel">Any number greater than '0'. This parameter is allowed for compatibility reasons and has no effect on the dictionary.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" /> implementation to use when comparing keys.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="comparer" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="concurrencyLevel" /> is less than 1. -or- <paramref name="capacity" /> is less than 0.</exception>
        public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : this(capacity, comparer)
        {
            if (concurrencyLevel < 1)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
        }

        private ConcurrentDictionary(
            int capacity,
            IEqualityComparer<TKey> comparer = null)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (default(TKey) == null)
            {
                if (typeof(TKey) == typeof(ValueType) ||
                    !(default(TKey) is ValueType))
                {
                    _table = DictionaryImpl<TKey, TValue>.CreateRefUnsafe(this, capacity);
                    _table._keyComparer = comparer ?? EqualityComparer<TKey>.Default;
                    return;
                }
            }
            else
            {
                if (typeof(TKey) == typeof(int))
                {
                    if (comparer == null)
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplIntNoComparer<TValue>(capacity, (ConcurrentDictionary<int, TValue>)(object)this);
                    }
                    else
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplInt<TValue>(capacity, (ConcurrentDictionary<int, TValue>)(object)this);
                        _table._keyComparer = comparer;
                    }
                    return;
                }

                if (typeof(TKey) == typeof(long))
                {
                    if (comparer == null)
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplLongNoComparer<TValue>(capacity, (ConcurrentDictionary<long, TValue>)(object)this);
                    }
                    else
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplLong<TValue>(capacity, (ConcurrentDictionary<long, TValue>)(object)this);
                        _table._keyComparer = comparer;
                    }
                    return ;
                }

                if (typeof(TKey) == typeof(uint))
                {
                    if (comparer == null)
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplUIntNoComparer<TValue>(capacity, (ConcurrentDictionary<uint, TValue>)(object)this);
                    }
                    else
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplUInt<TValue>(capacity, (ConcurrentDictionary<uint, TValue>)(object)this);
                        _table._keyComparer = comparer;
                    }
                    return;
                }

                if (typeof(TKey) == typeof(ulong))
                {
                    if (comparer == null)
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplULongNoComparer<TValue>(capacity, (ConcurrentDictionary<ulong, TValue>)(object)this);
                    }
                    else
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplULong<TValue>(capacity, (ConcurrentDictionary<ulong, TValue>)(object)this);
                        _table._keyComparer = comparer;
                    }
                    return;
                }

                if (typeof(TKey) == typeof(IntPtr))
                {
                    if (comparer == null)
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplIntPtrNoComparer<TValue>(capacity, (ConcurrentDictionary<IntPtr, TValue>)(object)this);
                    }
                    else
                    {
                        _table = (DictionaryImpl<TKey, TValue>)(object)new DictionaryImplIntPtr<TValue>(capacity, (ConcurrentDictionary<IntPtr, TValue>)(object)this);
                        _table._keyComparer = comparer;
                    }
                    return;
                }
            }

            _table = new DictionaryImplBoxed<TKey, TValue>(capacity, this);
            _table._keyComparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        /// <summary>
        /// Adds the specified key and value to the <see cref="ConcurrentDictionary{TKey,
        /// TValue}"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// An element with the same key already exists in the <see
        /// cref="ConcurrentDictionary{TKey,TValue}"/>.</exception>
        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
            {
                throw new ArgumentException("AddingDuplicate");
            }
        }

        /// <summary>
        /// Attempts to add the specified key and value to the <see cref="ConcurrentDictionary{TKey,
        /// TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        /// in Visual Basic) for reference types.</param>
        /// <returns>true if the key/value pair was added to the <see cref="ConcurrentDictionary{TKey,
        /// TValue}"/>
        /// successfully; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// contains too many elements.</exception>
        public bool TryAdd(TKey key, TValue value)
        {
            TValue oldVal = default;
            return _table.PutIfMatch(key, value, ref oldVal, ValueMatch.NullOrDead);
        }

        /// <summary>
        /// Removes the element with the specified key from the         
        /// <see cref="ConcurrentDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise false. This method also returns
        /// false if
        /// <paramref name="key"/> was not found in the original <see
        /// cref="T:System.Collections.Generic.IDictionary{TKey,TValue}"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        public bool Remove(TKey key)
        {
            TValue oldVal = default;
            return _table.RemoveIfMatch(key, ref oldVal, ValueMatch.Any);
        }

        /// <summary>
        /// Attempts to remove and return the value with the specified key from the
        /// <see cref="ConcurrentDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, <paramref name="value"/> contains the object removed from the
        /// <see cref="ConcurrentDictionary{TKey,TValue}"/> or the default value of <typeparamref
        /// name="TValue"/>
        /// if the operation failed.</param>
        /// <returns>true if an object was removed successfully; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        public bool TryRemove(TKey key, out TValue value)
        {
            value = default;
            return _table.RemoveIfMatch(key, ref value, ValueMatch.NotNullOrDead);
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key from the <see
        /// cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, <paramref name="value"/> contains the object from
        /// the
        /// <see cref="ConcurrentDictionary{TKey,TValue}"/> with the specified key or the default value of
        /// <typeparamref name="TValue"/>, if the operation failed.</param>
        /// <returns>true if the key was found in the <see cref="ConcurrentDictionary{TKey,TValue}"/>;
        /// otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            object oldValObj = _table.TryGetValue(key);

            Debug.Assert(!(oldValObj is Prime));

            if (oldValObj != null)
            {
                value = FromObjectValue(oldValObj);
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TValue FromObjectValue(object obj)
        {
            // regular value type
            if (default(TValue) != null)
            {
                return Unsafe.As<Boxed<TValue>>(obj).Value;               
            }

            // null
            if (obj == NULLVALUE)
            {
                return default(TValue);
            }

            // ref type
            if (!valueIsValueType)
            {
                return Unsafe.As<object, TValue>(ref obj);
            }

            // nullable
            return (TValue)obj;
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>The value associated with the specified key. If the specified key is not found, a get
        /// operation throws a
        /// <see cref="T:System.Collections.Generic.KeyNotFoundException"/>, and a set operation creates a new
        /// element with the specified key.</value>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and
        /// <paramref name="key"/>
        /// does not exist in the collection.</exception>
        public TValue this[TKey key]
        {
            get
            {
                object oldValObj = _table.TryGetValue(key);

                Debug.Assert(!(oldValObj is Prime));

                if (oldValObj != null)
                {
                    return FromObjectValue(oldValObj);
                }

                throw new KeyNotFoundException();
            }
            set
            {
                TValue oldVal = default;
                _table.PutIfMatch(key, value, ref oldVal, ValueMatch.Any);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="ConcurrentDictionary{TKey, TValue}"/> contains the specified
        /// key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="ConcurrentDictionary{TKey,
        /// TValue}"/>.</param>
        /// <returns>true if the <see cref="ConcurrentDictionary{TKey, TValue}"/> contains an element with
        /// the specified key; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        public bool ContainsKey(TKey key)
        {
            TValue value;
            return this.TryGetValue(key, out value);
        }

        /// <summary>
        /// Adds the specified value to the <see cref="T:System.Collections.Generic.ICollection{TValue}"/>
        /// with the specified key.
        /// </summary>
        /// <param name="keyValuePair">The <see cref="T:System.Collections.Generic.KeyValuePair{TKey,TValue}"/>
        /// structure representing the key and value to add to the <see
        /// cref="T:System.Collections.Generic.Dictionary{TKey,TValue}"/>.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="keyValuePair"/> of <paramref
        /// name="keyValuePair"/> is null.</exception>
        /// <exception cref="T:System.OverflowException">The <see
        /// cref="T:System.Collections.Generic.Dictionary{TKey,TValue}"/>
        /// contains too many elements.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already</exception>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            this.Add(keyValuePair.Key, keyValuePair.Value);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection{TKey,TValue}"/>
        /// contains a specific key and value.
        /// </summary>
        /// <param name="keyValuePair">The <see cref="T:System.Collections.Generic.KeyValuePair{TKey,TValue}"/>
        /// structure to locate in the <see
        /// cref="T:System.Collections.Generic.ICollection{TValue}"/>.</param>
        /// <returns>true if the <paramref name="keyValuePair"/> is found in the <see
        /// cref="T:System.Collections.Generic.ICollection{TKey,TValue}"/>; otherwise, false.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            TValue value;
            return TryGetValue(keyValuePair.Key, out value) && 
                EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value);
        }

        /// <summary>
        /// Compares the existing value for the specified key with a specified value, and if they're equal,
        /// updates the key with a third value.
        /// </summary>
        /// <param name="key">The key whose value is compared with <paramref name="comparisonValue"/> and
        /// possibly replaced.</param>
        /// <param name="newValue">The value that replaces the value of the element with <paramref
        /// name="key"/> if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element with
        /// <paramref name="key"/>.</param>
        /// <returns>true if the value with <paramref name="key"/> was equal to <paramref
        /// name="comparisonValue"/> and replaced with <paramref name="newValue"/>; otherwise,
        /// false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null
        /// reference.</exception>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            TValue oldVal = comparisonValue;
            return _table.PutIfMatch(key, newValue, ref oldVal, ValueMatch.OldValue);
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey,TValue}"/> 
        /// if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The value for the key.  This will be either the existing value for the key if the 
        /// key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
        public TValue GetOrAdd(TKey key, TValue value)
        {
            TValue oldVal = default;
            if (_table.PutIfMatch(key, value, ref oldVal, ValueMatch.NullOrDead))
            {
                return value;
            }

            return FromObjectValue(oldVal);
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey,TValue}"/> 
        /// if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="valueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return _table.GetOrAdd(key, valueFactory);
        }

        /// <summary>
        /// Removes a key and value from the dictionary.
        /// </summary>
        /// <param name="keyValuePair">The <see
        /// cref="T:System.Collections.Generic.KeyValuePair{TKey,TValue}"/>
        /// structure representing the key and value to remove from the <see
        /// cref="T:System.Collections.Generic.Dictionary{TKey,TValue}"/>.</param>
        /// <returns>true if the key and value represented by <paramref name="keyValuePair"/> is successfully
        /// found and removed; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException">The Key property of <paramref
        /// name="keyValuePair"/> is a null reference (Nothing in Visual Basic).</exception>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            TValue oldVal = keyValuePair.Value;
            return _table.RemoveIfMatch(keyValuePair.Key, ref oldVal, ValueMatch.OldValue);
        }

        bool IDictionary.IsReadOnly => false;
        bool IDictionary.IsFixedSize => false;

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is
        /// synchronized with the SyncRoot.
        /// </summary>
        /// <value>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized
        /// (thread safe); otherwise, false. For <see
        /// cref="T:System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/>, this property always
        /// returns false.</value>
        bool ICollection.IsSynchronized => false;


        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        /// <summary>
        /// Gets a value that indicates whether the <see cref="ConcurrentDictionary{TKey,TValue}"/> is empty.
        /// </summary>
        /// <value>true if the <see cref="ConcurrentDictionary{TKey,TValue}"/> is empty; otherwise,
        /// false.</value>
        public bool IsEmpty => _table.Count == 0;

        /// <summary>
        /// Gets the number of key/value pairs contained in the <see
        /// cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <value>The number of key/value pairs contained in the <see
        /// cref="ConcurrentDictionary{TKey,TValue}"/>.</value>
        /// <remarks>Count has snapshot semantics and represents the number of items in the <see
        /// cref="ConcurrentDictionary{TKey,TValue}"/>
        /// at the moment when Count was accessed.</remarks>
        public int Count => _table.Count;

        /// <summary>
        /// Removes all keys and values from the <see cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// </summary>
        public void Clear() => _table.Clear();

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.IEnumerable{TKey}"/> containing the keys of
        /// the <see cref="T:System.Collections.Generic.IReadOnlyDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <value>An <see cref="T:System.Collections.Generic.IEnumerable{TKey}"/> containing the keys of
        /// the <see cref="T:System.Collections.Generic.IReadOnlyDictionary{TKey,TValue}"/>.</value>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => GetKeys();

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.IEnumerable{TValue}"/> containing the values
        /// in the <see cref="T:System.Collections.Generic.IReadOnlyDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <value>An <see cref="T:System.Collections.Generic.IEnumerable{TValue}"/> containing the
        /// values in the <see cref="T:System.Collections.Generic.IReadOnlyDictionary{TKey,TValue}"/>.</value>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => GetValues();

        /// <summary>
        /// Gets a collection containing the keys in the <see
        /// cref="T:System.Collections.Generic.Dictionary{TKey,TValue}"/>.
        /// </summary>
        /// <value>An <see cref="T:System.Collections.Generic.ICollection{TKey}"/> containing the keys in the
        /// <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}"/>.</value>
        public ICollection<TKey> Keys => GetKeys();

        /// <summary>
        /// Gets a collection containing the values in the <see
        /// cref="T:System.Collections.Generic.Dictionary{TKey,TValue}"/>.
        /// </summary>
        /// <value>An <see cref="T:System.Collections.Generic.ICollection{TValue}"/> containing the values in
        /// the
        /// <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}"/>.</value>
        public ICollection<TValue> Values => GetValues();

        ICollection IDictionary.Keys => GetKeys();
        ICollection IDictionary.Values => GetValues();

        bool IDictionary.Contains(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return key is TKey && this.ContainsKey((TKey)((object)key));
        }

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!(key is TKey))
            {
                throw new ArgumentException();
            }
            TValue value2;
            try
            {
                value2 = (TValue)((object)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException();
            }
            ((IDictionary<TKey, TValue>)this).Add((TKey)((object)key), value2);
        }

        void IDictionary.Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key is TKey)
            {
                TValue tValue;
                this.TryRemove((TKey)((object)key), out tValue);
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an array, starting
        /// at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from
        /// the <see cref="T:System.Collections.ICollection"/>. The array must have zero-based
        /// indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying
        /// begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than
        /// 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="index"/> is equal to or greater than
        /// the length of the <paramref name="array"/>. -or- The number of elements in the source <see
        /// cref="T:System.Collections.ICollection"/>
        /// is greater than the available space from <paramref name="index"/> to the end of the destination
        /// <paramref name="array"/>.</exception>
        void ICollection.CopyTo(Array array, int index)
        {
            var pairs = array as KeyValuePair<TKey, TValue>[];
            if (pairs != null)
            {
                CopyToPairs(pairs, index);
                return;
            }

            var entries = array as DictionaryEntry[];
            if (entries != null)
            {
                CopyToEntries(entries, index);
                return;
            }

            var objects = array as object[];
            if (objects != null)
            {
                CopyToObjects(objects, index);
                return;
            }

            throw new ArgumentNullException("array");
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see
        /// cref="T:System.Collections.ICollection"/>. This property is not supported.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The SyncRoot property is not supported.</exception>
        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                TValue tValue;
                if (key is TKey && this.TryGetValue((TKey)((object)key), out tValue))
                {
                    return tValue;
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (!(key is TKey))
                {
                    throw new ArgumentException();
                }
                if (!(value is TValue))
                {
                    throw new ArgumentException();
                }
                this[(TKey)((object)key)] = (TValue)((object)value);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey,TValue}"/> if the key does not already 
        /// exist, or updates a key/value pair in the <see cref="ConcurrentDictionary{TKey,TValue}"/> if the key 
        /// already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key
        /// based on the key's existing value</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="addValueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="updateValueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The new value for the key.  This will be either the result of addValueFactory (if the key was 
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (addValueFactory == null)
            {
                throw new ArgumentNullException("addValueFactory");
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            TValue tValue2;
            while (true)
            {
                TValue tValue;
                if (this.TryGetValue(key, out tValue))
                {
                    tValue2 = updateValueFactory(key, tValue);
                    if (this.TryUpdate(key, tValue2, tValue))
                    {
                        break;
                    }
                }
                else
                {
                    tValue2 = addValueFactory(key);
                    if (this.TryAdd(key, tValue2))
                    {
                        break;
                    }
                }
            }
            return tValue2;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey,TValue}"/> if the key does not already 
        /// exist, or updates a key/value pair in the <see cref="ConcurrentDictionary{TKey,TValue}"/> if the key 
        /// already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on 
        /// the key's existing value</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="updateValueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The new value for the key.  This will be either the value of addValue (if the key was 
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            TValue tValue2;
            while (true)
            {
                TValue tValue;
                if (this.TryGetValue(key, out tValue))
                {
                    tValue2 = updateValueFactory(key, tValue);
                    if (this.TryUpdate(key, tValue2, tValue))
                    {
                        return tValue2;
                    }
                }
                else if (this.TryAdd(key, addValue))
                {
                    return addValue;
                }
            }            
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection"/> to an array of
        /// type <see cref="T:System.Collections.Generic.KeyValuePair{TKey,TValue}"/>, starting at the
        /// specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array of type <see
        /// cref="T:System.Collections.Generic.KeyValuePair{TKey,TValue}"/>
        /// that is the destination of the <see
        /// cref="T:System.Collections.Generic.KeyValuePair{TKey,TValue}"/> elements copied from the <see
        /// cref="T:System.Collections.ICollection"/>. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying
        /// begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than
        /// 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="index"/> is equal to or greater than
        /// the length of the <paramref name="array"/>. -or- The number of elements in the source <see
        /// cref="T:System.Collections.ICollection"/>
        /// is greater than the available space from <paramref name="index"/> to the end of the destination
        /// <paramref name="array"/>.</exception>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            CopyToPairs(array, index);
        }

        /// <summary>
        /// Copy dictionary contents to an array - shared implementation between ToArray and CopyTo.
        /// </summary>
        private void CopyToPairs(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            foreach (var entry in this)
            {
                array[index++] = entry;
            }
        }

        /// <summary>
        /// Copy dictionary contents to an array - shared implementation between ToArray and CopyTo.
        /// </summary>
        private void CopyToEntries(DictionaryEntry[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            foreach (var entry in this)
            {
                array[arrayIndex++] = new DictionaryEntry(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Copy dictionary contents to an array - shared implementation between ToArray and CopyTo.
        /// </summary>
        private void CopyToObjects(object[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var length = array.Length;
            foreach (var entry in this)
            {
                if ((uint)arrayIndex < (uint)length)
                {
                    array[arrayIndex++] = entry;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        /// <summary>Returns an enumerator that iterates through the <see
        /// cref="ConcurrentDictionary{TKey,TValue}"/>.</summary>
        /// <returns>An enumerator for the <see cref="ConcurrentDictionary{TKey,TValue}"/>.</returns>
        /// <remarks>
        /// The enumerator returned from the dictionary is safe to use concurrently with
        /// reads and writes to the dictionary, however it does not represent a moment-in-time snapshot
        /// of the dictionary.  The contents exposed through the enumerator may contain modifications
        /// made to the dictionary after <see cref="GetEnumerator"/> was called.
        /// </remarks>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return _table.GetdIDictEnumerator();
        }

        internal ReadOnlyCollection<TKey> GetKeys()
        {
            var keys = new List<TKey>(Count);
            foreach (var kv in this)
            {
                keys.Add(kv.Key);
            }

            return new ReadOnlyCollection<TKey>(keys);
        }

        internal ReadOnlyCollection<TValue> GetValues()
        {
            var values = new List<TValue>(Count);
            foreach (var kv in this)
            {
                values.Add(kv.Value);
            }

            return new ReadOnlyCollection<TValue>(values);
        }
    }
}
