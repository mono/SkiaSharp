// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace NonBlocking
{
    internal abstract class DictionaryImpl<TKey, TValue>
        : DictionaryImpl
    {
        // TODO: move to leafs
        internal IEqualityComparer<TKey> _keyComparer;

        internal static Func<ConcurrentDictionary<TKey, TValue>, int, DictionaryImpl<TKey, TValue>> CreateRefUnsafe =
            (ConcurrentDictionary <TKey, TValue> topDict, int capacity) =>
            {
                var mObj = new Func<ConcurrentDictionary<object, object>, int, DictionaryImpl<object, object>> (DictionaryImpl.CreateRef);
                var method = mObj.GetMethodInfo().GetGenericMethodDefinition().MakeGenericMethod(new Type[] { typeof(TKey), typeof(TValue) });
                var del = (Func<ConcurrentDictionary<TKey, TValue>, int, DictionaryImpl<TKey, TValue>>)method
                    .CreateDelegate(typeof(Func<ConcurrentDictionary<TKey, TValue>, int, DictionaryImpl<TKey, TValue>>));

                var result = del(topDict, capacity);
                CreateRefUnsafe = del;

                return result;
            };

        internal DictionaryImpl() { }         

        internal abstract void Clear();
        internal abstract int Count { get; }

        internal abstract object TryGetValue(TKey key);
        internal abstract bool PutIfMatch(TKey key, TValue newVal, ref TValue oldValue, ValueMatch match);
        internal abstract bool RemoveIfMatch(TKey key, ref TValue oldValue, ValueMatch match);
        internal abstract TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);

        internal abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
        internal abstract IDictionaryEnumerator GetdIDictEnumerator();
    }
}
