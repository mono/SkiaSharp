using System.Text.RegularExpressions;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>
	/// Read-only extraction of the handful of <c>scripts/VERSIONS.txt</c>
	/// lines the release tooling cares about. Ported from the regexes in
	/// Python's <c>release_prepare.py</c> (<c>_SKIA_NUGET_RE</c>,
	/// <c>_HARFBUZZ_NUGET_RE</c>) and <c>release_milestones.py</c>
	/// (<c>_SKIASHARP_NUGET_MAJOR_RE</c>, <c>_LIBSKIASHARP_MILESTONE_RE</c>).
	/// Never a general-purpose VERSIONS.txt parser: only these four
	/// specific, known line shapes are recognised.
	/// </summary>
	public static partial class VersionsTxt
	{
		[GeneratedRegex(@"^SkiaSharp\s+nuget\s+(\S+)", RegexOptions.Multiline)]
		private static partial Regex SkiaSharpNugetLine();

		[GeneratedRegex(@"^HarfBuzzSharp\s+nuget\s+(\S+)", RegexOptions.Multiline)]
		private static partial Regex HarfBuzzSharpNugetLine();

		[GeneratedRegex(@"^SkiaSharp\s+nuget\s+(\d+)\.", RegexOptions.Multiline)]
		private static partial Regex SkiaSharpNugetMajorLine();

		[GeneratedRegex(@"^libSkiaSharp\s+milestone\s+(\d+)\s*$", RegexOptions.Multiline)]
		private static partial Regex LibSkiaSharpMilestoneLine();

		public static string ParseSkiaSharpNugetVersion(string versionsText)
		{
			if (!TryParseSkiaSharpNugetVersion(versionsText, out var value))
				throw new PlanException("scripts/VERSIONS.txt has no 'SkiaSharp nuget X.Y.Z' line");
			return value;
		}

		public static string ParseHarfBuzzSharpNugetVersion(string versionsText)
		{
			if (!TryParseHarfBuzzSharpNugetVersion(versionsText, out var value))
				throw new PlanException("scripts/VERSIONS.txt has no 'HarfBuzzSharp nuget X' line");
			return value;
		}

		internal static bool TryParseSkiaSharpNugetVersion(string versionsText, out string value)
		{
			var match = SkiaSharpNugetLine().Match(versionsText);
			value = match.Success ? match.Groups[1].Value : "";
			return match.Success;
		}

		internal static bool TryParseHarfBuzzSharpNugetVersion(string versionsText, out string value)
		{
			var match = HarfBuzzSharpNugetLine().Match(versionsText);
			value = match.Success ? match.Groups[1].Value : "";
			return match.Success;
		}

		/// <summary>
		/// Returns <c>(major, currentSkiaMilestone)</c> from
		/// <c>scripts/VERSIONS.txt</c> -- e.g. major <c>4</c> from
		/// <c>SkiaSharp nuget 4.152.0</c> and milestone <c>152</c> from
		/// <c>libSkiaSharp milestone 152</c>.
		/// </summary>
		public static (int Major, int CurrentSkiaMilestone) ParseCurrentMajorAndMilestone(string versionsText)
		{
			var majorMatch = SkiaSharpNugetMajorLine().Match(versionsText);
			var milestoneMatch = LibSkiaSharpMilestoneLine().Match(versionsText);
			if (!majorMatch.Success || !milestoneMatch.Success)
				throw new PlanException(
					"scripts/VERSIONS.txt has no 'SkiaSharp nuget X.Y.Z' or 'libSkiaSharp milestone N' line");
			return (int.Parse(majorMatch.Groups[1].Value), int.Parse(milestoneMatch.Groups[1].Value));
		}
	}
}
