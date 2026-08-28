using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>
	/// The handful of version fields prepare-planning reads together from
	/// a ref/worktree: the SkiaSharp/HarfBuzzSharp NuGet versions (from
	/// <c>scripts/VERSIONS.txt</c>) and the current preview label (from
	/// <c>scripts/azure-templates-variables.yml</c>). Ported from
	/// Python's <c>release_prepare.VersionState</c>.
	/// </summary>
	public sealed record VersionState(string Skia, string HarfBuzz, string Label);

	/// <summary>Ported from Python's <c>release_prepare._parse_state</c>.</summary>
	public static class VersionStateReader
	{
		public static VersionState Parse(string variablesText, string versionsText)
		{
			// SKIASHARP_VERSION is only checked for *presence* here, never
			// used for its value: `.Skia` below always comes from
			// VERSIONS.txt's "SkiaSharp nuget" line instead. This exactly
			// mirrors Python's `_parse_state`, which computes
			// `version_match` purely to fold it into the "could not
			// parse ..." precondition and then never reads
			// `version_match.group(1)`.
			var hasSkiaSharpVersion = VariablesYaml.TryParseSkiaSharpVersion(variablesText, out _);
			var hasLabel = VariablesYaml.TryParsePreviewLabel(variablesText, out var label);
			var hasSkia = VersionsTxt.TryParseSkiaSharpNugetVersion(versionsText, out var skia);
			var hasHarfBuzz = VersionsTxt.TryParseHarfBuzzSharpNugetVersion(versionsText, out var harfbuzz);
			if (!(hasSkiaSharpVersion && hasLabel && hasSkia && hasHarfBuzz))
				throw new PlanException("could not parse SKIASHARP_VERSION/PREVIEW_LABEL/nuget versions");
			return new VersionState(skia, harfbuzz, label);
		}
	}
}
