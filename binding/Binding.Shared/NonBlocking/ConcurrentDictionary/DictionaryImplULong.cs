// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace NonBlocking
{
    internal sealed class DictionaryImplULong<TValue>
                : DictionaryImpl<ulong, long, TValue>
    {
        internal DictionaryImplULong(int capacity, ConcurrentDictionary<ulong, TValue> topDict)
            : base(capacity, topDict)
        {
        }

        internal DictionaryImplULong(int capacity, DictionaryImplULong<TValue> other)
            : base(capacity, other)
        {
        }

        protected override bool TryClaimSlotForPut(ref long entryKey, ulong key)
        {
            return TryClaimSlot(ref entryKey, (long)key);
        }

        protected override bool TryClaimSlotForCopy(ref long entryKey, long key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        private bool TryClaimSlot(ref long entryKey, long key)
        {
            var entryKeyValue = entryKey;
            //zero keys are claimed via hash
            if (entryKeyValue == 0 & key != 0)
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, key, 0);
                if (entryKeyValue == 0)
                {
                    // claimed a new slot
                    this.allocatedSlotCount.Increment();
                    return true;
                }
            }

            return key == entryKeyValue || _keyComparer.Equals((ulong)key, (ulong)entryKey);
        }

        protected override int hash(ulong key)
        {
            if (key == 0)
            {
                return ZEROHASH;
            }

            return base.hash(key);
        }

        protected override bool keyEqual(ulong key, long entryKey)
        {
            return key == (ulong)entryKey || _keyComparer.Equals(key, (ulong)entryKey);
        }

        protected override DictionaryImpl<ulong, long, TValue> CreateNew(int capacity)
        {
            return new DictionaryImplULong<TValue>(capacity, this);
        }

        protected override ulong keyFromEntry(long entryKey)
        {
            return (ulong)entryKey;
        }
    }

    internal sealed class DictionaryImplULongNoComparer<TValue>
            : DictionaryImpl<ulong, long, TValue>
    {
        internal DictionaryImplULongNoComparer(int capacity, ConcurrentDictionary<ulong, TValue> topDict)
            : base(capacity, topDict)
        {
        }

        internal DictionaryImplULongNoComparer(int capacity, DictionaryImplULongNoComparer<TValue> other)
            : base(capacity, other)
        {
        }

        protected override bool TryClaimSlotForPut(ref long entryKey, ulong key)
        {
            return TryClaimSlot(ref entryKey, (long)key);
        }

        protected override bool TryClaimSlotForCopy(ref long entryKey, long key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        private bool TryClaimSlot(ref long entryKey, long key)
        {
            var entryKeyValue = entryKey;
            //zero keys are claimed via hash
            if (entryKeyValue == 0 & key != 0)
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, key, 0);
                if (entryKeyValue == 0)
                {
                    // claimed a new slot
                    this.allocatedSlotCount.Increment();
                    return true;
                }
            }

            return key == entryKeyValue;
        }

        protected override int hash(ulong key)
        {
            return (key == 0) ?
                ZEROHASH :
                key.GetHashCode() | SPECIAL_HASH_BITS;
        }

        protected override bool keyEqual(ulong key, long entryKey)
        {
            return key == (ulong)entryKey;
        }

        protected override DictionaryImpl<ulong, long, TValue> CreateNew(int capacity)
        {
            return new DictionaryImplULongNoComparer<TValue>(capacity, this);
        }

        protected override ulong keyFromEntry(long entryKey)
        {
            return (ulong)entryKey;
        }
    }
}
