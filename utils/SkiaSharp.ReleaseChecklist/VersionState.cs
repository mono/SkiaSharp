namespace SkiaSharp.ReleaseChecklist;

/// <summary>Describes SkiaSharp and HarfBuzzSharp version metadata read from a commit.</summary>
/// <param name="SkiaSharpVersion">The numeric SkiaSharp package-family version.</param>
/// <param name="HarfBuzzSharpVersion">The numeric HarfBuzzSharp package-family version.</param>
/// <param name="Label">The release label, such as <c>preview.0</c>, <c>rc.1</c>, or <c>stable</c>.</param>
public sealed record VersionState(
	string SkiaSharpVersion,
	string HarfBuzzSharpVersion,
	string Label)
{
	/// <summary>Gets the release identity represented by the version and label.</summary>
	/// <value>The stable numeric version or prerelease identity.</value>
	public string IdentityText => Label switch
	{
		"stable" => SkiaSharpVersion,
		_ => $"{SkiaSharpVersion}-{Label}",
	};
}
