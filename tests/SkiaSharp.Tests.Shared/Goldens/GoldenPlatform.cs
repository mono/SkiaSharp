using System.Collections.Generic;

namespace SkiaSharp.Tests.Shared;

// Centralizes the small deterministic platform fallback policy.
internal static class GoldenPlatform
{
	public static string Normalize(string platform)
	{
		GoldenPaths.ValidateSegment(platform);
		return platform == "macos" ? "mac" : platform;
	}

	public static IEnumerable<string> FallbackChain(string platform)
	{
		platform = Normalize(platform);
		yield return platform;
		if (platform == "nanoserver") yield return "windows";
		if (platform != "portable") yield return "portable";
	}
}
