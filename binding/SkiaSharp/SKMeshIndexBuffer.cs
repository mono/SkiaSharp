using System;

namespace SkiaSharp;

public unsafe class SKMeshIndexBuffer : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
{
	internal SKMeshIndexBuffer (IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	protected override void Dispose (bool disposing) =>
		base.Dispose (disposing);

	public static SKMeshIndexBuffer Make (ReadOnlySpan<byte> data)
	{
		fixed (byte* d = data)
		{
			return GetObject (SkiaApi.sk_mesh_index_buffer_make ((IntPtr)d, (IntPtr)data.Length));
		}
	}

	public static SKMeshIndexBuffer Make (IntPtr data, int size)
	{
		return GetObject (SkiaApi.sk_mesh_index_buffer_make (data, (IntPtr)size));
	}

	public static SKMeshIndexBuffer Make (int size)
	{
		return GetObject (SkiaApi.sk_mesh_index_buffer_make (IntPtr.Zero, (IntPtr)size));
	}

	public int Size => (int)SkiaApi.sk_mesh_index_buffer_get_size (Handle);

	internal static SKMeshIndexBuffer GetObject (IntPtr handle) =>
		handle == IntPtr.Zero ? null : new SKMeshIndexBuffer (handle, true);
}
