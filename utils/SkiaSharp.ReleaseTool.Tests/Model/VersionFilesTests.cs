using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	public sealed class VersionFilesTests
	{
		private const string VariablesText =
			"variables:\n" +
			"  SKIASHARP_VERSION: 3.119.0\n" +
			"  PREVIEW_LABEL: 'preview.0'\n";

		private const string VersionsText =
			"libSkiaSharp            milestone   119\n" +
			"SkiaSharp               file        3.119.0.0\n" +
			"HarfBuzzSharp           file        1.8.8.1\n" +
			"SkiaSharp                                       nuget       3.119.0\n" +
			"SkiaSharp.HarfBuzz                               nuget       3.119.0\n" +
			"HarfBuzzSharp                                   nuget       1.8.8.1\n" +
			"HarfBuzzSharp.NativeAssets.Win32                nuget       1.8.8.1\n";

		[Fact]
		public void Repository_version_files_are_consistent()
		{
			var repositoryRoot = Path.GetFullPath(
				Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
			var state = VersionStateReader.Parse(
				File.ReadAllText(Path.Combine(repositoryRoot, VersionFileEditor.VariablesPath)),
				File.ReadAllText(Path.Combine(repositoryRoot, VersionFileEditor.VersionsPath)));

			Assert.NotNull(state.Skia);
			Assert.NotNull(state.HarfBuzz);
		}

		[Fact]
		public void Version_state_is_typed_and_requires_both_files_to_agree()
		{
			var state = VersionStateReader.Parse(VariablesText, VersionsText);

			Assert.Equal("3.119.0", state.Skia.ToNormalizedString());
			Assert.Equal("1.8.8.1", state.HarfBuzz.ToNormalizedString());
			Assert.Equal("preview.0", state.Label);

			var mismatched = VariablesText.Replace("3.119.0", "3.120.0", StringComparison.Ordinal);
			Assert.Throws<PlanException>(
				() => VersionStateReader.Parse(mismatched, VersionsText));
		}

		[Fact]
		public void Versions_parser_requires_unique_consistent_family_rows()
		{
			var parsed = VersionsTxt.Parse(VersionsText);

			Assert.Equal(2, parsed.SkiaSharpNugetRows);
			Assert.Equal(2, parsed.HarfBuzzSharpNugetRows);
			Assert.Equal((3, 119), VersionsTxt.ParseCurrentMajorAndMilestone(VersionsText));

			var inconsistent = VersionsText.Replace(
				"SkiaSharp.HarfBuzz                               nuget       3.119.0",
				"SkiaSharp.HarfBuzz                               nuget       3.120.0",
				StringComparison.Ordinal);
			Assert.Throws<PlanException>(() => VersionsTxt.Parse(inconsistent));

			var duplicate = VersionsText +
				"SkiaSharp                                       nuget       3.119.0\n";
			Assert.Throws<PlanException>(() => VersionsTxt.Parse(duplicate));
		}

		[Theory]
		[InlineData("3.119")]
		[InlineData("3.119.0-preview.1")]
		[InlineData("3.119.0+metadata")]
		[InlineData("2147483648.0.0")]
		public void Version_files_reject_partial_prerelease_metadata_and_overflow(string value)
		{
			var variables = VariablesText.Replace("3.119.0", value, StringComparison.Ordinal);
			Assert.Throws<PlanException>(
				() => VersionStateReader.Parse(variables, VersionsText));
		}

		[Fact]
		public void Preview_zero_is_valid_repository_state_but_not_a_release_identity()
		{
			Assert.Equal("preview.0", VariablesYaml.ParsePreviewLabel(VariablesText));
			Assert.False(SkiaSharpReleaseIdentity.TryParse("3.119.0-preview.0", out _));
		}

		[Fact]
		public void Full_edit_updates_and_revalidates_every_family_row()
		{
			var result = VersionFileEditor.ComputeEdits(
				VariablesText,
				VersionsText,
				"preview.0",
				"3.120.0",
				"1.8.8.2");

			var state = VersionStateReader.Parse(
				result.NewVariablesText,
				result.NewVersionsText);
			Assert.Equal("3.120.0", state.Skia.ToNormalizedString());
			Assert.Equal("1.8.8.2", state.HarfBuzz.ToNormalizedString());
			Assert.Contains("SkiaSharp               file        3.120.0.0", result.NewVersionsText);
			Assert.Equal(
				[VersionFileEditor.VersionsPath, VersionFileEditor.VariablesPath],
				result.ChangedPaths);
		}

		[Fact]
		public void Full_edit_preserves_a_four_part_zero_hotfix()
		{
			var result = VersionFileEditor.ComputeEdits(
				VariablesText,
				VersionsText,
				"preview.0",
				"3.120.0.0",
				"1.8.8.2");

			Assert.Contains("SKIASHARP_VERSION: 3.120.0.0", result.NewVariablesText);
			Assert.Contains("SkiaSharp               file        3.120.0.0", result.NewVersionsText);
			Assert.Equal(4, VersionsTxt.Parse(result.NewVersionsText).SkiaSharpComponentCount);
		}

		[Fact]
		public void Label_only_edit_preserves_versions_and_newline_style()
		{
			var variables = VariablesText.Replace("\n", "\r\n", StringComparison.Ordinal);
			var versions = VersionsText.Replace("\n", "\r\n", StringComparison.Ordinal);

			var result = VersionFileEditor.ComputeEdits(
				variables,
				versions,
				"preview.1");

			Assert.Equal(versions, result.NewVersionsText);
			Assert.DoesNotContain("\n", result.NewVariablesText.Replace("\r\n", "", StringComparison.Ordinal));
			Assert.Contains("PREVIEW_LABEL: 'preview.1'\r\n", result.NewVariablesText);
			Assert.Equal([VersionFileEditor.VariablesPath], result.ChangedPaths);
		}

		[Fact]
		public void Editor_rejects_duplicate_expected_rows_and_partial_arguments()
		{
			var duplicateVariables = VariablesText +
				"  PREVIEW_LABEL: 'preview.0'\n";
			Assert.Throws<PlanException>(
				() => VersionFileEditor.ComputeEdits(duplicateVariables, VersionsText, "preview.1"));
			Assert.Throws<PlanException>(
				() => VersionFileEditor.ComputeEdits(
					VariablesText,
					VersionsText,
					"preview.1",
					skiaVersion: "3.120.0"));
		}

		[Fact]
		public void Editor_rejects_no_op_and_malformed_new_versions()
		{
			Assert.Throws<PlanException>(
				() => VersionFileEditor.ComputeEdits(
					VariablesText,
					VersionsText,
					"preview.0"));
			Assert.Throws<PlanException>(
				() => VersionFileEditor.ComputeEdits(
					VariablesText,
					VersionsText,
					"preview.1",
					"3.120",
					"1.8.8.2"));
			Assert.Throws<PlanException>(
				() => VersionFileEditor.ComputeEdits(
					VariablesText,
					VersionsText,
					"preview.1",
					"3.120.0",
					"1.8.8.2+metadata"));
		}
	}
}
