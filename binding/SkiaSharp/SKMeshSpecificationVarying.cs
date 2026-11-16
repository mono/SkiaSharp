using System;

namespace SkiaSharp;

/// <summary>Describes a single value passed from a mesh vertex program to its fragment program.</summary>
/// <remarks />
public readonly struct SKMeshSpecificationVarying
{
	/// <summary>Gets the type of the varying.</summary>
	/// <value>The varying type.</value>
	/// <remarks />
	public SKMeshSpecificationVaryingType Type { get; }

	/// <summary>Gets the name of the varying as referenced by the SkSL programs.</summary>
	/// <value>The varying name.</value>
	/// <remarks />
	public string Name { get; }

	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMeshSpecificationVarying" /> structure.</summary>
	/// <param name="type">The data type of the varying.</param>
	/// <param name="name">The name of the varying as referenced by the SkSL programs.</param>
	/// <remarks />
	public SKMeshSpecificationVarying (SKMeshSpecificationVaryingType type, string name)
	{
		Type = type;
		Name = name ?? throw new ArgumentNullException (nameof (name));
	}
}
