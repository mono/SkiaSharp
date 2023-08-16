using System;
using System.Runtime.InteropServices;

using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	public static class BlobExtensions
	{
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
