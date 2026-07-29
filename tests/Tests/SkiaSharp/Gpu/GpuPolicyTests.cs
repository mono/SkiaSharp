using System.Linq;
using SkiaSharp.Tests.Visual;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GpuPolicyTests : BaseTest
	{
		/// <summary>
		/// An unrecognised host would classify every backend as not-required and skip
		/// the whole GPU suite — the silent hole this policy exists to close.
		/// </summary>
		[Fact]
		public void HostPlatformIsRecognised() =>
			Assert.True(
				TestConfig.Current.Platform != TestPlatforms.None,
				"The host platform was not recognised. Add it to TestPlatforms and TestConfig.DetectPlatform.");

		/// <summary>A typo must not quietly leave a backend required.</summary>
		[Fact]
		public void OptOutListNamesOnlyKnownBackends() => GpuPolicy.Validate();

		/// <summary>
		/// Renderer names are the policy ids, so a renderer the policy does not know
		/// would never be gated. Catches drift between the two.
		/// </summary>
		[Fact]
		public void EveryRendererNameIsAKnownBackend() =>
			Assert.All(RendererCatalog.AllNames, name => Assert.Contains(name, GpuPolicy.All));
	}
}
