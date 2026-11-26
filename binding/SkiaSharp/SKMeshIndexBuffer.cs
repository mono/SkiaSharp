using System;

namespace SkiaSharp;

/// <summary>Represents a CPU- or GPU-backed buffer holding the index data of a mesh.</summary>
/// <remarks />
public unsafe partial class SKMeshIndexBuffer : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
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

	/// <summary>Creates a CPU-backed index buffer containing a copy of the supplied data.</summary>
	/// <param name="data">The bytes to copy into the buffer.</param>
	/// <returns>The new buffer, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshIndexBuffer? Create (ReadOnlySpan<byte> data)
	{
		if (data.IsEmpty)
			throw new ArgumentException ("Data cannot be empty.", nameof (data));

		fixed (byte* d = data)
		{
			return Create ((IntPtr)d, data.Length);
		}
	}

	/// <summary>Creates a GPU-backed index buffer containing a copy of the supplied data.</summary>
	/// <param name="context">The GPU context to upload the buffer to.</param>
	/// <param name="data">The bytes to copy into the buffer.</param>
	/// <returns>The new buffer, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshIndexBuffer? Create (GRContext context, ReadOnlySpan<byte> data)
	{
		if (context == null)
			throw new ArgumentNullException (nameof (context));
		if (data.IsEmpty)
			throw new ArgumentException ("Data cannot be empty.", nameof (data));

		fixed (byte* d = data)
		{
			return Create (context, (IntPtr)d, data.Length);
		}
	}

	/// <summary>Creates a CPU-backed index buffer containing a copy of <paramref name="size" /> bytes read from <paramref name="data" />.</summary>
	/// <param name="data">A pointer to the bytes to copy.</param>
	/// <param name="size">The number of bytes to copy.</param>
	/// <returns>The new buffer, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshIndexBuffer? Create (IntPtr data, int size)
	{
		if (data == IntPtr.Zero)
			throw new ArgumentNullException (nameof (data));
		if (size <= 0)
			throw new ArgumentOutOfRangeException (nameof (size));

		return CreateCore (null, (void*)data, size);
	}

	/// <summary>Creates a GPU-backed index buffer containing a copy of <paramref name="size" /> bytes read from <paramref name="data" />.</summary>
	/// <param name="context">The GPU context to upload the buffer to.</param>
	/// <param name="data">A pointer to the bytes to copy.</param>
	/// <param name="size">The number of bytes to copy.</param>
	/// <returns>The new buffer, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshIndexBuffer? Create (GRContext context, IntPtr data, int size)
	{
		if (context == null)
			throw new ArgumentNullException (nameof (context));
		if (data == IntPtr.Zero)
			throw new ArgumentNullException (nameof (data));
		if (size <= 0)
			throw new ArgumentOutOfRangeException (nameof (size));

		return CreateCore (context, (void*)data, size);
	}

	/// <summary>Creates a zero-initialized CPU-backed index buffer of the specified size.</summary>
	/// <param name="size">The size of the buffer, in bytes.</param>
	/// <returns>The new buffer, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshIndexBuffer? Create (int size)
	{
		if (size <= 0)
			throw new ArgumentOutOfRangeException (nameof (size));

		return CreateCore (null, null, size);
	}

	/// <summary>Creates a zero-initialized GPU-backed index buffer of the specified size.</summary>
	/// <param name="context">The GPU context to allocate the buffer on.</param>
	/// <param name="size">The size of the buffer, in bytes.</param>
	/// <returns>The new buffer, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshIndexBuffer? Create (GRContext context, int size)
	{
		if (context == null)
			throw new ArgumentNullException (nameof (context));
		if (size <= 0)
			throw new ArgumentOutOfRangeException (nameof (size));

		return CreateCore (context, null, size);
	}

	private static SKMeshIndexBuffer? CreateCore (GRContext? context, void* data, int size)
	{
		if (context == null)
			return GetObject (SkiaApi.sk_mesh_index_buffer_make (data, (IntPtr)size));

		var result = GetObject (SkiaApi.sk_mesh_index_buffer_make_ganesh (context.Handle, data, (IntPtr)size));
		GC.KeepAlive (context);
		return result;
	}

	/// <summary>Creates a CPU-backed copy of this index buffer.</summary>
	/// <returns>The copied buffer, or <see langword="null" /> if the buffer could not be copied.</returns>
	/// <remarks>Copying is only supported when the source buffer is CPU-backed.</remarks>
	public SKMeshIndexBuffer? Copy ()
	{
		var cpuResult = GetObject (SkiaApi.sk_mesh_index_buffer_copy (Handle));
		GC.KeepAlive (this);
		return cpuResult;
	}

	/// <summary>Creates a GPU-backed copy of this index buffer.</summary>
	/// <param name="context">The GPU context to upload the copy to.</param>
	/// <returns>The copied buffer, or <see langword="null" /> if the buffer could not be copied.</returns>
	/// <remarks>Copying is only supported when the source buffer is CPU-backed.</remarks>
	public SKMeshIndexBuffer? Copy (GRContext context)
	{
		if (context == null)
			throw new ArgumentNullException (nameof (context));

		var result = GetObject (SkiaApi.sk_mesh_index_buffer_copy_ganesh (context.Handle, Handle));
		GC.KeepAlive (context);
		GC.KeepAlive (this);
		return result;
	}

	/// <summary>Updates a region of a CPU-backed index buffer with the supplied bytes.</summary>
	/// <param name="data">The bytes to copy into the buffer.</param>
	/// <param name="offset">The byte offset in the buffer, which must be a multiple of 4.</param>
	/// <returns><see langword="true" /> if the update succeeded; otherwise <see langword="false" />.</returns>
	/// <remarks>Both <paramref name="offset" /> and <paramref name="data" /> length must be multiples of 4 bytes.</remarks>
	public bool Update (ReadOnlySpan<byte> data, int offset)
	{
		if (data.IsEmpty)
			throw new ArgumentException ("Data cannot be empty.", nameof (data));

		fixed (byte* d = data)
		{
			return Update ((IntPtr)d, offset, data.Length);
		}
	}

	/// <summary>Updates a region of a GPU-backed index buffer with the supplied bytes.</summary>
	/// <param name="context">The GPU context that created the buffer.</param>
	/// <param name="data">The bytes to copy into the buffer.</param>
	/// <param name="offset">The byte offset in the buffer, which must be a multiple of 4.</param>
	/// <returns><see langword="true" /> if the update succeeded; otherwise <see langword="false" />.</returns>
	/// <remarks>Both <paramref name="offset" /> and <paramref name="data" /> length must be multiples of 4 bytes.</remarks>
	public bool Update (GRContext context, ReadOnlySpan<byte> data, int offset)
	{
		if (context == null)
			throw new ArgumentNullException (nameof (context));
		if (data.IsEmpty)
			throw new ArgumentException ("Data cannot be empty.", nameof (data));

		fixed (byte* d = data)
		{
			return Update (context, (IntPtr)d, offset, data.Length);
		}
	}

	/// <summary>Updates a region of a CPU-backed index buffer with <paramref name="size" /> bytes read from <paramref name="data" />.</summary>
	/// <param name="data">A pointer to the bytes to copy.</param>
	/// <param name="offset">The byte offset in the buffer, which must be a multiple of 4.</param>
	/// <param name="size">The number of bytes to copy, which must be a multiple of 4.</param>
	/// <returns><see langword="true" /> if the update succeeded; otherwise <see langword="false" />.</returns>
	/// <remarks />
	public bool Update (IntPtr data, int offset, int size)
	{
		if (data == IntPtr.Zero)
			throw new ArgumentNullException (nameof (data));
		if (offset < 0)
			throw new ArgumentOutOfRangeException (nameof (offset));
		if (size <= 0)
			throw new ArgumentOutOfRangeException (nameof (size));

		return UpdateCore (null, (void*)data, offset, size);
	}

	/// <summary>Updates a region of a GPU-backed index buffer with <paramref name="size" /> bytes read from <paramref name="data" />.</summary>
	/// <param name="context">The GPU context that created the buffer.</param>
	/// <param name="data">A pointer to the bytes to copy.</param>
	/// <param name="offset">The byte offset in the buffer, which must be a multiple of 4.</param>
	/// <param name="size">The number of bytes to copy, which must be a multiple of 4.</param>
	/// <returns><see langword="true" /> if the update succeeded; otherwise <see langword="false" />.</returns>
	/// <remarks />
	public bool Update (GRContext context, IntPtr data, int offset, int size)
	{
		if (context == null)
			throw new ArgumentNullException (nameof (context));
		if (data == IntPtr.Zero)
			throw new ArgumentNullException (nameof (data));
		if (offset < 0)
			throw new ArgumentOutOfRangeException (nameof (offset));
		if (size <= 0)
			throw new ArgumentOutOfRangeException (nameof (size));

		return UpdateCore (context, (void*)data, offset, size);
	}

	private bool UpdateCore (GRContext? context, void* data, int offset, int size)
	{
		var result = SkiaApi.sk_mesh_index_buffer_update (Handle, context?.Handle ?? IntPtr.Zero, data, (IntPtr)offset, (IntPtr)size);
		GC.KeepAlive (context);
		GC.KeepAlive (this);
		return result;
	}

	/// <summary>Gets the size of the buffer, in bytes.</summary>
	/// <value>The size of the buffer, in bytes.</value>
	/// <remarks />
	public int Size => (int)SkiaApi.sk_mesh_index_buffer_get_size (Handle);

	internal static SKMeshIndexBuffer? GetObject (IntPtr handle) =>
		handle == IntPtr.Zero ? null : new SKMeshIndexBuffer (handle, true);
}
