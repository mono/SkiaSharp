namespace SkiaSharpSample;

/// <summary>
/// Whether a sample tag represents a type (e.g. SKShader) or a method (e.g. DrawPath).
/// Used by the gallery UI to visually distinguish types from methods in the tag cloud.
/// </summary>
public enum TagKind
{
	Type,
	Method,
}
