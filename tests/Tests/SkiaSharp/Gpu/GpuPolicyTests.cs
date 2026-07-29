using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Guards the GPU policy and records how it resolved here. Deliberately not tagged
	/// <c>Category=GPU</c>: a leg that filters the GPU suite out should still publish
	/// its policy report, because the report is how a correctly-skipped backend is told
	/// apart from one that silently never ran.
	/// </summary>
	public class GpuPolicyTests : BaseTest
	{
		public const string PolicyMarker = "##SKIA-GPU-POLICY##";

		public GpuPolicyTests(ITestOutputHelper output)
			: base(output)
		{
		}

		[Fact]
		public void HostPlatformIsRecognised() =>
			Assert.True(
				GpuPolicy.Platform != TestPlatforms.None,
				"The host platform was not recognised, so every GPU backend would skip. " +
				"Add it to TestPlatforms and GpuPolicy.DetectPlatform.");

		[Fact]
		public void OptOutListNamesOnlyKnownBackends() => GpuPolicy.Validate();

		[Fact]
		public void ReportsResolvedPolicy()
		{
			WriteOutput($"{PolicyMarker} platform={GpuPolicy.PlatformName}");

			foreach (var line in GpuPolicy.Describe())
				WriteOutput($"{PolicyMarker} {line}");
		}
	}
}
