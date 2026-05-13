using System.Collections.Generic;
using System.Linq;

namespace SkiaSharpSample;

public enum TagKind
{
	Type,
	Method,
	Other,
}

/// <summary>
/// Classifies sample tags as Types vs Methods using heuristics derived
/// from the actual tags collected across all samples. No hardcoded registry —
/// classification is based on naming conventions.
/// </summary>
public static class KnownApis
{
	/// <summary>
	/// Classify a tag as Type, Method, or Other based on naming conventions.
	/// Types: start with "SK", contain a ".", or are known external type names.
	/// Methods: start with "Draw", "Create", "Get", "Set", "From", "To", or
	///          are other known verb-based API names.
	/// </summary>
	public static TagKind Classify(string tag)
	{
		if (string.IsNullOrEmpty(tag))
			return TagKind.Other;

		// Types: SK-prefixed, dotted namespaces (HarfBuzz.Face), known externals
		if (tag.StartsWith("SK", StringComparison.Ordinal))
			return TagKind.Type;
		if (tag.Contains('.'))
			return TagKind.Type;
		if (tag == "Animation")
			return TagKind.Type;

		// Methods: verb prefixes
		if (tag.StartsWith("Draw", StringComparison.Ordinal) ||
			tag.StartsWith("Create", StringComparison.Ordinal) ||
			tag.StartsWith("Get", StringComparison.Ordinal) ||
			tag.StartsWith("Set", StringComparison.Ordinal) ||
			tag.StartsWith("From", StringComparison.Ordinal) ||
			tag.StartsWith("To", StringComparison.Ordinal) ||
			tag.StartsWith("Encode", StringComparison.Ordinal) ||
			tag.StartsWith("Decode", StringComparison.Ordinal) ||
			tag.StartsWith("Match", StringComparison.Ordinal) ||
			tag.StartsWith("Measure", StringComparison.Ordinal) ||
			tag.StartsWith("Clip", StringComparison.Ordinal))
			return TagKind.Method;

		// Known method names that don't match a prefix pattern
		return tag switch
		{
			"Save" or "Restore" or "Translate" or "Scale" or "Concat" => TagKind.Method,
			"RotateDegrees" or "SaveLayer" => TagKind.Method,
			"Clone" or "Decode" or "Snapshot" or "Render" => TagKind.Method,
			"SeekFrameTime" or "PaletteCount" or "BeginPage" => TagKind.Method,
			_ => TagKind.Other,
		};
	}
}
