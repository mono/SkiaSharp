using System;

namespace SkiaSharp;

/// <summary>Describes a single per-vertex attribute of a mesh.</summary>
/// <remarks />
public readonly struct SKMeshSpecificationAttribute
{
	/// <summary>Gets the type of the attribute.</summary>
	/// <value>The attribute type.</value>
	/// <remarks />
	public SKMeshSpecificationAttributeType Type { get; }

	/// <summary>Gets the byte offset of the attribute within a vertex.</summary>
	/// <value>The offset, in bytes.</value>
	/// <remarks />
	public int Offset { get; }

	/// <summary>Gets the name of the attribute as referenced by the SkSL programs.</summary>
	/// <value>The attribute name.</value>
	/// <remarks />
	public string Name { get; }

	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMeshSpecificationAttribute" /> structure.</summary>
	/// <param name="type">The data type of the attribute.</param>
	/// <param name="offset">The byte offset of the attribute within a vertex.</param>
	/// <param name="name">The name of the attribute as referenced by the SkSL programs.</param>
	/// <remarks />
	public SKMeshSpecificationAttribute (SKMeshSpecificationAttributeType type, int offset, string name)
	{
		if (offset < 0)
			throw new ArgumentOutOfRangeException (nameof (offset));

		Type = type;
		Offset = offset;
		Name = name ?? throw new ArgumentNullException (nameof (name));
	}
}
