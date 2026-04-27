using System;

namespace SkiaSharp;

public unsafe class SKMeshVertexBuffer : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
{
	internal SKMeshVertexBuffer (IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	protected override void Dispose (bool disposing) =>
		base.Dispose (disposing);

	public static SKMeshVertexBuffer Make (ReadOnlySpan<byte> data)
	{
		fixed (byte* d = data)
		{
			return GetObject (SkiaApi.sk_mesh_vertex_buffer_make ((IntPtr)d, (IntPtr)data.Length));
		}
	}

	public static SKMeshVertexBuffer Make (IntPtr data, int size)
	{
		return GetObject (SkiaApi.sk_mesh_vertex_buffer_make (data, (IntPtr)size));
	}

	public static SKMeshVertexBuffer Make (int size)
	{
		return GetObject (SkiaApi.sk_mesh_vertex_buffer_make (IntPtr.Zero, (IntPtr)size));
	}

	public int Size => (int)SkiaApi.sk_mesh_vertex_buffer_get_size (Handle);

	internal static SKMeshVertexBuffer GetObject (IntPtr handle) =>
		handle == IntPtr.Zero ? null : new SKMeshVertexBuffer (handle, true);
}
