// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

using System;
using System.Runtime.CompilerServices;

namespace NonBlocking
{
    /// <summary>
    /// Scalable counter base.
    /// </summary>
    public class CounterBase
    {
        private protected const int CACHE_LINE = 64;
        private protected const int OBJ_HEADER_SIZE = 8;

        private protected static readonly int s_MaxCellCount = Util.AlignToPowerOfTwo(Environment.ProcessorCount) + 1;

        // how many cells we have
        private protected int cellCount;

        // delayed count time
        private protected uint lastCntTicks;

        private protected CounterBase()
        {
            // touch static
            _ = s_MaxCellCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected unsafe static int GetIndex(uint cellCount)
        {
            if (IntPtr.Size == 4)
            {
                uint addr = (uint)&cellCount;
                return (int)(addr % cellCount);
            }
            else
            {
                ulong addr = (ulong)&cellCount;
                return (int)(addr % cellCount);
            }
        }
    }
}
