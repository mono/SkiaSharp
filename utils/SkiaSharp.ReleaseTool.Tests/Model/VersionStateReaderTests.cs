using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	/// <summary>Ported from Python's <c>release_prepare._parse_state</c> (indirectly exercised there via prepare-plan tests).</summary>
	public class VersionStateReaderTests
	{
		private const string VariablesText =
			"variables:\n  SKIASHARP_VERSION: 3.119.0\n  PREVIEW_LABEL: 'preview.0'\n";

		private const string VersionsText =
			"SkiaSharp                                       nuget       3.119.0\n" +
			"HarfBuzzSharp                                   nuget       1.8.8.1\n";

		[Fact]
		public void Parses_skia_harfbuzz_and_label_together()
		{
			var state = VersionStateReader.Parse(VariablesText, VersionsText);

			Assert.Equal("3.119.0", state.Skia);
			Assert.Equal("1.8.8.1", state.HarfBuzz);
			Assert.Equal("preview.0", state.Label);
		}

		[Fact]
		public void Skia_comes_from_VERSIONS_txt_not_SKIASHARP_VERSION()
		{
			// Mirrors a subtle but deliberate Python behaviour: `_parse_state`
			// only checks SKIASHARP_VERSION for *presence*, never reads its
			// value -- `.Skia` always comes from VERSIONS.txt's "SkiaSharp
			// nuget" line instead, even when the two disagree.
			const string mismatchedVariables = "variables:\n  SKIASHARP_VERSION: 9.9.9\n  PREVIEW_LABEL: 'preview.0'\n";

			var state = VersionStateReader.Parse(mismatchedVariables, VersionsText);

			Assert.Equal("3.119.0", state.Skia);
		}

		[Fact]
		public void Throws_the_combined_message_when_any_field_is_missing()
		{
			var ex = Assert.Throws<PlanException>(
				() => VersionStateReader.Parse("variables:\n  OTHER: 1\n", VersionsText));

			Assert.Contains("SKIASHARP_VERSION/PREVIEW_LABEL/nuget versions", ex.Message);
		}
	}
}
