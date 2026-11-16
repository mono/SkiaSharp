using System;

namespace SkiaSharp;

/// <summary>Represents a custom vertex mesh that can be drawn with <see cref="T:SkiaSharp.SKCanvas" />.</summary>
/// <remarks>Meshes are built with <see cref="T:SkiaSharp.SKMeshBuilder" /> or <see cref="T:SkiaSharp.SKMeshSpecification" /> and only draw on GPU-backed surfaces.</remarks>
public unsafe class SKMesh : SKObject, ISKSkipObjectRegistration
{
	internal SKMesh (IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	/// <summary>Releases the unmanaged resources used by the mesh.</summary>
	/// <remarks />
	protected override void DisposeNative () =>
		SkiaApi.sk_mesh_delete (Handle);

	/// <summary>Gets a value indicating whether the mesh passed validation and can be drawn.</summary>
	/// <value><see langword="true" /> if the mesh is valid; otherwise <see langword="false" />.</value>
	/// <remarks />
	public bool IsValid => SkiaApi.sk_mesh_get_is_valid (Handle);

	/// <summary>Gets how the vertices are assembled into triangles.</summary>
	/// <value>The assembly mode of the mesh.</value>
	/// <remarks />
	public SKMeshMode Mode => SkiaApi.sk_mesh_get_mode (Handle);

	/// <summary>Gets the number of vertices read from the vertex buffer.</summary>
	/// <value>The vertex count.</value>
	/// <remarks />
	public int VertexCount => (int)SkiaApi.sk_mesh_get_vertex_count (Handle);

	/// <summary>Gets the byte offset of the first vertex in the vertex buffer.</summary>
	/// <value>The vertex byte offset.</value>
	/// <remarks />
	public int VertexOffset => (int)SkiaApi.sk_mesh_get_vertex_offset (Handle);

	/// <summary>Gets the number of indices read from the index buffer, or 0 for a non-indexed mesh.</summary>
	/// <value>The index count, or 0 if the mesh is non-indexed.</value>
	/// <remarks />
	public int IndexCount => (int)SkiaApi.sk_mesh_get_index_count (Handle);

	/// <summary>Gets the byte offset of the first index in the index buffer, or 0 for a non-indexed mesh.</summary>
	/// <value>The index byte offset.</value>
	/// <remarks />
	public int IndexOffset => (int)SkiaApi.sk_mesh_get_index_offset (Handle);

	/// <summary>Gets the bounds of the mesh geometry, used for culling.</summary>
	/// <value>The bounding rectangle of the mesh.</value>
	/// <remarks />
	public SKRect Bounds {
		get {
			SKRect bounds;
			SkiaApi.sk_mesh_get_bounds (Handle, &bounds);
			return bounds;
		}
	}

	internal static SKMesh? GetObject (IntPtr handle) =>
		handle == IntPtr.Zero ? null : new SKMesh (handle, true);
}

/// <summary>Builds an <see cref="T:SkiaSharp.SKMesh" /> from a specification, buffers, uniforms and child effects.</summary>
/// <remarks />
public class SKMeshBuilder : IDisposable
{
	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMeshBuilder" /> class.</summary>
	/// <param name="specification">The specification describing the mesh attributes, varyings and programs.</param>
	/// <remarks>Disposing a builder does not dispose <paramref name="specification" />.</remarks>
	public SKMeshBuilder (SKMeshSpecification specification)
	{
		Specification = specification ?? throw new ArgumentNullException (nameof (specification));
		Uniforms = new SKRuntimeEffectUniforms (specification);
		Children = new SKRuntimeEffectChildren (specification);
	}

	/// <summary>Gets the specification the mesh is built against.</summary>
	/// <value>The mesh specification.</value>
	/// <remarks />
	public SKMeshSpecification Specification { get; }

	/// <summary>Gets the uniform values supplied to the mesh programs.</summary>
	/// <value>The uniform collection declared by the specification.</value>
	/// <remarks />
	public SKRuntimeEffectUniforms Uniforms { get; }

	/// <summary>Gets the child effects supplied to the mesh programs.</summary>
	/// <value>The child slot collection declared by the specification.</value>
	/// <remarks />
	public SKRuntimeEffectChildren Children { get; }

	/// <summary>Gets or sets how the vertices are assembled into triangles.</summary>
	/// <value>The assembly mode. The default is <see cref="F:SkiaSharp.SKMeshMode.Triangles" />.</value>
	/// <remarks />
	public SKMeshMode Mode { get; set; } = SKMeshMode.Triangles;

	/// <summary>Gets or sets the bounds of the mesh, used for culling.</summary>
	/// <value>A rectangle that contains the drawn geometry.</value>
	/// <remarks>The bounds must contain the geometry the vertex program produces, otherwise the mesh may be culled incorrectly.</remarks>
	public SKRect Bounds { get; set; }

	/// <summary>Gets the vertex buffer configured on the builder.</summary>
	/// <value>The vertex buffer, or <see langword="null" /> if none has been set.</value>
	/// <remarks />
	public SKMeshVertexBuffer? VertexBuffer { get; private set; }

	/// <summary>Gets the number of vertices configured on the builder.</summary>
	/// <value>The vertex count.</value>
	/// <remarks />
	public int VertexCount { get; private set; }

	/// <summary>Gets the byte offset of the first vertex in <see cref="P:SkiaSharp.SKMeshBuilder.VertexBuffer" />.</summary>
	/// <value>The vertex byte offset.</value>
	/// <remarks />
	public int VertexOffset { get; private set; }

	/// <summary>Gets the optional index buffer configured on the builder.</summary>
	/// <value>The index buffer, or <see langword="null" /> for a non-indexed mesh.</value>
	/// <remarks />
	public SKMeshIndexBuffer? IndexBuffer { get; private set; }

	/// <summary>Gets the number of indices configured on the builder.</summary>
	/// <value>The index count, or 0 for a non-indexed mesh.</value>
	/// <remarks />
	public int IndexCount { get; private set; }

	/// <summary>Gets the byte offset of the first index in <see cref="P:SkiaSharp.SKMeshBuilder.IndexBuffer" />.</summary>
	/// <value>The index byte offset.</value>
	/// <remarks />
	public int IndexOffset { get; private set; }

	/// <summary>Configures the vertex buffer using all vertices in the buffer starting at byte offset 0.</summary>
	/// <param name="buffer">The buffer holding the vertex data.</param>
	/// <remarks />
	public void SetVertexBuffer (SKMeshVertexBuffer buffer)
	{
		if (buffer == null)
			throw new ArgumentNullException (nameof (buffer));

		SetVertexBuffer (buffer, buffer.Size / Specification.Stride, 0);
	}

	/// <summary>Configures the vertex buffer and vertex count starting at byte offset 0.</summary>
	/// <param name="buffer">The buffer holding the vertex data.</param>
	/// <param name="count">The number of vertices to read.</param>
	/// <remarks />
	public void SetVertexBuffer (SKMeshVertexBuffer buffer, int count) =>
		SetVertexBuffer (buffer, count, 0);

	/// <summary>Configures the vertex buffer, vertex count, and byte offset.</summary>
	/// <param name="buffer">The buffer holding the vertex data.</param>
	/// <param name="count">The number of vertices to read.</param>
	/// <param name="offset">The byte offset of the first vertex.</param>
	/// <remarks />
	public void SetVertexBuffer (SKMeshVertexBuffer buffer, int count, int offset)
	{
		if (buffer == null)
			throw new ArgumentNullException (nameof (buffer));
		if (count <= 0)
			throw new ArgumentOutOfRangeException (nameof (count));
		if (offset < 0)
			throw new ArgumentOutOfRangeException (nameof (offset));

		VertexBuffer = buffer;
		VertexCount = count;
		VertexOffset = offset;
	}

	/// <summary>Configures the index buffer using all 16-bit indices in the buffer starting at byte offset 0, or clears the index buffer when <paramref name="buffer" /> is <see langword="null" />.</summary>
	/// <param name="buffer">The buffer holding the index data, or <see langword="null" /> to build a non-indexed mesh.</param>
	/// <remarks />
	public void SetIndexBuffer (SKMeshIndexBuffer? buffer)
	{
		if (buffer == null) {
			IndexBuffer = null;
			IndexCount = 0;
			IndexOffset = 0;
			return;
		}

		SetIndexBuffer (buffer, buffer.Size / sizeof (ushort), 0);
	}

	/// <summary>Configures the index buffer and index count starting at byte offset 0.</summary>
	/// <param name="buffer">The buffer holding the index data.</param>
	/// <param name="count">The number of indices to read.</param>
	/// <remarks />
	public void SetIndexBuffer (SKMeshIndexBuffer buffer, int count) =>
		SetIndexBuffer (buffer, count, 0);

	/// <summary>Configures the index buffer, index count, and byte offset.</summary>
	/// <param name="buffer">The buffer holding the index data.</param>
	/// <param name="count">The number of indices to read.</param>
	/// <param name="offset">The byte offset of the first index.</param>
	/// <remarks />
	public void SetIndexBuffer (SKMeshIndexBuffer buffer, int count, int offset)
	{
		if (buffer == null)
			throw new ArgumentNullException (nameof (buffer));
		if (count <= 0)
			throw new ArgumentOutOfRangeException (nameof (count));
		if (offset < 0)
			throw new ArgumentOutOfRangeException (nameof (offset));

		IndexBuffer = buffer;
		IndexCount = count;
		IndexOffset = offset;
	}

	/// <summary>Builds a mesh using the configured vertex buffer, optional index buffer, uniforms, children, mode, and bounds.</summary>
	/// <returns>The mesh, or <see langword="null" /> if it could not be built.</returns>
	/// <remarks />
	public SKMesh? Build ()
	{
		if (VertexBuffer == null)
			throw new InvalidOperationException ("A vertex buffer must be set before building a mesh.");

		if (IndexBuffer != null) {
			return Specification.ToMeshIndexedCore (
				Mode,
				VertexBuffer,
				VertexCount,
				VertexOffset,
				IndexBuffer,
				IndexCount,
				IndexOffset,
				Uniforms,
				Children,
				Bounds,
				captureErrors: false,
				out _);
		}

		return Specification.ToMeshCore (
			Mode,
			VertexBuffer,
			VertexCount,
			VertexOffset,
			Uniforms,
			Children,
			Bounds,
			captureErrors: false,
			out _);
	}

	/// <summary>Builds a mesh using the configured vertex buffer, optional index buffer, uniforms, children, mode, and bounds, reporting any validation errors.</summary>
	/// <param name="errors">When this method returns, contains the message describing why creation failed, or <see langword="null" /> on success.</param>
	/// <returns>The mesh, or <see langword="null" /> if it could not be built.</returns>
	/// <remarks />
	public SKMesh? Build (out string? errors)
	{
		if (VertexBuffer == null)
			throw new InvalidOperationException ("A vertex buffer must be set before building a mesh.");

		if (IndexBuffer != null) {
			return Specification.ToMeshIndexed (
				Mode,
				VertexBuffer,
				VertexCount,
				VertexOffset,
				IndexBuffer,
				IndexCount,
				IndexOffset,
				Uniforms,
				Children,
				Bounds,
				out errors);
		}

		return Specification.ToMesh (
			Mode,
			VertexBuffer,
			VertexCount,
			VertexOffset,
			Uniforms,
			Children,
			Bounds,
			out errors);
	}

	/// <summary>Releases all resources owned by the builder.</summary>
	/// <remarks />
	public void Dispose ()
	{
		Uniforms?.Dispose ();
		Children?.Dispose ();
	}
}
