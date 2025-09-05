using System;
using System.Runtime.InteropServices;

using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	/// <summary>
	/// Various extension methods to integrate SkiaSharp and a HarfBuzz <see cref="T:HarfBuzzSharp.Blob" />.
	/// </summary>
	public static class BlobExtensions
	{
		/// <summary>
		/// Converts a seekable stream into a <see cref="T:HarfBuzzSharp.Blob" />.
		/// </summary>
		/// <param name="asset">The stream to convert into a <see cref="T:HarfBuzzSharp.Blob" />.</param>
		/// <returns>Returns the new <see cref="T:HarfBuzzSharp.Blob" /> instance.</returns>
		public static Blob ToHarfBuzzBlob(this SKStreamAsset asset)
		{
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}

			var size = asset.Length;

			Blob blob;

			var memoryBase = asset.GetMemoryBase();
			if (memoryBase != IntPtr.Zero)
			{
				blob = new Blob(memoryBase, size, MemoryMode.ReadOnly, () => asset.Dispose());
			}
			else
			{
				var ptr = Marshal.AllocCoTaskMem(size);
				asset.Read(ptr, size);
				blob = new Blob(ptr, size, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem(ptr));
			}

			blob.MakeImmutable();

			return blob;
		}
	}
}
