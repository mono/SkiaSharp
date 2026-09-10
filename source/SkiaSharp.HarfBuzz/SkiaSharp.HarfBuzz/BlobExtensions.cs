using System;
using System.Runtime.InteropServices;

using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	/// <summary>Various extension methods to integrate SkiaSharp and a HarfBuzz <see cref="T:HarfBuzzSharp.Blob" />.</summary>
	/// <remarks />
	public static class BlobExtensions
	{
		/// <param name="asset">The stream to convert into a <see cref="T:HarfBuzzSharp.Blob" />.</param>
		/// <summary>Converts a seekable stream into a <see cref="T:HarfBuzzSharp.Blob" />.</summary>
		/// <returns>Returns the new <see cref="T:HarfBuzzSharp.Blob" /> instance.</returns>
		/// <remarks />
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
				blob = new Blob(ptr, size, MemoryMode.ReadOnly, () =>
				{
					Marshal.FreeCoTaskMem(ptr);
					asset.Dispose();
				});
			}

			blob.MakeImmutable();

			return blob;
		}
	}
}
