using System;
using System.Collections.Generic;

namespace SkiaSharp;

/// <summary>Builds an <see cref="T:SkiaSharp.SKMeshSpecification" /> from attributes, varyings, and SkSL programs.</summary>
/// <remarks />
public class SKMeshSpecificationBuilder
{
	private readonly string vertexShader;
	private readonly string fragmentShader;
	private readonly List<SKMeshSpecificationAttribute> attributes = new ();
	private readonly List<SKMeshSpecificationVarying> varyings = new ();
	private int? explicitVertexStride;

	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMeshSpecificationBuilder" /> class with the specified vertex and fragment SkSL programs.</summary>
	/// <param name="vertexShader">The SkSL source of the vertex program.</param>
	/// <param name="fragmentShader">The SkSL source of the fragment program.</param>
	/// <remarks />
	public SKMeshSpecificationBuilder (string vertexShader, string fragmentShader)
	{
		this.vertexShader = vertexShader ?? throw new ArgumentNullException (nameof (vertexShader));
		this.fragmentShader = fragmentShader ?? throw new ArgumentNullException (nameof (fragmentShader));
	}

	/// <summary>Gets the collection of vertex attributes declared on the builder.</summary>
	/// <value>The list of vertex attributes.</value>
	/// <remarks />
	public IList<SKMeshSpecificationAttribute> Attributes => attributes;

	/// <summary>Gets the collection of varyings passed from the vertex program to the fragment program.</summary>
	/// <value>The list of varyings.</value>
	/// <remarks />
	public IList<SKMeshSpecificationVarying> Varyings => varyings;

	/// <summary>Gets or sets the byte stride of a single vertex. When not explicitly set, returns the stride calculated from the added attributes.</summary>
	/// <value>The vertex stride, in bytes.</value>
	/// <remarks />
	public int VertexStride {
		get => explicitVertexStride ?? CalculateVertexStride ();
		set {
			if (value <= 0)
				throw new ArgumentOutOfRangeException (nameof (value));
			explicitVertexStride = value;
		}
	}

	/// <summary>Gets or sets the color space of the color produced by the fragment program.</summary>
	/// <value>The color space, or <see langword="null" /> for none.</value>
	/// <remarks />
	public SKColorSpace? ColorSpace { get; set; }

	/// <summary>Gets or sets the alpha type of the color produced by the fragment program.</summary>
	/// <value>The alpha type. The default is <see cref="F:SkiaSharp.SKAlphaType.Premul" />.</value>
	/// <remarks />
	public SKAlphaType AlphaType { get; set; } = SKAlphaType.Premul;

	/// <summary>Adds a vertex attribute immediately after the previous attribute and advances the calculated stride by the attribute size.</summary>
	/// <param name="name">The name of the attribute as referenced in the SkSL vertex program.</param>
	/// <param name="type">The data type of the attribute.</param>
	/// <remarks />
	public void AddAttribute (string name, SKMeshSpecificationAttributeType type) =>
		AddAttribute (name, type, CalculateVertexStride ());

	/// <summary>Adds a vertex attribute at the specified byte offset and updates the calculated stride if needed.</summary>
	/// <param name="name">The name of the attribute as referenced in the SkSL vertex program.</param>
	/// <param name="type">The data type of the attribute.</param>
	/// <param name="offset">The byte offset of the attribute within a vertex.</param>
	/// <remarks />
	public void AddAttribute (string name, SKMeshSpecificationAttributeType type, int offset)
	{
		if (name == null)
			throw new ArgumentNullException (nameof (name));
		if (offset < 0)
			throw new ArgumentOutOfRangeException (nameof (offset));

		attributes.Add (new SKMeshSpecificationAttribute (type, offset, name));
	}

	/// <summary>Adds a varying passed from the vertex program to the fragment program.</summary>
	/// <param name="name">The name of the varying as referenced in the SkSL programs.</param>
	/// <param name="type">The data type of the varying.</param>
	/// <remarks />
	public void AddVarying (string name, SKMeshSpecificationVaryingType type)
	{
		if (name == null)
			throw new ArgumentNullException (nameof (name));

		varyings.Add (new SKMeshSpecificationVarying (type, name));
	}

	private int CalculateVertexStride ()
	{
		var stride = 0;
		for (var i = 0; i < attributes.Count; i++)
		{
			var attr = attributes[i];
			if (attr.Offset >= 0)
			{
				var end = attr.Offset + GetAttributeSize (attr.Type);
				if (end > stride)
					stride = end;
			}
		}
		return stride;
	}

	/// <summary>Builds the mesh specification.</summary>
	/// <returns>The compiled mesh specification, or <see langword="null" /> if compilation failed.</returns>
	/// <remarks />
	public SKMeshSpecification? Build () =>
		BuildCore (captureErrors: false, out _);

	/// <summary>Builds the mesh specification, reporting any compilation errors.</summary>
	/// <param name="errors">When this method returns, contains the message describing why creation failed, or <see langword="null" /> on success.</param>
	/// <returns>The compiled mesh specification, or <see langword="null" /> if compilation failed.</returns>
	/// <remarks />
	public SKMeshSpecification? Build (out string? errors) =>
		BuildCore (captureErrors: true, out errors);

	private SKMeshSpecification? BuildCore (bool captureErrors, out string? errors)
	{
		var attrSpan = attributes.ToArray ();
		var varyingSpan = varyings.Count > 0 ? varyings.ToArray () : Array.Empty<SKMeshSpecificationVarying> ();

		return SKMeshSpecification.CreateCore (
			attrSpan,
			VertexStride,
			varyingSpan,
			vertexShader,
			fragmentShader,
			ColorSpace,
			AlphaType,
			captureErrors,
			out errors);
	}

	private static int GetAttributeSize (SKMeshSpecificationAttributeType type) =>
		type switch
		{
			SKMeshSpecificationAttributeType.Float => sizeof (float),
			SKMeshSpecificationAttributeType.Float2 => sizeof (float) * 2,
			SKMeshSpecificationAttributeType.Float3 => sizeof (float) * 3,
			SKMeshSpecificationAttributeType.Float4 => sizeof (float) * 4,
			SKMeshSpecificationAttributeType.Ubyte4Unorm => sizeof (byte) * 4,
			_ => throw new ArgumentOutOfRangeException (nameof (type)),
		};
}
