using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	public sealed record VersionFileEditResult(
		string NewVariablesText,
		string NewVersionsText,
		IReadOnlyList<string> ChangedPaths);

	public static class VersionFileEditor
	{
		public const string VariablesPath = "scripts/azure-templates-variables.yml";
		public const string VersionsPath = "scripts/VERSIONS.txt";

		public static VersionFileEditResult ComputeEdits(
			string variablesText,
			string versionsText,
			string previewLabel,
			string? skiaVersion = null,
			string? harfbuzzVersion = null)
		{
			VariablesYaml.ValidatePreviewLabel(previewLabel);
			var current = VersionStateReader.Parse(variablesText, versionsText);
			var currentRows = VersionsTxt.Parse(versionsText);
			var originalVariablesText = variablesText;
			var originalVersionsText = versionsText;

			variablesText = TextFileLines.ReplaceExactlyOnce(
				variablesText,
				line => VariablesYaml.IsAssignment(line, "PREVIEW_LABEL"),
				line => VariablesYaml.ReplaceAssignment(line, "PREVIEW_LABEL", previewLabel, quote: true),
				"PREVIEW_LABEL");

			if ((skiaVersion is null) != (harfbuzzVersion is null))
				throw new PlanException("skiaVersion and harfbuzzVersion must be set together");

			NuGetVersion expectedSkia = current.Skia;
			NuGetVersion expectedHarfBuzz = current.HarfBuzz;
			var expectedSkiaComponentCount = currentRows.SkiaSharpComponentCount;
			var expectedHarfBuzzComponentCount = currentRows.HarfBuzzSharpComponentCount;
			if (skiaVersion is not null && harfbuzzVersion is not null)
			{
				expectedSkia = ReleaseVersionPolicy.ParseStableVersion(
					skiaVersion, "SkiaSharp version", 3, 4);
				expectedHarfBuzz = ReleaseVersionPolicy.ParseStableVersion(
					harfbuzzVersion, "HarfBuzzSharp version", 3, 4);
				ReleaseVersionPolicy.TryGetNumericParts(skiaVersion, out var skiaParts);
				ReleaseVersionPolicy.TryGetNumericParts(harfbuzzVersion, out var harfBuzzParts);
				expectedSkiaComponentCount = skiaParts.Length;
				expectedHarfBuzzComponentCount = harfBuzzParts.Length;
				var renderedSkia = ReleaseVersionPolicy.FormatNumeric(
					expectedSkia, expectedSkiaComponentCount);
				var renderedHarfBuzz = ReleaseVersionPolicy.FormatNumeric(
					expectedHarfBuzz, expectedHarfBuzzComponentCount);

				variablesText = TextFileLines.ReplaceExactlyOnce(
					variablesText,
					line => VariablesYaml.IsAssignment(line, "SKIASHARP_VERSION"),
					line => VariablesYaml.ReplaceAssignment(
						line, "SKIASHARP_VERSION", renderedSkia, quote: false),
					"SKIASHARP_VERSION");

				versionsText = TextFileLines.ReplaceAll(
					versionsText,
					line => VersionsTxt.IsFamilyNugetRow(line, "SkiaSharp"),
					line => TextFileLines.ReplaceLastToken(line, renderedSkia),
					currentRows.SkiaSharpNugetRows,
					"SkiaSharp nuget");
				versionsText = TextFileLines.ReplaceAll(
					versionsText,
					line => VersionsTxt.IsFamilyNugetRow(line, "HarfBuzzSharp"),
					line => TextFileLines.ReplaceLastToken(line, renderedHarfBuzz),
					currentRows.HarfBuzzSharpNugetRows,
					"HarfBuzzSharp nuget");

				var skiaFile = expectedSkiaComponentCount == 3
					? new Version(expectedSkia.Major, expectedSkia.Minor, expectedSkia.Patch, 0).ToString(4)
					: expectedSkia.Version.ToString(4);
				versionsText = TextFileLines.ReplaceExactlyOnce(
					versionsText,
					line => VersionsTxt.IsRootFileRow(line, "SkiaSharp"),
					line => TextFileLines.ReplaceLastToken(line, skiaFile),
					"SkiaSharp file");
				versionsText = TextFileLines.ReplaceExactlyOnce(
					versionsText,
					line => VersionsTxt.IsRootFileRow(line, "HarfBuzzSharp"),
					line => TextFileLines.ReplaceLastToken(line, renderedHarfBuzz),
					"HarfBuzzSharp file");
			}

			var verified = VersionStateReader.Parse(variablesText, versionsText);
			var verifiedRows = VersionsTxt.Parse(versionsText);
			if (!VersionComparer.VersionRelease.Equals(verified.Skia, expectedSkia) ||
				!VersionComparer.VersionRelease.Equals(verified.HarfBuzz, expectedHarfBuzz) ||
				verifiedRows.SkiaSharpComponentCount != expectedSkiaComponentCount ||
				verifiedRows.HarfBuzzSharpComponentCount != expectedHarfBuzzComponentCount ||
				verified.Label != previewLabel)
			{
				throw new PlanException("version file edit did not produce the requested state");
			}

			var changed = new List<string>(2);
			if (versionsText != originalVersionsText)
				changed.Add(VersionsPath);
			if (variablesText != originalVariablesText)
				changed.Add(VariablesPath);
			if (changed.Count == 0)
				throw new PlanException("version file edit made no changes");

			return new VersionFileEditResult(variablesText, versionsText, changed);
		}
	}
}
