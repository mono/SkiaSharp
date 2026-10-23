using System.Collections.Generic;

namespace SkiaSharp.Tests.Shared
{
	public static class GoldenPlatform
	{
		public static string Normalize(string profile)
		{
			GoldenPaths.ValidateSegment(profile);
			return profile == "macos" ? "mac" : profile;
		}

		public static IEnumerable<string> Profiles(string profile)
		{
			profile = Normalize(profile);
			yield return profile;
			if (profile == "nanoserver") yield return "windows";
			if (profile != "portable") yield return "portable";
		}
	}
}
