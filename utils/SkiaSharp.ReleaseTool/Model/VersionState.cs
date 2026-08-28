using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	public sealed record VersionState(
		NuGetVersion Skia,
		NuGetVersion HarfBuzz,
		string Label,
		int SkiaComponentCount,
		int HarfBuzzComponentCount)
	{
		public string SkiaText =>
			ReleaseVersionPolicy.FormatNumeric(Skia, SkiaComponentCount);

		public string HarfBuzzText =>
			ReleaseVersionPolicy.FormatNumeric(HarfBuzz, HarfBuzzComponentCount);
	}

	public static class VersionStateReader
	{
		public static VersionState Parse(string variablesText, string versionsText)
		{
			var variableVersion = VariablesYaml.ParseSkiaSharpVersion(variablesText);
			ReleaseVersionPolicy.TryGetNumericParts(
				VariablesYaml.ParseSkiaSharpVersionText(variablesText),
				out var variableParts);
			var label = VariablesYaml.ParsePreviewLabel(variablesText);
			var versions = VersionsTxt.Parse(versionsText);
			if (variableParts.Length != versions.SkiaSharpComponentCount ||
				!VersionComparer.VersionRelease.Equals(variableVersion, versions.SkiaSharp))
			{
				throw new PlanException(
					$"SKIASHARP_VERSION '{variableVersion}' does not match VERSIONS.txt '{versions.SkiaSharp}'");
			}
			return new VersionState(
				versions.SkiaSharp,
				versions.HarfBuzzSharp,
				label,
				versions.SkiaSharpComponentCount,
				versions.HarfBuzzSharpComponentCount);
		}
	}
}
