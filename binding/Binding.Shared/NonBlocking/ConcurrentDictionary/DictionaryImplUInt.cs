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
    internal sealed class DictionaryImplUInt<TValue>
                : DictionaryImpl<uint, int, TValue>
    {
        internal DictionaryImplUInt(int capacity, ConcurrentDictionary<uint, TValue> topDict)
            : base(capacity, topDict)
        {
        }

        internal DictionaryImplUInt(int capacity, DictionaryImplUInt<TValue> other)
            : base(capacity, other)
        {
        }

        protected override bool TryClaimSlotForPut(ref int entryKey, uint key)
        {
            return TryClaimSlot(ref entryKey, (int)key);
        }

        protected override bool TryClaimSlotForCopy(ref int entryKey, int key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        private bool TryClaimSlot(ref int entryKey, int key)
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

            return key == entryKeyValue || _keyComparer.Equals((uint)key, (uint)entryKey);
        }

        protected override int hash(uint key)
        {
            if (key == 0)
            {
                return ZEROHASH;
            }

            return base.hash(key);
        }

        protected override bool keyEqual(uint key, int entryKey)
        {
            return key == (uint)entryKey || _keyComparer.Equals(key, (uint)entryKey);
        }

        protected override DictionaryImpl<uint, int, TValue> CreateNew(int capacity)
        {
            return new DictionaryImplUInt<TValue>(capacity, this);
        }

        protected override uint keyFromEntry(int entryKey)
        {
            return (uint)entryKey;
        }
    }

    internal sealed class DictionaryImplUIntNoComparer<TValue>
            : DictionaryImpl<uint, int, TValue>
    {
        internal DictionaryImplUIntNoComparer(int capacity, ConcurrentDictionary<uint, TValue> topDict)
            : base(capacity, topDict)
        {
        }

        internal DictionaryImplUIntNoComparer(int capacity, DictionaryImplUIntNoComparer<TValue> other)
            : base(capacity, other)
        {
        }

        protected override bool TryClaimSlotForPut(ref int entryKey, uint key)
        {
            return TryClaimSlot(ref entryKey, (int)key);
        }

        protected override bool TryClaimSlotForCopy(ref int entryKey, int key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        private bool TryClaimSlot(ref int entryKey, int key)
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

        protected override int hash(uint key)
        {
            return (key == 0) ?
                ZEROHASH :
                (int)key | SPECIAL_HASH_BITS;
        }

        protected override bool keyEqual(uint key, int entryKey)
        {
            return key == (uint)entryKey;
        }

        protected override DictionaryImpl<uint, int, TValue> CreateNew(int capacity)
        {
            return new DictionaryImplUIntNoComparer<TValue>(capacity, this);
        }

        protected override uint keyFromEntry(int entryKey)
        {
            return (uint)entryKey;
        }
    }
}
