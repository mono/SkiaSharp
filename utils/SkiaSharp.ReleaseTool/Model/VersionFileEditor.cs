using System.Text.RegularExpressions;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>
	/// The result of planning an in-place edit of
	/// <c>scripts/azure-templates-variables.yml</c> and
	/// <c>scripts/VERSIONS.txt</c>: the new text for each file, and which
	/// of the two repository-relative paths actually changed. Pure and
	/// side-effect free -- writing the edited text back to disk is a
	/// later slice's concern.
	/// </summary>
	public sealed record VersionFileEditResult(
		string NewVariablesText,
		string NewVersionsText,
		IReadOnlyList<string> ChangedPaths);

	/// <summary>
	/// Computes the exact in-place text edit
	/// <c>create-release-branches.py</c>'s <c>update_version_files</c>
	/// applies for a prerelease-label bump (``PREVIEW_LABEL`` only) or a
	/// full version bump (``SKIASHARP_VERSION`` and every ``*nuget*``/
	/// ``*file*`` line together). Ported from Python's
	/// <c>release_prepare.update_version_files</c>, minus the actual
	/// file I/O.
	/// </summary>
	public static partial class VersionFileEditor
	{
		public const string VariablesPath = "scripts/azure-templates-variables.yml";
		public const string VersionsPath = "scripts/VERSIONS.txt";

		[GeneratedRegex(@"^(\s*PREVIEW_LABEL:\s*)[^\r\n]*(\r?)$", RegexOptions.Multiline)]
		private static partial Regex PreviewLabelLinePattern();

		[GeneratedRegex(@"^(\s*SKIASHARP_VERSION:\s*)\S+([^\S\r\n]*)(\r?)$", RegexOptions.Multiline)]
		private static partial Regex SkiaSharpVersionLinePattern();

		[GeneratedRegex(@"^(SkiaSharp\s+file\s+)\S+([^\S\r\n]*)(\r?)$", RegexOptions.Multiline)]
		private static partial Regex SkiaFileLinePattern();

		[GeneratedRegex(@"^(SkiaSharp\S*\s+nuget\s+)\S+([^\S\r\n]*)(\r?)$", RegexOptions.Multiline)]
		private static partial Regex SkiaNugetLinePattern();

		[GeneratedRegex(@"^(HarfBuzzSharp\s+file\s+)\S+([^\S\r\n]*)(\r?)$", RegexOptions.Multiline)]
		private static partial Regex HarfBuzzFileLinePattern();

		[GeneratedRegex(@"^(HarfBuzzSharp\S*\s+nuget\s+)\S+([^\S\r\n]*)(\r?)$", RegexOptions.Multiline)]
		private static partial Regex HarfBuzzNugetLinePattern();

		/// <param name="variablesText">The current contents of <see cref="VariablesPath"/>.</param>
		/// <param name="versionsText">The current contents of <see cref="VersionsPath"/>.</param>
		/// <param name="previewLabel">The new <c>PREVIEW_LABEL</c> value (always rewritten).</param>
		/// <param name="skiaVersion">
		/// The new SkiaSharp version, or <see langword="null"/> for a
		/// prerelease-label-only bump. Must be given together with
		/// <paramref name="harfbuzzVersion"/> or not at all.
		/// </param>
		/// <param name="harfbuzzVersion">The new HarfBuzzSharp version; see <paramref name="skiaVersion"/>.</param>
		public static VersionFileEditResult ComputeEdits(
			string variablesText,
			string versionsText,
			string previewLabel,
			string? skiaVersion = null,
			string? harfbuzzVersion = null)
		{
			var originalVersionsText = versionsText;
			var changed = new SortedSet<string>(StringComparer.Ordinal);

			var newVariablesText = ReplaceExactlyOnce(
				PreviewLabelLinePattern(), variablesText,
				m => $"{m.Groups[1].Value}'{previewLabel}'{m.Groups[2].Value}",
				VariablesPath, "PREVIEW_LABEL");
			if (newVariablesText != variablesText)
				changed.Add(VariablesPath);
			variablesText = newVariablesText;

			if (skiaVersion is not null || harfbuzzVersion is not null)
			{
				if (skiaVersion is null || harfbuzzVersion is null)
					throw new PlanException("skia_version and harfbuzz_version must be set together");

				var parts = skiaVersion.Split('.');
				var skiaFile = parts.Length == 3 ? $"{skiaVersion}.0" : skiaVersion;

				versionsText = ReplaceExactlyOnce(
					SkiaFileLinePattern(), versionsText,
					m => $"{m.Groups[1].Value}{skiaFile}{m.Groups[2].Value}{m.Groups[3].Value}",
					VersionsPath, "'SkiaSharp file'");

				versionsText = SkiaNugetLinePattern().Replace(
					versionsText,
					m => $"{m.Groups[1].Value}{skiaVersion}{m.Groups[2].Value}{m.Groups[3].Value}");

				versionsText = ReplaceExactlyOnce(
					HarfBuzzFileLinePattern(), versionsText,
					m => $"{m.Groups[1].Value}{harfbuzzVersion}{m.Groups[2].Value}{m.Groups[3].Value}",
					VersionsPath, "'HarfBuzzSharp file'");

				versionsText = HarfBuzzNugetLinePattern().Replace(
					versionsText,
					m => $"{m.Groups[1].Value}{harfbuzzVersion}{m.Groups[2].Value}{m.Groups[3].Value}");

				variablesText = ReplaceExactlyOnce(
					SkiaSharpVersionLinePattern(), variablesText,
					m => $"{m.Groups[1].Value}{skiaVersion}{m.Groups[2].Value}{m.Groups[3].Value}",
					VariablesPath, "SKIASHARP_VERSION");
				changed.Add(VariablesPath);
			}

			if (versionsText != originalVersionsText)
				changed.Add(VersionsPath);

			if (changed.Count == 0)
				throw new PlanException("update_version_files made no changes");

			return new VersionFileEditResult(variablesText, versionsText, [.. changed]);
		}

		private static string ReplaceExactlyOnce(
			Regex pattern, string input, MatchEvaluator evaluator, string path, string description)
		{
			if (!pattern.IsMatch(input))
				throw new PlanException($"could not update {description} in {path}");
			return pattern.Replace(input, evaluator, 1);
		}
	}
}
