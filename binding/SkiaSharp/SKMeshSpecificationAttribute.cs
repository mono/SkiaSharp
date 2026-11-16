namespace SkiaSharp;

/// <summary>Describes a single per-vertex attribute of a mesh.</summary>
/// <remarks />
public struct SKMeshSpecificationAttribute
{
	/// <summary>Gets or sets the type of the attribute.</summary>
	/// <value>The attribute type.</value>
	/// <remarks />
	public SKMeshSpecificationAttributeType Type { get; set; }
	/// <summary>Gets or sets the byte offset of the attribute within a vertex.</summary>
	/// <value>The offset, in bytes.</value>
	/// <remarks />
	public int Offset { get; set; }
	/// <summary>Gets or sets the name of the attribute as referenced by the SkSL programs.</summary>
	/// <value>The attribute name.</value>
	/// <remarks />
	public string Name { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMeshSpecificationAttribute" /> structure.</summary>
	/// <remarks />
	public SKMeshSpecificationAttribute (SKMeshSpecificationAttributeType type, int offset, string name)
	{
		Type = type;
		Offset = offset;
		Name = name;
	}
}
