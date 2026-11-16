using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp;

/// <summary>Describes the attributes, varyings and SkSL programs that define a custom vertex mesh.</summary>
/// <remarks />
public unsafe class SKMeshSpecification : SKObject, ISKNonVirtualReferenceCounted, ISKSkipObjectRegistration
{
	/// <summary>The maximum allowed vertex stride, in bytes.</summary>
	public const int MaxStride = 1024;

	/// <summary>The maximum number of attributes allowed in a mesh specification.</summary>
	public const int MaxAttributes = 8;

	/// <summary>The required byte alignment for a vertex stride.</summary>
	public const int StrideAlignment = 4;

	/// <summary>The required byte alignment for an attribute offset.</summary>
	public const int OffsetAlignment = 4;

	/// <summary>The maximum number of varyings allowed in a mesh specification.</summary>
	public const int MaxVaryings = 6;

	private IReadOnlyList<string>? uniformNames;
	private IReadOnlyList<string>? childNames;

	internal SKMeshSpecification (IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	/// <summary>Releases the unmanaged resources used by the specification and optionally releases the managed resources.</summary>
	/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
	/// <remarks />
	protected override void Dispose (bool disposing) =>
		base.Dispose (disposing);

	void ISKNonVirtualReferenceCounted.ReferenceNative () => SkiaApi.sk_meshspecification_ref (Handle);

	void ISKNonVirtualReferenceCounted.UnreferenceNative () => SkiaApi.sk_meshspecification_unref (Handle);

	// Create

	/// <summary>Creates a mesh specification with an explicit color space and alpha type, reporting any compilation errors.</summary>
	/// <param name="attributes">The per-vertex attributes declared by the mesh.</param>
	/// <param name="vertexStride">The size of a single vertex, in bytes.</param>
	/// <param name="varyings">The values passed from the vertex program to the fragment program.</param>
	/// <param name="vertexShader">The SkSL source of the vertex program.</param>
	/// <param name="fragmentShader">The SkSL source of the fragment program.</param>
	/// <param name="colorSpace">The color space of the color the fragment program produces, or <see langword="null" /> for none.</param>
	/// <param name="alphaType">The alpha type of the color the fragment program produces.</param>
	/// <param name="errors">When this method returns, contains the message describing why creation failed, or <see langword="null" /> on success.</param>
	/// <returns>The specification, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public static SKMeshSpecification? Create (
		ReadOnlySpan<SKMeshSpecificationAttribute> attributes,
		int vertexStride,
		ReadOnlySpan<SKMeshSpecificationVarying> varyings,
		string vertexShader,
		string fragmentShader,
		SKColorSpace? colorSpace,
		SKAlphaType alphaType,
		out string? errors) =>
		CreateCore (attributes, vertexStride, varyings, vertexShader, fragmentShader, colorSpace, alphaType, captureErrors: true, out errors);

	internal static SKMeshSpecification? CreateCore (
		ReadOnlySpan<SKMeshSpecificationAttribute> attributes,
		int vertexStride,
		ReadOnlySpan<SKMeshSpecificationVarying> varyings,
		string vertexShader,
		string fragmentShader,
		SKColorSpace? colorSpace,
		SKAlphaType alphaType,
		bool captureErrors,
		out string? errors)
	{
		if (vertexShader == null)
			throw new ArgumentNullException (nameof (vertexShader));
		if (fragmentShader == null)
			throw new ArgumentNullException (nameof (fragmentShader));
		if (attributes.Length == 0)
			throw new ArgumentException ("At least one attribute is required.", nameof (attributes));
		if (vertexStride <= 0)
			throw new ArgumentOutOfRangeException (nameof (vertexStride));

		for (int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i].Offset < 0)
				throw new ArgumentOutOfRangeException (nameof (attributes), "Attribute offset cannot be negative.");
		}

		using var vs = new SKString (vertexShader);
		using var fs = new SKString (fragmentShader);
		using var errorString = captureErrors ? new SKString () : null;

		var attrArray = attributes.Length > MaxAttributes ? new SKMeshSpecificationAttributeNative[attributes.Length] : null;
		var varyingArray = varyings.Length > MaxVaryings ? new SKMeshSpecificationVaryingNative[varyings.Length] : null;
		var attrNamePins = new System.Buffers.MemoryHandle[attributes.Length];
		var varyingNamePins = new System.Buffers.MemoryHandle[varyings.Length];

		fixed (SKMeshSpecificationAttributeNative* pinnedAttrs = attrArray)
		fixed (SKMeshSpecificationVaryingNative* pinnedVaryings = varyingArray)
		{
			var stackAttrs = stackalloc SKMeshSpecificationAttributeNative[attributes.Length <= MaxAttributes ? attributes.Length : 0];
			var stackVaryings = stackalloc SKMeshSpecificationVaryingNative[varyings.Length <= MaxVaryings ? varyings.Length : 0];
			var nativeAttrs = attrArray != null ? pinnedAttrs : stackAttrs;
			var nativeVaryings = varyingArray != null ? pinnedVaryings : stackVaryings;

			try
			{
				for (int i = 0; i < attributes.Length; i++)
				{
					var nameBytes = StringUtilities.GetEncodedText (attributes[i].Name ?? string.Empty, SKTextEncoding.Utf8, true);
					var pin = nameBytes.AsMemory ().Pin ();
					attrNamePins[i] = pin;

					nativeAttrs[i] = new SKMeshSpecificationAttributeNative
					{
						fType = attributes[i].Type,
						fOffset = (IntPtr)attributes[i].Offset,
						fName = (byte*)pin.Pointer,
					};
				}

				for (int i = 0; i < varyings.Length; i++)
				{
					var nameBytes = StringUtilities.GetEncodedText (varyings[i].Name ?? string.Empty, SKTextEncoding.Utf8, true);
					var pin = nameBytes.AsMemory ().Pin ();
					varyingNamePins[i] = pin;

					nativeVaryings[i] = new SKMeshSpecificationVaryingNative
					{
						fType = varyings[i].Type,
						fName = (byte*)pin.Pointer,
					};
				}

				var spec = GetObject (SkiaApi.sk_meshspecification_make (
					nativeAttrs,
					(IntPtr)attributes.Length,
					(IntPtr)vertexStride,
					nativeVaryings,
					(IntPtr)varyings.Length,
					vs.Handle,
					fs.Handle,
					colorSpace?.Handle ?? IntPtr.Zero,
					alphaType,
					errorString?.Handle ?? IntPtr.Zero));

				GC.KeepAlive (colorSpace);

				errors = errorString?.ToString ();
				if (errors?.Length == 0)
					errors = null;

				return spec;
			}
			finally
			{
				for (int i = 0; i < varyings.Length; i++)
					varyingNamePins[i].Dispose ();
				for (int i = 0; i < attributes.Length; i++)
					attrNamePins[i].Dispose ();
			}
		}
	}

	// Properties

	/// <summary>Gets the size of a single vertex, in bytes.</summary>
	/// <value>The vertex stride, in bytes.</value>
	/// <remarks />
	public int Stride => (int)SkiaApi.sk_meshspecification_get_stride (Handle);

	/// <summary>Gets the combined size of the uniforms, in bytes.</summary>
	/// <value>The total uniform size, in bytes.</value>
	/// <remarks />
	public int UniformSize => (int)SkiaApi.sk_meshspecification_get_uniform_byte_size (Handle);

	/// <summary>Gets the color space of the color produced by the fragment program.</summary>
	/// <value>The color space, or <see langword="null" /> if none was specified or the fragment program does not output a color.</value>
	/// <remarks />
	public SKColorSpace? ColorSpace => SKColorSpace.GetObject (SkiaApi.sk_meshspecification_get_colorspace (Handle));

	/// <summary>Gets the names of the uniforms declared by the specification.</summary>
	/// <value>A read-only list of uniform names.</value>
	/// <remarks />
	public IReadOnlyList<string> Uniforms => uniformNames ??= GetUniformNames ().ToArray ();

	/// <summary>Gets the names of the child slots declared by the specification.</summary>
	/// <value>A read-only list of child slot names.</value>
	/// <remarks />
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

	/// <summary>Creates a non-indexed mesh from this specification using the supplied uniforms and child effects, reporting any validation errors.</summary>
	/// <param name="mode">How the vertices are assembled into triangles.</param>
	/// <param name="vertexBuffer">The buffer holding the vertex data.</param>
	/// <param name="vertexCount">The number of vertices to read.</param>
	/// <param name="vertexOffset">The byte offset of the first vertex.</param>
	/// <param name="uniforms">The uniform values for the mesh programs, or <see langword="null" /> for none.</param>
	/// <param name="children">The child effects for the mesh programs, or <see langword="null" /> for none.</param>
	/// <param name="bounds">The bounds of the drawn geometry, used for culling.</param>
	/// <param name="errors">When this method returns, contains the message describing why creation failed, or <see langword="null" /> on success.</param>
	/// <returns>The mesh, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public SKMesh? ToMesh (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKRuntimeEffectUniforms? uniforms,
		SKRuntimeEffectChildren? children,
		SKRect bounds,
		out string? errors) =>
		ToMeshCore (mode, vertexBuffer, vertexCount, vertexOffset, uniforms, children, bounds, captureErrors: true, out errors);

	internal SKMesh? ToMeshCore (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKRuntimeEffectUniforms? uniforms,
		SKRuntimeEffectChildren? children,
		SKRect bounds,
		bool captureErrors,
		out string? errors)
	{
		if (vertexBuffer == null)
			throw new ArgumentNullException (nameof (vertexBuffer));
		if (vertexCount <= 0)
			throw new ArgumentOutOfRangeException (nameof (vertexCount));
		if (vertexOffset < 0)
			throw new ArgumentOutOfRangeException (nameof (vertexOffset));

		var rawData = uniforms?.RawData;
		var childrenArray = children?.ChildrenArray;

		using var childrenHandles = childrenArray != null && childrenArray.Length > 0
			? Utils.RentHandlesArray (childrenArray, true)
			: default;
		using var errorString = captureErrors ? new SKString () : null;

		fixed (IntPtr* ch = childrenHandles)
		{
			var args = new SKMeshMakeArgsNative
			{
				fMode = mode,
				fVB = vertexBuffer.Handle,
				fVCount = (IntPtr)vertexCount,
				fVOffset = (IntPtr)vertexOffset,
				fUniformData = (void*)(rawData?.Data ?? IntPtr.Zero),
				fUniformSize = (IntPtr)(rawData?.Size ?? 0),
				fChildren = ch,
				fChildCount = (IntPtr)childrenHandles.Length,
				fBounds = bounds,
			};

			var mesh = SKMesh.GetObject (SkiaApi.sk_mesh_make (
				Handle,
				&args,
				errorString?.Handle ?? IntPtr.Zero));

			GC.KeepAlive (vertexBuffer);
			GC.KeepAlive (rawData);
			GC.KeepAlive (childrenArray);

			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;

			return mesh;
		}
	}

	// ToMeshIndexed

	/// <summary>Creates an indexed mesh from this specification using the supplied uniforms and child effects, reporting any validation errors.</summary>
	/// <param name="mode">How the vertices are assembled into triangles.</param>
	/// <param name="vertexBuffer">The buffer holding the vertex data.</param>
	/// <param name="vertexCount">The number of vertices to read.</param>
	/// <param name="vertexOffset">The byte offset of the first vertex.</param>
	/// <param name="indexBuffer">The buffer holding the index data.</param>
	/// <param name="indexCount">The number of indices to read.</param>
	/// <param name="indexOffset">The byte offset of the first index.</param>
	/// <param name="uniforms">The uniform values for the mesh programs, or <see langword="null" /> for none.</param>
	/// <param name="children">The child effects for the mesh programs, or <see langword="null" /> for none.</param>
	/// <param name="bounds">The bounds of the drawn geometry, used for culling.</param>
	/// <param name="errors">When this method returns, contains the message describing why creation failed, or <see langword="null" /> on success.</param>
	/// <returns>The mesh, or <see langword="null" /> if it could not be created.</returns>
	/// <remarks />
	public SKMesh? ToMeshIndexed (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset,
		SKRuntimeEffectUniforms? uniforms,
		SKRuntimeEffectChildren? children,
		SKRect bounds,
		out string? errors) =>
		ToMeshIndexedCore (mode, vertexBuffer, vertexCount, vertexOffset, indexBuffer, indexCount, indexOffset, uniforms, children, bounds, captureErrors: true, out errors);

	internal SKMesh? ToMeshIndexedCore (
		SKMeshMode mode,
		SKMeshVertexBuffer vertexBuffer,
		int vertexCount,
		int vertexOffset,
		SKMeshIndexBuffer indexBuffer,
		int indexCount,
		int indexOffset,
		SKRuntimeEffectUniforms? uniforms,
		SKRuntimeEffectChildren? children,
		SKRect bounds,
		bool captureErrors,
		out string? errors)
	{
		if (vertexBuffer == null)
			throw new ArgumentNullException (nameof (vertexBuffer));
		if (indexBuffer == null)
			throw new ArgumentNullException (nameof (indexBuffer));
		if (vertexCount <= 0)
			throw new ArgumentOutOfRangeException (nameof (vertexCount));
		if (vertexOffset < 0)
			throw new ArgumentOutOfRangeException (nameof (vertexOffset));
		if (indexCount <= 0)
			throw new ArgumentOutOfRangeException (nameof (indexCount));
		if (indexOffset < 0)
			throw new ArgumentOutOfRangeException (nameof (indexOffset));

		var rawData = uniforms?.RawData;
		var childrenArray = children?.ChildrenArray;

		using var childrenHandles = childrenArray != null && childrenArray.Length > 0
			? Utils.RentHandlesArray (childrenArray, true)
			: default;
		using var errorString = captureErrors ? new SKString () : null;

		fixed (IntPtr* ch = childrenHandles)
		{
			var args = new SKMeshMakeIndexedArgsNative
			{
				fMode = mode,
				fVB = vertexBuffer.Handle,
				fVCount = (IntPtr)vertexCount,
				fVOffset = (IntPtr)vertexOffset,
				fIB = indexBuffer.Handle,
				fICount = (IntPtr)indexCount,
				fIOffset = (IntPtr)indexOffset,
				fUniformData = (void*)(rawData?.Data ?? IntPtr.Zero),
				fUniformSize = (IntPtr)(rawData?.Size ?? 0),
				fChildren = ch,
				fChildCount = (IntPtr)childrenHandles.Length,
				fBounds = bounds,
			};

			var mesh = SKMesh.GetObject (SkiaApi.sk_mesh_make_indexed (
				Handle,
				&args,
				errorString?.Handle ?? IntPtr.Zero));

			GC.KeepAlive (vertexBuffer);
			GC.KeepAlive (indexBuffer);
			GC.KeepAlive (rawData);
			GC.KeepAlive (childrenArray);

			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;

			return mesh;
		}
	}

	internal static SKMeshSpecification? GetObject (IntPtr handle) =>
		handle == IntPtr.Zero ? null : new SKMeshSpecification (handle, true);
}
