using System;

namespace SkiaSharp;

/// <summary>Represents a GPU-resident buffer holding the index data of a mesh.</summary>
/// <remarks />
public unsafe class SKMeshIndexBuffer : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
{
	internal SKMeshIndexBuffer (IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	/// <summary>Releases the unmanaged resources used by the buffer and optionally releases the managed resources.</summary>
	/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
	/// <remarks />
	protected override void Dispose (bool disposing) =>
		base.Dispose (disposing);

	/// <summary>Creates a index buffer containing a copy of the supplied data.</summary>
	/// <param name="data">The bytes to copy into the buffer.</param>
	/// <returns>The new buffer, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshIndexBuffer? Make (ReadOnlySpan<byte> data)
	{
		fixed (byte* d = data)
		{
			return GetObject (SkiaApi.sk_mesh_index_buffer_make ((IntPtr)d, (IntPtr)data.Length));
		}
	}

	/// <summary>Creates a index buffer containing a copy of <paramref name="size" /> bytes read from <paramref name="data" />.</summary>
	/// <param name="data">A pointer to the bytes to copy.</param>
	/// <param name="size">The number of bytes to copy.</param>
	/// <returns>The new buffer, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshIndexBuffer? Make (IntPtr data, int size)
	{
		return GetObject (SkiaApi.sk_mesh_index_buffer_make (data, (IntPtr)size));
	}

	/// <summary>Creates an uninitialized index buffer of the specified size.</summary>
	/// <param name="size">The size of the buffer, in bytes.</param>
	/// <returns>The new buffer, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshIndexBuffer? Make (int size)
	{
		return GetObject (SkiaApi.sk_mesh_index_buffer_make (IntPtr.Zero, (IntPtr)size));
	}

	/// <summary>Gets the size of the buffer, in bytes.</summary>
	/// <value>The size of the buffer, in bytes.</value>
	/// <remarks />
	public int Size => (int)SkiaApi.sk_mesh_index_buffer_get_size (Handle);

	internal static SKMeshIndexBuffer? GetObject (IntPtr handle) =>
		handle == IntPtr.Zero ? null : new SKMeshIndexBuffer (handle, true);
}
