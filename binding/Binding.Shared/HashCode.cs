// Partial code copied from:
// https://github.com/dotnet/runtime/blob/6072e4d3a7a2a1493f514cdf4be75a3d56580e84/src/libraries/System.Private.CoreLib/src/System/HashCode.cs

#nullable disable

using System;
using System.Runtime.CompilerServices;

#if HARFBUZZ
namespace HarfBuzzSharp
#else
namespace SkiaSharp
#endif
{
	internal unsafe struct HashCode
	{
		private static readonly uint s_seed = GenerateGlobalSeed ();

		private const uint Prime1 = 2654435761U;
		private const uint Prime2 = 2246822519U;
		private const uint Prime3 = 3266489917U;
		private const uint Prime4 = 668265263U;
		private const uint Prime5 = 374761393U;

		private uint _v1, _v2, _v3, _v4;
		private uint _queue1, _queue2, _queue3;
		private uint _length;

		private static unsafe uint GenerateGlobalSeed ()
		{
			var rnd = new Random ();
			var result = rnd.Next ();
			return unchecked((uint)result);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static void Initialize (out uint v1, out uint v2, out uint v3, out uint v4)
		{
			v1 = s_seed + Prime1 + Prime2;
			v2 = s_seed + Prime2;
			v3 = s_seed;
			v4 = s_seed - Prime1;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static uint Round (uint hash, uint input) =>
			RotateLeft (hash + input * Prime2, 13) * Prime1;

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static uint QueueRound (uint hash, uint queuedValue) =>
			RotateLeft (hash + queuedValue * Prime3, 17) * Prime4;

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static uint MixState (uint v1, uint v2, uint v3, uint v4) =>
			RotateLeft (v1, 1) + RotateLeft (v2, 7) + RotateLeft (v3, 12) + RotateLeft (v4, 18);

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static uint RotateLeft (uint value, int offset) =>
			(value << offset) | (value >> (32 - offset));

		private static uint MixEmptyState () =>
			s_seed + Prime5;

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static uint MixFinal (uint hash)
		{
			hash ^= hash >> 15;
			hash *= Prime2;
			hash ^= hash >> 13;
			hash *= Prime3;
			hash ^= hash >> 16;
			return hash;
		}

		public void Add (void* value) =>
			Add (value == null ? 0 : ((IntPtr)value).GetHashCode ());

		public void Add<T> (T value) =>
			Add (value?.GetHashCode () ?? 0);

		private void Add (int value)
		{
			uint val = (uint)value;

			// Storing the value of _length locally shaves of quite a few bytes
			// in the resulting machine code.
			uint previousLength = _length++;
			uint position = previousLength % 4;

			// Switch can't be inlined.

			if (position == 0)
				_queue1 = val;
			else if (position == 1)
				_queue2 = val;
			else if (position == 2)
				_queue3 = val;
			else // position == 3
			{
				if (previousLength == 3)
					Initialize (out _v1, out _v2, out _v3, out _v4);

				_v1 = Round (_v1, _queue1);
				_v2 = Round (_v2, _queue2);
				_v3 = Round (_v3, _queue3);
				_v4 = Round (_v4, val);
			}
		}

		public int ToHashCode ()
		{
			// Storing the value of _length locally shaves of quite a few bytes
			// in the resulting machine code.
			uint length = _length;

			// position refers to the *next* queue position in this method, so
			// position == 1 means that _queue1 is populated; _queue2 would have
			// been populated on the next call to Add.
			uint position = length % 4;

			// If the length is less than 4, _v1 to _v4 don't contain anything
			// yet. xxHash32 treats this differently.

			uint hash = length < 4 ? MixEmptyState () : MixState (_v1, _v2, _v3, _v4);

			// _length is incremented once per Add(Int32) and is therefore 4
			// times too small (xxHash length is in bytes, not ints).

			hash += length * 4;

			// Mix what remains in the queue

			// Switch can't be inlined right now, so use as few branches as
			// possible by manually excluding impossible scenarios (position > 1
			// is always false if position is not > 0).
			if (position > 0) {
				hash = QueueRound (hash, _queue1);
				if (position > 1) {
					hash = QueueRound (hash, _queue2);
					if (position > 2)
						hash = QueueRound (hash, _queue3);
				}
			}

			hash = MixFinal (hash);
			return (int)hash;
		}

		public static int Combine<T1> (T1 value1)
		{
			// Provide a way of diffusing bits from something with a limited
			// input hash space. For example, many enums only have a few
			// possible hashes, only using the bottom few bits of the code. Some
			// collections are built on the assumption that hashes are spread
			// over a larger space, so diffusing the bits may help the
			// collection work more efficiently.

			uint hc1 = (uint)(value1?.GetHashCode () ?? 0);

			uint hash = MixEmptyState ();
			hash += 4;

			hash = QueueRound (hash, hc1);

			hash = MixFinal (hash);
			return (int)hash;
		}

		public static int Combine<T1, T2> (T1 value1, T2 value2)
		{
			uint hc1 = (uint)(value1?.GetHashCode () ?? 0);
			uint hc2 = (uint)(value2?.GetHashCode () ?? 0);

			uint hash = MixEmptyState ();
			hash += 8;

			hash = QueueRound (hash, hc1);
			hash = QueueRound (hash, hc2);

			hash = MixFinal (hash);
			return (int)hash;
		}

		public static int Combine<T1, T2, T3> (T1 value1, T2 value2, T3 value3)
		{
			uint hc1 = (uint)(value1?.GetHashCode () ?? 0);
			uint hc2 = (uint)(value2?.GetHashCode () ?? 0);
			uint hc3 = (uint)(value3?.GetHashCode () ?? 0);

			uint hash = MixEmptyState ();
			hash += 12;

			hash = QueueRound (hash, hc1);
			hash = QueueRound (hash, hc2);
			hash = QueueRound (hash, hc3);

			hash = MixFinal (hash);
			return (int)hash;
		}

		public static int Combine<T1, T2, T3, T4> (T1 value1, T2 value2, T3 value3, T4 value4)
		{
			uint hc1 = (uint)(value1?.GetHashCode () ?? 0);
			uint hc2 = (uint)(value2?.GetHashCode () ?? 0);
			uint hc3 = (uint)(value3?.GetHashCode () ?? 0);
			uint hc4 = (uint)(value4?.GetHashCode () ?? 0);

			Initialize (out uint v1, out uint v2, out uint v3, out uint v4);

			v1 = Round (v1, hc1);
			v2 = Round (v2, hc2);
			v3 = Round (v3, hc3);
			v4 = Round (v4, hc4);

			uint hash = MixState (v1, v2, v3, v4);
			hash += 16;

			hash = MixFinal (hash);
			return (int)hash;
		}

		public static int Combine<T1, T2, T3, T4, T5> (T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
		{
			uint hc1 = (uint)(value1?.GetHashCode () ?? 0);
			uint hc2 = (uint)(value2?.GetHashCode () ?? 0);
			uint hc3 = (uint)(value3?.GetHashCode () ?? 0);
			uint hc4 = (uint)(value4?.GetHashCode () ?? 0);
			uint hc5 = (uint)(value5?.GetHashCode () ?? 0);

			Initialize (out uint v1, out uint v2, out uint v3, out uint v4);

			v1 = Round (v1, hc1);
			v2 = Round (v2, hc2);
			v3 = Round (v3, hc3);
			v4 = Round (v4, hc4);

			uint hash = MixState (v1, v2, v3, v4);
			hash += 20;

			hash = QueueRound (hash, hc5);

			hash = MixFinal (hash);
			return (int)hash;
		}

		public static int Combine<T1, T2, T3, T4, T5, T6> (T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
		{
			uint hc1 = (uint)(value1?.GetHashCode () ?? 0);
			uint hc2 = (uint)(value2?.GetHashCode () ?? 0);
			uint hc3 = (uint)(value3?.GetHashCode () ?? 0);
			uint hc4 = (uint)(value4?.GetHashCode () ?? 0);
			uint hc5 = (uint)(value5?.GetHashCode () ?? 0);
			uint hc6 = (uint)(value6?.GetHashCode () ?? 0);

			Initialize (out uint v1, out uint v2, out uint v3, out uint v4);

			v1 = Round (v1, hc1);
			v2 = Round (v2, hc2);
			v3 = Round (v3, hc3);
			v4 = Round (v4, hc4);

			uint hash = MixState (v1, v2, v3, v4);
			hash += 24;

			hash = QueueRound (hash, hc5);
			hash = QueueRound (hash, hc6);

			hash = MixFinal (hash);
			return (int)hash;
		}

		public static int Combine<T1, T2, T3, T4, T5, T6, T7> (T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
		{
			uint hc1 = (uint)(value1?.GetHashCode () ?? 0);
			uint hc2 = (uint)(value2?.GetHashCode () ?? 0);
			uint hc3 = (uint)(value3?.GetHashCode () ?? 0);
			uint hc4 = (uint)(value4?.GetHashCode () ?? 0);
			uint hc5 = (uint)(value5?.GetHashCode () ?? 0);
			uint hc6 = (uint)(value6?.GetHashCode () ?? 0);
			uint hc7 = (uint)(value7?.GetHashCode () ?? 0);

			Initialize (out uint v1, out uint v2, out uint v3, out uint v4);

			v1 = Round (v1, hc1);
			v2 = Round (v2, hc2);
			v3 = Round (v3, hc3);
			v4 = Round (v4, hc4);

			uint hash = MixState (v1, v2, v3, v4);
			hash += 28;

			hash = QueueRound (hash, hc5);
			hash = QueueRound (hash, hc6);
			hash = QueueRound (hash, hc7);

			hash = MixFinal (hash);
			return (int)hash;
		}

		public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8> (T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
		{
			uint hc1 = (uint)(value1?.GetHashCode () ?? 0);
			uint hc2 = (uint)(value2?.GetHashCode () ?? 0);
			uint hc3 = (uint)(value3?.GetHashCode () ?? 0);
			uint hc4 = (uint)(value4?.GetHashCode () ?? 0);
			uint hc5 = (uint)(value5?.GetHashCode () ?? 0);
			uint hc6 = (uint)(value6?.GetHashCode () ?? 0);
			uint hc7 = (uint)(value7?.GetHashCode () ?? 0);
			uint hc8 = (uint)(value8?.GetHashCode () ?? 0);

			Initialize (out uint v1, out uint v2, out uint v3, out uint v4);

			v1 = Round (v1, hc1);
			v2 = Round (v2, hc2);
			v3 = Round (v3, hc3);
			v4 = Round (v4, hc4);

			v1 = Round (v1, hc5);
			v2 = Round (v2, hc6);
			v3 = Round (v3, hc7);
			v4 = Round (v4, hc8);

			uint hash = MixState (v1, v2, v3, v4);
			hash += 32;

			hash = MixFinal (hash);
			return (int)hash;
		}
	}
}
