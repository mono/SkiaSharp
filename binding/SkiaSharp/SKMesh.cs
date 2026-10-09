using System;

namespace SkiaSharp;

/// <summary>Represents a custom vertex mesh that can be drawn with <see cref="T:SkiaSharp.SKCanvas" />.</summary>
/// <remarks>Meshes are built with <see cref="T:SkiaSharp.SKMeshBuilder" /> and only draw on GPU-backed surfaces.</remarks>
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

	internal static SKMesh? GetObject (IntPtr handle) =>
		handle == IntPtr.Zero ? null : new SKMesh (handle, true);
}

/// <summary>Builds an <see cref="T:SkiaSharp.SKMesh" /> from a specification, buffers, uniforms and child shaders.</summary>
/// <remarks />
public class SKMeshBuilder : IDisposable
{
	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMeshBuilder" /> class.</summary>
	/// <param name="specification">The specification describing the mesh attributes, varyings and programs.</param>
	/// <remarks />
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
	public SKMesh? Build (SKMeshVertexBuffer vertexBuffer, int vertexCount, int vertexOffset)
	{
		return Specification.ToMesh (Mode, vertexBuffer, vertexCount, vertexOffset,
			Uniforms, Children, Bounds);
	}

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
		int indexOffset)
	{
		return Specification.ToMeshIndexed (Mode, vertexBuffer, vertexCount, vertexOffset,
			indexBuffer, indexCount, indexOffset, Uniforms, Children, Bounds);
	}

	/// <summary>Releases all resources used by the builder.</summary>
	/// <remarks />
	public void Dispose ()
	{
		Uniforms?.Dispose ();
		Children?.Dispose ();
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
