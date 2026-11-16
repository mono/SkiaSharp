namespace SkiaSharp;

/// <summary>Describes a single value passed from a mesh vertex program to its fragment program.</summary>
/// <remarks />
public struct SKMeshSpecificationVarying
{
	/// <summary>Gets or sets the type of the varying.</summary>
	/// <value>The varying type.</value>
	/// <remarks />
	public SKMeshSpecificationVaryingType Type { get; set; }
	/// <summary>Gets or sets the name of the varying as referenced by the SkSL programs.</summary>
	/// <value>The varying name.</value>
	/// <remarks />
	public string Name { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMeshSpecificationVarying" /> structure.</summary>
	/// <remarks />
	public SKMeshSpecificationVarying (SKMeshSpecificationVaryingType type, string name)
	{
		Type = type;
		Name = name;
	}
}
