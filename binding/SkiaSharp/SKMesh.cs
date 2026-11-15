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

/// <summary>Builds an <see cref="T:SkiaSharp.SKMesh" /> from a specification, buffers, uniforms and child shaders.</summary>
/// <remarks />
public class SKMeshBuilder : IDisposable
{
	private readonly bool ownsSpecification;

	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMeshBuilder" /> class.</summary>
	/// <param name="specification">The specification describing the mesh attributes, varyings and programs.</param>
	/// <remarks>Disposing a builder created with this constructor does not dispose <paramref name="specification" />.</remarks>
	public SKMeshBuilder (SKMeshSpecification specification)
		: this (specification, false)
	{
	}

	internal SKMeshBuilder (SKMeshSpecification specification, bool ownsSpecification)
	{
		Specification = specification ?? throw new ArgumentNullException (nameof (specification));
		this.ownsSpecification = ownsSpecification;
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

	/// <summary>Gets the child shaders supplied to the mesh programs.</summary>
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

	/// <summary>Builds a non-indexed mesh from the supplied vertex buffer.</summary>
	/// <param name="vertexBuffer">The buffer holding the vertex data.</param>
	/// <param name="vertexCount">The number of vertices to read.</param>
	/// <param name="vertexOffset">The byte offset of the first vertex.</param>
	/// <returns>The mesh, or <see langword="null" /> if it could not be built.</returns>
	/// <remarks />
	public SKMesh? Build (SKMeshVertexBuffer vertexBuffer, int vertexCount, int vertexOffset) =>
		Build (vertexBuffer, vertexCount, vertexOffset, out _);

	/// <summary>Builds a non-indexed mesh from the supplied vertex buffer, reporting any validation errors.</summary>
	/// <param name="vertexBuffer">The buffer holding the vertex data.</param>
	/// <param name="vertexCount">The number of vertices to read.</param>
	/// <param name="vertexOffset">The byte offset of the first vertex.</param>
	/// <param name="errors">When this method returns, contains the message describing why creation failed, or <see langword="null" /> on success.</param>
	/// <returns>The mesh, or <see langword="null" /> if it could not be built.</returns>
	/// <remarks />
	public SKMesh? Build (SKMeshVertexBuffer vertexBuffer, int vertexCount, int vertexOffset, out string? errors) =>
		Specification.ToMesh (Mode, vertexBuffer, vertexCount, vertexOffset,
			Uniforms, Children, Bounds, out errors);

	/// <summary>Builds an indexed mesh from the supplied vertex and index buffers.</summary>
	/// <param name="vertexBuffer">The buffer holding the vertex data.</param>
	/// <param name="vertexCount">The number of vertices to read.</param>
	/// <param name="vertexOffset">The byte offset of the first vertex.</param>
	/// <param name="indexBuffer">The buffer holding the index data.</param>
	/// <param name="indexCount">The number of indices to read.</param>
	/// <param name="indexOffset">The byte offset of the first index.</param>
	/// <returns>The mesh, or <see langword="null" /> if it could not be built.</returns>
	/// <remarks />
	public SKMesh? BuildIndexed (
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset) =>
		BuildIndexed (vertexBuffer, vertexCount, vertexOffset, indexBuffer, indexCount, indexOffset, out _);

	/// <summary>Builds an indexed mesh from the supplied vertex and index buffers, reporting any validation errors.</summary>
	/// <param name="vertexBuffer">The buffer holding the vertex data.</param>
	/// <param name="vertexCount">The number of vertices to read.</param>
	/// <param name="vertexOffset">The byte offset of the first vertex.</param>
	/// <param name="indexBuffer">The buffer holding the index data.</param>
	/// <param name="indexCount">The number of indices to read.</param>
	/// <param name="indexOffset">The byte offset of the first index.</param>
	/// <param name="errors">When this method returns, contains the message describing why creation failed, or <see langword="null" /> on success.</param>
	/// <returns>The mesh, or <see langword="null" /> if it could not be built.</returns>
	/// <remarks />
	public SKMesh? BuildIndexed (
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset,
		out string? errors) =>
		Specification.ToMeshIndexed (Mode, vertexBuffer, vertexCount, vertexOffset,
			indexBuffer, indexCount, indexOffset, Uniforms, Children, Bounds, out errors);

	/// <summary>Releases all resources owned by the builder.</summary>
	/// <remarks />
	public void Dispose ()
	{
		Uniforms?.Dispose ();
		Children?.Dispose ();
		if (ownsSpecification)
			Specification?.Dispose ();
	}
}

/// <summary>The exception that is thrown when a mesh specification cannot be created.</summary>
/// <remarks />
public class SKMeshSpecificationException : ApplicationException
{
	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMeshSpecificationException" /> class.</summary>
	/// <param name="message">The message that describes the error.</param>
	/// <remarks />
	public SKMeshSpecificationException (string message)
		: base (message)
	{
	}
}
