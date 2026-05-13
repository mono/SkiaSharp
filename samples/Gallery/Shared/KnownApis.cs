using System;

namespace SkiaSharpSample;

/// <summary>
/// Classifies API tags for visual differentiation in the gallery.
/// Tags use the format "Type" (e.g. "SKShader") or "Type.Method"
/// (e.g. "SKImageFilter.CreateBlur"). Classification is derived
/// from the format — no hardcoded registry needed.
/// </summary>
public static class KnownApis
{
	/// <summary>
	/// Classify a tag as Type or Method.
	/// Tags containing a dot are methods (Type.Method format).
	/// Tags without a dot are types.
	/// </summary>
	public static TagKind Classify(string tag) =>
		tag.Contains('.') ? TagKind.Method : TagKind.Type;

	/// <summary>
	/// Extract the display name for a tag.
	/// "SKImageFilter.CreateBlur" → "CreateBlur"
	/// "SKCanvas" → "SKCanvas"
	/// </summary>
	public static string GetDisplayName(string tag)
	{
		var dotIndex = tag.LastIndexOf('.');
		return dotIndex >= 0 ? tag[(dotIndex + 1)..] : tag;
	}

	/// <summary>
	/// Extract the parent type from a method tag.
	/// "SKImageFilter.CreateBlur" → "SKImageFilter"
	/// "SKCanvas" → null
	/// </summary>
	public static string? GetParentType(string tag)
	{
		var dotIndex = tag.LastIndexOf('.');
		return dotIndex >= 0 ? tag[..dotIndex] : null;
	}
}
