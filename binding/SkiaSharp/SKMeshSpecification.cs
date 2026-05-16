using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp;

public unsafe class SKMeshSpecification : SKObject, ISKNonVirtualReferenceCounted, ISKSkipObjectRegistration
{
	private IReadOnlyList<string> uniformNames;
	private IReadOnlyList<string> childNames;

	internal SKMeshSpecification (IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	protected override void Dispose (bool disposing) =>
		base.Dispose (disposing);

	void ISKNonVirtualReferenceCounted.ReferenceNative () => SkiaApi.sk_meshspecification_ref (Handle);

	void ISKNonVirtualReferenceCounted.UnreferenceNative () => SkiaApi.sk_meshspecification_unref (Handle);

	// Create

	public static SKMeshSpecification? Create (
		ReadOnlySpan<SKMeshSpecificationAttribute> attributes,
		int vertexStride,
		ReadOnlySpan<SKMeshSpecificationVarying> varyings,
		string vertexShader,
		string fragmentShader,
		out string? errors)
	{
		return Create (attributes, vertexStride, varyings, vertexShader, fragmentShader, null, SKAlphaType.Premul, out errors);
	}

	public static SKMeshSpecification? Create (
		ReadOnlySpan<SKMeshSpecificationAttribute> attributes,
		int vertexStride,
		ReadOnlySpan<SKMeshSpecificationVarying> varyings,
		string vertexShader,
		string fragmentShader,
		SKColorSpace? colorSpace,
		SKAlphaType alphaType,
		out string? errors)
	{
		if (vertexShader == null)
			throw new ArgumentNullException (nameof (vertexShader));
		if (fragmentShader == null)
			throw new ArgumentNullException (nameof (fragmentShader));
		if (attributes.Length == 0)
			throw new ArgumentException ("At least one attribute is required.", nameof (attributes));

		using var vs = new SKString (vertexShader);
		using var fs = new SKString (fragmentShader);
		using var errorString = new SKString ();

		var nativeAttrs = stackalloc SKMeshSpecificationAttributeNative[attributes.Length];
		var pinnedAttrNames = new byte*[attributes.Length];
		var attrNamePins = new System.Buffers.MemoryHandle[attributes.Length];

		try
		{
			for (int i = 0; i < attributes.Length; i++)
			{
				var nameBytes = StringUtilities.GetEncodedText (attributes[i].Name ?? string.Empty, SKTextEncoding.Utf8, true);
				var pin = nameBytes.AsMemory ().Pin ();
				attrNamePins[i] = pin;
				pinnedAttrNames[i] = (byte*)pin.Pointer;

				nativeAttrs[i] = new SKMeshSpecificationAttributeNative
				{
					fType = attributes[i].Type,
					fOffset = (IntPtr)attributes[i].Offset,
					fName = pinnedAttrNames[i],
				};
			}

			var nativeVaryings = stackalloc SKMeshSpecificationVaryingNative[varyings.Length];
			var pinnedVaryingNames = new byte*[varyings.Length];
			var varyingNamePins = new System.Buffers.MemoryHandle[varyings.Length];

			try
			{
				for (int i = 0; i < varyings.Length; i++)
				{
					var nameBytes = StringUtilities.GetEncodedText (varyings[i].Name ?? string.Empty, SKTextEncoding.Utf8, true);
					var pin = nameBytes.AsMemory ().Pin ();
					varyingNamePins[i] = pin;
					pinnedVaryingNames[i] = (byte*)pin.Pointer;

					nativeVaryings[i] = new SKMeshSpecificationVaryingNative
					{
						fType = varyings[i].Type,
						fName = pinnedVaryingNames[i],
					};
				}

				var spec = GetObject (SkiaApi.sk_meshspecification_make (
					(IntPtr)nativeAttrs,
					(IntPtr)attributes.Length,
					(IntPtr)vertexStride,
					(IntPtr)nativeVaryings,
					(IntPtr)varyings.Length,
					vs.Handle,
					fs.Handle,
					colorSpace?.Handle ?? IntPtr.Zero,
					alphaType,
					errorString.Handle));

				errors = errorString?.ToString ();
				if (errors?.Length == 0)
					errors = null;

				return spec;
			}
			finally
			{
				for (int i = 0; i < varyings.Length; i++)
					varyingNamePins[i].Dispose ();
			}
		}
		finally
		{
			for (int i = 0; i < attributes.Length; i++)
				attrNamePins[i].Dispose ();
		}
	}

	// Build

	public static SKMeshBuilder Build (
		ReadOnlySpan<SKMeshSpecificationAttribute> attributes,
		int vertexStride,
		ReadOnlySpan<SKMeshSpecificationVarying> varyings,
		string vertexShader,
		string fragmentShader)
	{
		return Build (attributes, vertexStride, varyings, vertexShader, fragmentShader, null, SKAlphaType.Premul);
	}

	public static SKMeshBuilder Build (
		ReadOnlySpan<SKMeshSpecificationAttribute> attributes,
		int vertexStride,
		ReadOnlySpan<SKMeshSpecificationVarying> varyings,
		string vertexShader,
		string fragmentShader,
		SKColorSpace colorSpace,
		SKAlphaType alphaType)
	{
		var spec = Create (attributes, vertexStride, varyings, vertexShader, fragmentShader, colorSpace, alphaType, out var errors);

		if (spec is null) {
			if (string.IsNullOrEmpty (errors))
				throw new SKMeshSpecificationException ("Failed to create the mesh specification. There was an unknown error.");
			else
				throw new SKMeshSpecificationException ($"Failed to create the mesh specification. There was an error: {errors}");
		}

		return new SKMeshBuilder (spec);
	}

	// Properties

	public int Stride => (int)SkiaApi.sk_meshspecification_get_stride (Handle);

	public int UniformSize => (int)SkiaApi.sk_meshspecification_get_uniform_byte_size (Handle);

	public IReadOnlyList<string> Uniforms => uniformNames ??= GetUniformNames ().ToArray ();

	public IReadOnlyList<string> Children => childNames ??= GetChildrenNames ().ToArray ();

	private IEnumerable<string> GetUniformNames ()
	{
		var count = (int)SkiaApi.sk_meshspecification_get_uniforms_size (Handle);
		using var str = new SKString ();
		for (var i = 0; i < count; i++) {
			SkiaApi.sk_meshspecification_get_uniform_name (Handle, i, str.Handle);
			yield return str.ToString ();
		}
	}

	private IEnumerable<string> GetChildrenNames ()
	{
		var count = (int)SkiaApi.sk_meshspecification_get_children_size (Handle);
		using var str = new SKString ();
		for (var i = 0; i < count; i++) {
			SkiaApi.sk_meshspecification_get_child_name (Handle, i, str.Handle);
			yield return str.ToString ();
		}
	}

	// ToMesh

	public SKMesh ToMesh (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKRect bounds)
	{
		return ToMesh (mode, vertexBuffer, vertexCount, vertexOffset, (SKData?)null, null, bounds, out _);
	}

	public SKMesh ToMesh (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKRect bounds,
		out string? errors)
	{
		return ToMesh (mode, vertexBuffer, vertexCount, vertexOffset, (SKData?)null, null, bounds, out errors);
	}

	public SKMesh ToMesh (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKRuntimeEffectUniforms? uniforms,
		SKRuntimeEffectChildren? children,
		SKRect bounds)
	{
		return ToMesh (mode, vertexBuffer, vertexCount, vertexOffset,
			uniforms?.ToData (), children?.ToArray (), bounds, out _);
	}

	public SKMesh ToMesh (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKData? uniforms,
		SKRect bounds)
	{
		return ToMesh (mode, vertexBuffer, vertexCount, vertexOffset, uniforms, null, bounds, out _);
	}

	public SKMesh ToMesh (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKData? uniforms,
		SKRect bounds,
		out string? errors)
	{
		return ToMesh (mode, vertexBuffer, vertexCount, vertexOffset, uniforms, null, bounds, out errors);
	}

	private SKMesh ToMesh (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKData? uniforms,
		SKObject?[]? children,
		SKRect bounds,
		out string? errors)
	{
		if (vertexBuffer == null)
			throw new ArgumentNullException (nameof (vertexBuffer));

		var mesh = new SKMesh (SkiaApi.sk_mesh_new (), true);
		SkiaApi.sk_mesh_set_spec (mesh.Handle, Handle);
		SkiaApi.sk_mesh_set_mode (mesh.Handle, mode);
		SkiaApi.sk_mesh_set_vertex_buffer (mesh.Handle, vertexBuffer.Handle, (IntPtr)vertexCount, (IntPtr)vertexOffset);

		if (uniforms != null)
			SkiaApi.sk_mesh_set_uniforms (mesh.Handle, uniforms.Handle);

		if (children != null && children.Length > 0) {
			var childHandles = stackalloc IntPtr[children.Length];
			for (int i = 0; i < children.Length; i++)
				childHandles[i] = children[i]?.Handle ?? IntPtr.Zero;
			SkiaApi.sk_mesh_set_children (mesh.Handle, (IntPtr)childHandles, (IntPtr)children.Length);
		}

		var b = bounds;
		SkiaApi.sk_mesh_set_bounds (mesh.Handle, (IntPtr)(&b));

		using var errorString = new SKString ();
		if (!SkiaApi.sk_mesh_validate (mesh.Handle, errorString.Handle)) {
			mesh.Dispose ();

			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;

			return null;
		}

		errors = null;
		return mesh;
	}

	// ToMeshIndexed

	public SKMesh ToMeshIndexed (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset,
		SKRect bounds)
	{
		return ToMeshIndexed (mode, vertexBuffer, vertexCount, vertexOffset,
			indexBuffer, indexCount, indexOffset, (SKData?)null, null, bounds, out _);
	}

	public SKMesh ToMeshIndexed (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset,
		SKRect bounds,
		out string? errors)
	{
		return ToMeshIndexed (mode, vertexBuffer, vertexCount, vertexOffset,
			indexBuffer, indexCount, indexOffset, (SKData?)null, null, bounds, out errors);
	}

	public SKMesh ToMeshIndexed (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset,
		SKRuntimeEffectUniforms? uniforms,
		SKRuntimeEffectChildren? children,
		SKRect bounds)
	{
		return ToMeshIndexed (mode, vertexBuffer, vertexCount, vertexOffset,
			indexBuffer, indexCount, indexOffset,
			uniforms?.ToData (), children?.ToArray (), bounds, out _);
	}

	public SKMesh ToMeshIndexed (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset,
		SKData? uniforms,
		SKRect bounds)
	{
		return ToMeshIndexed (mode, vertexBuffer, vertexCount, vertexOffset,
			indexBuffer, indexCount, indexOffset, uniforms, null, bounds, out _);
	}

	public SKMesh ToMeshIndexed (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset,
		SKData? uniforms,
		SKRect bounds,
		out string? errors)
	{
		return ToMeshIndexed (mode, vertexBuffer, vertexCount, vertexOffset,
			indexBuffer, indexCount, indexOffset, uniforms, null, bounds, out errors);
	}

	private SKMesh ToMeshIndexed (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset,
		SKData? uniforms,
		SKObject?[]? children,
		SKRect bounds,
		out string? errors)
	{
		if (vertexBuffer == null)
			throw new ArgumentNullException (nameof (vertexBuffer));
		if (indexBuffer == null)
			throw new ArgumentNullException (nameof (indexBuffer));

		var mesh = new SKMesh (SkiaApi.sk_mesh_new (), true);
		SkiaApi.sk_mesh_set_spec (mesh.Handle, Handle);
		SkiaApi.sk_mesh_set_mode (mesh.Handle, mode);
		SkiaApi.sk_mesh_set_vertex_buffer (mesh.Handle, vertexBuffer.Handle, (IntPtr)vertexCount, (IntPtr)vertexOffset);
		SkiaApi.sk_mesh_set_index_buffer (mesh.Handle, indexBuffer.Handle, (IntPtr)indexCount, (IntPtr)indexOffset);

		if (uniforms != null)
			SkiaApi.sk_mesh_set_uniforms (mesh.Handle, uniforms.Handle);

		if (children != null && children.Length > 0) {
			var childHandles = stackalloc IntPtr[children.Length];
			for (int i = 0; i < children.Length; i++)
				childHandles[i] = children[i]?.Handle ?? IntPtr.Zero;
			SkiaApi.sk_mesh_set_children (mesh.Handle, (IntPtr)childHandles, (IntPtr)children.Length);
		}

		var b = bounds;
		SkiaApi.sk_mesh_set_bounds (mesh.Handle, (IntPtr)(&b));

		using var errorString = new SKString ();
		if (!SkiaApi.sk_mesh_validate (mesh.Handle, errorString.Handle)) {
			mesh.Dispose ();

			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;

			return null;
		}

		errors = null;
		return mesh;
	}

	internal static SKMeshSpecification GetObject (IntPtr handle) =>
		handle == IntPtr.Zero ? null : new SKMeshSpecification (handle, true);
}

public struct SKMeshSpecificationAttribute
{
	public SKMeshSpecificationAttributeType Type { get; set; }
	public int Offset { get; set; }
	public string Name { get; set; }

	public SKMeshSpecificationAttribute (SKMeshSpecificationAttributeType type, int offset, string name)
	{
		Type = type;
		Offset = offset;
		Name = name;
	}
}

public struct SKMeshSpecificationVarying
{
	public SKMeshSpecificationVaryingType Type { get; set; }
	public string Name { get; set; }

	public SKMeshSpecificationVarying (SKMeshSpecificationVaryingType type, string name)
	{
		Type = type;
		Name = name;
	}
}
