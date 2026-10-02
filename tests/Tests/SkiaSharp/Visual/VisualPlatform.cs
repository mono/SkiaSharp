using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Golden directory tags for the current host, most specific first (e.g.
	/// <c>ganesh-gl.macos-arm64</c>, then <c>ganesh-gl.macos</c>). The same
	/// renderer can produce different pixels on different platforms, architectures,
	/// or drivers, so every test records its golden using layered host tags.
	/// </summary>
	internal static class VisualPlatform
	{
		public static IReadOnlyList<string> Tags { get; } = DetermineTags().ToList();

		private static IEnumerable<string> DetermineTags()
		{
			var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

			// Nano Server IS Windows but rasterizes text with FreeType instead of
			// DirectWrite, so it looks up its own golden first and then falls back to
			// the shared "windows" one for the scenes that render identically.
			if (TestConfig.Current.Platform == TestPlatforms.NanoServer)
			{
				yield return $"{TestConfig.Current.PlatformName}-{architecture}";
				yield return TestPlatforms.NanoServer.ToString().ToLowerInvariant();
				yield return $"{TestPlatforms.Windows.ToString().ToLowerInvariant()}-{architecture}";
				yield return TestPlatforms.Windows.ToString().ToLowerInvariant();
				yield break;
			}

			yield return $"{TestConfig.Current.PlatformName}-{architecture}";
			yield return TestConfig.Current.PlatformName;
		}
	}
}
