using System;

namespace SkiaSharp;

/// <summary>Builds an indexed <see cref="T:SkiaSharp.SKMesh" /> from a specification, vertex buffer, index buffer, uniforms and child effects.</summary>
/// <remarks />
public partial class SKIndexedMeshBuilder : SKMeshBuilder
{
	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKIndexedMeshBuilder" /> class.</summary>
	/// <param name="specification">The specification describing the mesh attributes, varyings and programs.</param>
	/// <remarks>Disposing a builder does not dispose <paramref name="specification" />.</remarks>
	public SKIndexedMeshBuilder (SKMeshSpecification specification)
		: base (specification)
	{
	}

	/// <summary>Gets the index buffer configured on the builder.</summary>
	/// <value>The index buffer, or <see langword="null" /> if none has been set.</value>
	/// <remarks />
	public SKMeshIndexBuffer? IndexBuffer { get; private set; }

	/// <summary>Gets the number of indices configured on the builder.</summary>
	/// <value>The index count.</value>
	/// <remarks />
	public int IndexCount { get; private set; }

	/// <summary>Gets the byte offset of the first index in <see cref="P:SkiaSharp.SKIndexedMeshBuilder.IndexBuffer" />.</summary>
	/// <value>The index byte offset.</value>
	/// <remarks />
	public int IndexOffset { get; private set; }

	/// <summary>Configures the index buffer using all 16-bit indices in the buffer starting at byte offset 0.</summary>
	/// <param name="buffer">The buffer holding the index data.</param>
	/// <remarks />
	public void SetIndexBuffer (SKMeshIndexBuffer buffer)
	{
		if (buffer == null)
			throw new ArgumentNullException (nameof (buffer));

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

	/// <summary>Builds an indexed mesh using the configured vertex buffer, index buffer, uniforms, children, mode, and bounds.</summary>
	/// <returns>The mesh, or <see langword="null" /> if it could not be built.</returns>
	/// <remarks />
	public override SKMesh? Build ()
	{
		if (VertexBuffer == null)
			throw new InvalidOperationException ("A vertex buffer must be set before building a mesh.");
		if (IndexBuffer == null)
			throw new InvalidOperationException ("An index buffer must be set before building an indexed mesh.");

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

	/// <summary>Builds an indexed mesh using the configured vertex buffer, index buffer, uniforms, children, mode, and bounds, reporting any validation errors.</summary>
	/// <param name="errors">When this method returns, contains the message describing why creation failed, or <see langword="null" /> on success.</param>
	/// <returns>The mesh, or <see langword="null" /> if it could not be built.</returns>
	/// <remarks />
	public override SKMesh? Build (out string? errors)
	{
		if (VertexBuffer == null)
			throw new InvalidOperationException ("A vertex buffer must be set before building a mesh.");
		if (IndexBuffer == null)
			throw new InvalidOperationException ("An index buffer must be set before building an indexed mesh.");

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
}
