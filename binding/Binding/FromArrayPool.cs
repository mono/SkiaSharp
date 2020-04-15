using System;
using System.Buffers;

namespace SkiaSharp
{
	internal static class FromArrayPool
	{
		/// <summary>
		/// RAII helper struct for the scope of a rented array from ArrayPool.
		/// </summary>
		internal readonly struct Scope<T> : IDisposable
		{
			internal Scope (T[] items)
			{
				Items = items;
			}

			public readonly T[] Items;

			public void Dispose () => ArrayPool<T>.Shared.Return (Items);
		}

		public static Scope<T> Rent<T> (int length, out Span<T> span)
		{
			var mem = ArrayPool<T>.Shared.Rent (length);
			span = mem.AsSpan (0, length);
			return new Scope<T> (mem);
		}
	}
}
