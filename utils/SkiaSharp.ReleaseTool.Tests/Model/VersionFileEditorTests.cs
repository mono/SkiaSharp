using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	/// <summary>Ported test-for-test from Python's <c>create-release-branches.py</c>' <c>update_version_files</c> (via <c>release_prepare.update_version_files</c>).</summary>
	public class VersionFileEditorTests
	{
		private const string VariablesText =
			"variables:\n  SKIASHARP_VERSION: 3.119.0\n  PREVIEW_LABEL: 'preview.0'\n";

		private const string VersionsText =
			"SkiaSharp                                       nuget       3.119.0\n" +
			"SkiaSharp.HarfBuzz                               nuget       3.119.0\n" +
			"HarfBuzzSharp                                   nuget       1.8.8.1\n" +
			"SkiaSharp               assembly    3.119.0.0\n" +
			"SkiaSharp               file        3.119.0\n" +
			"HarfBuzzSharp           assembly    1.0.0.0\n" +
			"HarfBuzzSharp           file        1.8.8.1\n";

		[Fact]
		public void Prerelease_label_only_bump_touches_only_variables()
		{
			var result = VersionFileEditor.ComputeEdits(VariablesText, VersionsText, "preview.1");

			Assert.Contains("PREVIEW_LABEL: 'preview.1'", result.NewVariablesText);
			Assert.Contains("SKIASHARP_VERSION: 3.119.0", result.NewVariablesText);
			Assert.Equal(VersionsText, result.NewVersionsText);
			Assert.Equal([VersionFileEditor.VariablesPath], result.ChangedPaths);
		}

		[Fact]
		public void Full_version_bump_rewrites_every_related_line_in_both_files()
		{
			var result = VersionFileEditor.ComputeEdits(
				VariablesText, VersionsText, "preview.0", skiaVersion: "3.120.0", harfbuzzVersion: "1.8.8.2");

			Assert.Contains("SKIASHARP_VERSION: 3.120.0", result.NewVariablesText);
			Assert.Contains("SkiaSharp                                       nuget       3.120.0", result.NewVersionsText);
			Assert.Contains("SkiaSharp.HarfBuzz                               nuget       3.120.0", result.NewVersionsText);
			Assert.Contains("HarfBuzzSharp                                   nuget       1.8.8.2", result.NewVersionsText);
			Assert.Contains("SkiaSharp               file        3.120.0.0", result.NewVersionsText);
			Assert.Contains("HarfBuzzSharp           file        1.8.8.2", result.NewVersionsText);
			Assert.Equal(
				new[] { VersionFileEditor.VersionsPath, VersionFileEditor.VariablesPath },
				result.ChangedPaths);
		}

		[Fact]
		public void Skia_file_line_gets_a_trailing_zero_when_the_new_version_has_three_parts()
		{
			var result = VersionFileEditor.ComputeEdits(
				VariablesText, VersionsText, "preview.0", skiaVersion: "3.120.0", harfbuzzVersion: "1.8.8.2");

			Assert.Contains("SkiaSharp               file        3.120.0.0", result.NewVersionsText);
		}

		[Fact]
		public void Skia_file_line_keeps_a_four_part_version_as_is()
		{
			var result = VersionFileEditor.ComputeEdits(
				VariablesText, VersionsText, "preview.0", skiaVersion: "3.120.0.1", harfbuzzVersion: "1.8.8.2");

			Assert.Contains("SkiaSharp               file        3.120.0.1", result.NewVersionsText);
		}

		[Fact]
		public void Rejects_skia_version_without_harfbuzz_version()
		{
			Assert.Throws<PlanException>(
				() => VersionFileEditor.ComputeEdits(VariablesText, VersionsText, "preview.0", skiaVersion: "3.120.0"));
		}

		[Fact]
		public void Rejects_harfbuzz_version_without_skia_version()
		{
			Assert.Throws<PlanException>(
				() => VersionFileEditor.ComputeEdits(
					VariablesText, VersionsText, "preview.0", harfbuzzVersion: "1.8.8.2"));
		}

		[Fact]
		public void Throws_when_nothing_actually_changes()
		{
			// The label is already "preview.0" and no version bump was
			// requested, so neither file's text would change at all.
			Assert.Throws<PlanException>(
				() => VersionFileEditor.ComputeEdits(VariablesText, VersionsText, "preview.0"));
		}

		[Fact]
		public void Throws_when_PREVIEW_LABEL_line_is_missing()
		{
			Assert.Throws<PlanException>(
				() => VersionFileEditor.ComputeEdits("variables:\n  OTHER: 1\n", VersionsText, "preview.1"));
		}
	}
}
