using System.Runtime.InteropServices;
using SkiaSharp.Tests.Goldens;

namespace SkiaSharp.Tests.Visual
{
	internal static class VisualPlatform
	{
		public static GoldenRequest Request(string rendererName, string sceneName)
		{
			var profile = TestConfig.Current.Platform == TestPlatforms.MacOS
				? "mac" : TestConfig.Current.PlatformName;
			var parents = TestConfig.Current.Platform == TestPlatforms.NanoServer
				? new[] { "windows", "portable" } : new[] { "portable" };
			return new GoldenRequest(profile, "base", GoldenNames.Case(sceneName),
				RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(), rendererName,
				GoldenVersionScope.Unversioned, parents);
		}
	}
}
