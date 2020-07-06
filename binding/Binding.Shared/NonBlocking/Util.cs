using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NonBlocking
{
    internal static class Util
    {
        // returns 2^x >= size
        internal static int AlignToPowerOfTwo(int size)
        {
            Debug.Assert(size > 0);

            size--;
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;
            return size + 1;
        }
    }
}
