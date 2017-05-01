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
				blob = new Blob(memoryBase, (uint)size, MemoryMode.ReadOnly, asset, p => ((SKStreamAsset)p).Dispose());
			}
			else
			{
				var ptr = Marshal.AllocCoTaskMem(size);
				asset.Read(ptr, size);
				blob = new Blob(ptr, (uint)size, MemoryMode.ReadOnly, ptr, p => Marshal.FreeCoTaskMem((IntPtr)p));
			}

			blob.MakeImmutable();

			return blob;
		}
	}
}
