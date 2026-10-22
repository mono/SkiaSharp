using System;
using System.Linq;
using System.Runtime.InteropServices;
using SkiaSharp.Tests.Goldens;

namespace SkiaSharp.Tests.Visual
{
	internal static class VisualPlatform
	{
		private static readonly Lazy<GoldenEnvironment> environment = new(DetermineEnvironment);

		public static GoldenRequest Request(string rendererName, string sceneName)
		{
			var host = environment.Value;
			var parents = host.FallbackProfiles.Concat(host.Profile == "windows-nanoserver"
				? new[] { "windows", "portable" } : new[] { "portable" }).ToArray();
			return new GoldenRequest(host.Profile, host.Version, GoldenNames.Case(sceneName),
				host.Architecture, rendererName, host.VersionScope, parents);
		}

		private static GoldenEnvironment DetermineEnvironment()
		{
			switch (TestConfig.Current.Platform)
			{
				case TestPlatforms.MacOS:
				case TestPlatforms.Linux:
				case TestPlatforms.Windows:
				case TestPlatforms.NanoServer:
					return GoldenEnvironment.Current();
				default:
					var profile = TestConfig.Current.PlatformName;
					return new GoldenEnvironment(profile,
						TestConfig.Current.GoldenVersion,
						RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
						RuntimeInformation.OSDescription);
			}
		}
	}
}
