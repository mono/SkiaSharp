using System;

namespace SkiaSharp;

public unsafe class SKMesh : SKObject, ISKSkipObjectRegistration
{
	internal SKMesh (IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	protected override void DisposeNative () =>
		SkiaApi.sk_mesh_delete (Handle);

	public bool IsValid => SkiaApi.sk_mesh_get_is_valid (Handle);

	internal static SKMesh GetObject (IntPtr handle) =>
		handle == IntPtr.Zero ? null : new SKMesh (handle, true);
}

public class SKMeshBuilder : IDisposable
{
	public SKMeshBuilder (SKMeshSpecification specification)
	{
		Specification = specification ?? throw new ArgumentNullException (nameof (specification));
		Uniforms = new SKRuntimeEffectUniforms (specification);
		Children = new SKRuntimeEffectChildren (specification);
	}

	public SKMeshSpecification Specification { get; }

	public SKRuntimeEffectUniforms Uniforms { get; }

	public SKRuntimeEffectChildren Children { get; }

	public SKMeshMode Mode { get; set; } = SKMeshMode.Triangles;

	public SKRect Bounds { get; set; }

	public SKMesh Build (SKMeshVertexBuffer vertexBuffer, int vertexCount, int vertexOffset)
	{
		return Specification.ToMesh (Mode, vertexBuffer, vertexCount, vertexOffset,
			Uniforms, Children, Bounds);
	}

	public SKMesh BuildIndexed (
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

	public void Dispose ()
	{
		Uniforms?.Dispose ();
		Children?.Dispose ();
		Specification?.Dispose ();
	}
}

public class SKMeshSpecificationException : ApplicationException
{
	public SKMeshSpecificationException (string message)
		: base (message)
	{
	}
}
