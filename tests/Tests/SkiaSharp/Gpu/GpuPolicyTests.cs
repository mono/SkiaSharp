using Xunit;

namespace SkiaSharp.Tests
{
	public class GpuPolicyTests : BaseTest
	{
		// An unrecognised host would leave every backend not-required, silently
		// skipping the whole GPU suite.
		[Fact]
		public void HostPlatformIsRecognised() =>
			Assert.True(
				TestConfig.Current.Platform != TestPlatforms.None,
				"The host platform was not recognised. Add it to TestPlatforms and TestConfig.DetectPlatform.");

		[Fact]
		public void OptOutListNamesOnlyKnownBackends() => GpuPolicy.Disabled();

		// Renderer names are the policy ids, so a renderer the policy does not know
		// would never be gated.
		[Fact]
		public void EveryRendererNameIsAKnownBackend() =>
			Assert.All(Visual.RendererCatalog.AllNames, name => Assert.Contains(name, GpuPolicy.RequiredOn.Keys));
	}
}
