using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Golden directory tags for the current host, most specific first (e.g.
	/// <c>ganesh-gl.macos</c>). The same renderer produces different pixels on
	/// different platforms/drivers, so every cell records its golden per platform.
	/// </summary>
	internal static class VisualPlatform
	{
		public static IReadOnlyList<string> Tags { get; } = DetermineTags().ToList();

		private static IEnumerable<string> DetermineTags()
		{
			// Nano Server IS Windows but rasterizes text with FreeType instead of
			// DirectWrite, so it looks up its own golden first and then falls back to
			// the shared "windows" one for the cells that render identically.
			if (TestConfig.Current.Platform == TestPlatforms.NanoServer)
			{
				yield return TestPlatforms.NanoServer.ToString().ToLowerInvariant();
				yield return TestPlatforms.Windows.ToString().ToLowerInvariant();
				yield break;
			}

			yield return TestConfig.Current.PlatformName;
		}
	}
}
