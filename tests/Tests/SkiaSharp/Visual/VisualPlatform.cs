using System;
using System.Runtime.InteropServices;
using SkiaSharp.Tests.Goldens;

namespace SkiaSharp.Tests.Visual
{
	internal static class VisualPlatform
	{
		private static readonly Lazy<GoldenEnvironment> environment = new(DetermineEnvironment);

		public static GoldenRequest Request(string rendererName, string sceneName) =>
			environment.Value.Request(GoldenNames.Case(sceneName), rendererName);

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
