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
    internal sealed class DictionaryImplIntPtr<TValue>
                : DictionaryImpl<IntPtr, IntPtr, TValue>
    {
        internal DictionaryImplIntPtr(int capacity, ConcurrentDictionary<IntPtr, TValue> topDict)
            : base(capacity, topDict)
        {
        }

        internal DictionaryImplIntPtr(int capacity, DictionaryImplIntPtr<TValue> other)
            : base(capacity, other)
        {
        }

        protected override bool TryClaimSlotForPut(ref IntPtr entryKey, IntPtr key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        protected override bool TryClaimSlotForCopy(ref IntPtr entryKey, IntPtr key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        private bool TryClaimSlot(ref IntPtr entryKey, IntPtr key)
        {
            var entryKeyValue = entryKey;
            //zero keys are claimed via hash
            if (entryKeyValue == default(IntPtr) & key != default(IntPtr))
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, key, default(IntPtr));
                if (entryKeyValue == default(IntPtr))
                {
                    // claimed a new slot
                    this.allocatedSlotCount.Increment();
                    return true;
                }
            }

            return key == entryKeyValue || _keyComparer.Equals(key, entryKey);
        }

        protected override int hash(IntPtr key)
        {
            if (key == default(IntPtr))
            {
                return ZEROHASH;
            }

            return base.hash(key);
        }

        protected override bool keyEqual(IntPtr key, IntPtr entryKey)
        {
            return key == entryKey || _keyComparer.Equals(key, entryKey);
        }

        protected override DictionaryImpl<IntPtr, IntPtr, TValue> CreateNew(int capacity)
        {
            return new DictionaryImplIntPtr<TValue>(capacity, this);
        }

        protected override IntPtr keyFromEntry(IntPtr entryKey)
        {
            return entryKey;
        }
    }

    internal sealed class DictionaryImplIntPtrNoComparer<TValue>
            : DictionaryImpl<IntPtr, IntPtr, TValue>
    {
        internal DictionaryImplIntPtrNoComparer(int capacity, ConcurrentDictionary<IntPtr, TValue> topDict)
            : base(capacity, topDict)
        {
        }

        internal DictionaryImplIntPtrNoComparer(int capacity, DictionaryImplIntPtrNoComparer<TValue> other)
            : base(capacity, other)
        {
        }

        protected override bool TryClaimSlotForPut(ref IntPtr entryKey, IntPtr key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        protected override bool TryClaimSlotForCopy(ref IntPtr entryKey, IntPtr key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        private bool TryClaimSlot(ref IntPtr entryKey, IntPtr key)
        {
            var entryKeyValue = entryKey;
            //zero keys are claimed via hash
            if (entryKeyValue == default(IntPtr) & key != default(IntPtr))
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, key, default(IntPtr));
                if (entryKeyValue == default(IntPtr))
                {
                    // claimed a new slot
                    this.allocatedSlotCount.Increment();
                    return true;
                }
            }

            return key == entryKeyValue;
        }

        protected override int hash(IntPtr key)
        {
            return (key == default(IntPtr)) ?
                ZEROHASH :
                key.GetHashCode() | SPECIAL_HASH_BITS;
        }

        protected override bool keyEqual(IntPtr key, IntPtr entryKey)
        {
            return key == entryKey;
        }

        protected override DictionaryImpl<IntPtr, IntPtr, TValue> CreateNew(int capacity)
        {
            return new DictionaryImplIntPtrNoComparer<TValue>(capacity, this);
        }

        protected override IntPtr keyFromEntry(IntPtr entryKey)
        {
            return entryKey;
        }
    }
}
