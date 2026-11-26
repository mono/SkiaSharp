using System;

namespace SkiaSharp;

/// <summary>Represents a custom vertex mesh that can be drawn with <see cref="T:SkiaSharp.SKCanvas" />.</summary>
/// <remarks>Meshes are built with <see cref="T:SkiaSharp.SKMeshBuilder" /> or <see cref="T:SkiaSharp.SKMeshSpecification" /> and only draw on GPU-backed surfaces.</remarks>
public unsafe partial class SKMesh : SKObject, ISKSkipObjectRegistration
{
	internal SKMesh (IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	/// <summary>Releases the unmanaged resources used by the mesh.</summary>
	/// <remarks />
	protected override void DisposeNative () =>
		SkiaApi.sk_mesh_delete (Handle);

	/// <summary>Creates a builder for a non-indexed <see cref="T:SkiaSharp.SKMesh" /> using the specified specification.</summary>
	/// <param name="specification">The specification describing the mesh attributes, varyings and programs.</param>
	/// <returns>A new mesh builder.</returns>
	/// <remarks />
	public static SKMeshBuilder CreateBuilder (SKMeshSpecification specification) =>
		new SKMeshBuilder (specification);

	/// <summary>Creates a builder for an indexed <see cref="T:SkiaSharp.SKMesh" /> using the specified specification.</summary>
	/// <param name="specification">The specification describing the mesh attributes, varyings and programs.</param>
	/// <returns>A new indexed mesh builder.</returns>
	/// <remarks />
	public static SKIndexedMeshBuilder CreateIndexedBuilder (SKMeshSpecification specification) =>
		new SKIndexedMeshBuilder (specification);

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
