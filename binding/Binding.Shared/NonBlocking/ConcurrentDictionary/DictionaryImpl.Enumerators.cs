// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NonBlocking
{
    internal abstract partial class DictionaryImpl<TKey, TKeyStore, TValue>
        : DictionaryImpl<TKey, TValue>
    {

        internal override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new SnapshotKV(this);
        }

        internal override IDictionaryEnumerator GetdIDictEnumerator()
        {
            return new SnapshotIDict(this);
        }

        private class Snapshot : IDisposable
        {
            private readonly DictionaryImpl<TKey, TKeyStore, TValue> _table;
            private int _idx;
            protected TKey _curKey, _nextK;
            protected object _curValue, _nextV;

            public Snapshot(DictionaryImpl<TKey, TKeyStore, TValue> dict)
            {
                this._table = dict;

                // linearization point.
                // if table is quiescent and has no copy in progress,
                // we can simply iterate over its table.
                while (true)
                {
                    if (_table._newTable == null)
                    {
                        break;
                    }

                    // there is a copy in progress, finish it and try again
                    _table.HelpCopy(copy_all: true);
                    this._table = (DictionaryImpl<TKey, TKeyStore, TValue>)(this._table._topDict._table);
                }

                // Warm-up the iterator
                MoveNext();
            }

            public bool MoveNext()
            {
                if (_nextV == NULLVALUE)
                {
                    return false;
                }

                _curKey = _nextK;
                _curValue = _nextV;
                _nextV = NULLVALUE;

                var entries = this._table._entries;
                while (_idx < entries.Length)
                {  // Scan array
                    var nextEntry = entries[_idx++];

                    if (nextEntry.value != null)
                    {
                        var nextKstore = nextEntry.key;
                        if (nextKstore == null)
                        {
                            // slot was deleted.
                            continue;
                        }

                        var nextK = _table.keyFromEntry(nextKstore);

                        object nextV = _table.TryGetValue(nextK);
                        if (nextV != null)
                        {
                            _nextK = nextK;

                            // PERF: this would be nice to have as a helper, 
                            // but it does not get inlined
                            if (default(TValue) == null && nextV == NULLVALUE)
                            {
                                _nextV = default(TValue);
                            }
                            else
                            {
                                _nextV = nextV;
                            }


                            break;
                        }
                    }
                }

                return _curValue != NULLVALUE;
            }

            public void Reset()
            {
                _idx = 0;
            }

            public void Dispose()
            {
            }
        }

        private sealed class SnapshotKV : Snapshot, IEnumerator<KeyValuePair<TKey, TValue>>
        {
            public SnapshotKV(DictionaryImpl<TKey, TKeyStore, TValue> dict) : base(dict) { }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    var curValue = this._curValue;
                    if (curValue == NULLVALUE)
                    {
                        throw new InvalidOperationException();
                    }

                    var curValueUnboxed = default(TValue) != null ?
                                            Unsafe.As<Boxed<TValue>>(curValue).Value :
                                            (TValue)curValue;

                    return new KeyValuePair<TKey, TValue>(this._curKey, curValueUnboxed);
                }
            }

            object IEnumerator.Current => Current;
        }

        private sealed class SnapshotIDict : Snapshot, IDictionaryEnumerator
        {
            public SnapshotIDict(DictionaryImpl<TKey, TKeyStore, TValue> dict) : base(dict) { }

            public DictionaryEntry Entry
            {
                get
                {
                    var curValue = this._curValue;
                    if (curValue == NULLVALUE)
                    {
                        throw new InvalidOperationException();
                    }

                    var curValueUnboxed = default(TValue) != null ?
                        Unsafe.As<Boxed<TValue>>(curValue).Value :
                        (TValue)curValue;

                    return new DictionaryEntry(this._curKey, curValueUnboxed);
                }
            }

            public object Key
            {
                get
                {
                    return Entry.Key;
                }
            }

            public object Value
            {
                get
                {
                    return Entry.Value;
                }
            }

            object IEnumerator.Current => Entry;
        }
    }
}
