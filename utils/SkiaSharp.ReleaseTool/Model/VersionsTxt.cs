using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	public sealed record VersionsDocument(
		NuGetVersion SkiaSharp,
		NuGetVersion HarfBuzzSharp,
		NuGetVersion SkiaSharpFile,
		NuGetVersion HarfBuzzSharpFile,
		int SkiaSharpComponentCount,
		int HarfBuzzSharpComponentCount,
		int SkiaSharpNugetRows,
		int HarfBuzzSharpNugetRows);

	public static class VersionsTxt
	{
		private const string NuGetSectionHeader = "# nuget versions";

		public static VersionsDocument Parse(string versionsText)
		{
			var seenRows = new HashSet<string>(StringComparer.Ordinal);
			var skiaVersions = new List<NuGetVersion>();
			var harfBuzzVersions = new List<NuGetVersion>();
			NuGetVersion? skiaRoot = null;
			NuGetVersion? harfBuzzRoot = null;
			NuGetVersion? skiaFile = null;
			NuGetVersion? harfBuzzFile = null;
			var skiaComponentCount = 0;
			var harfBuzzComponentCount = 0;

			foreach (var line in TextFileLines.Split(versionsText))
			{
				var columns = TextFileLines.Columns(line.Content);
				if (columns.Length < 3)
					continue;

				var package = columns[0];
				var kind = columns[1];
				if (kind == "nuget" &&
					(package.StartsWith("SkiaSharp", StringComparison.Ordinal) ||
					 package.StartsWith("HarfBuzzSharp", StringComparison.Ordinal)))
				{
					RequireUnique(seenRows, $"{package}\0{kind}", $"{package} {kind}");
					var version = ReleaseVersionPolicy.ParseStableVersion(
						columns[2], $"{package} nuget version", 3, 4);
					if (package.StartsWith("SkiaSharp", StringComparison.Ordinal))
					{
						skiaVersions.Add(version);
						if (package == "SkiaSharp")
						{
							skiaRoot = version;
							ReleaseVersionPolicy.TryGetNumericParts(columns[2], out var parts);
							skiaComponentCount = parts.Length;
						}
					}
					else
					{
						harfBuzzVersions.Add(version);
						if (package == "HarfBuzzSharp")
						{
							harfBuzzRoot = version;
							ReleaseVersionPolicy.TryGetNumericParts(columns[2], out var parts);
							harfBuzzComponentCount = parts.Length;
						}
					}
				}
				else if (kind == "file" && package is "SkiaSharp" or "HarfBuzzSharp")
				{
					RequireUnique(seenRows, $"{package}\0{kind}", $"{package} {kind}");
					var version = ReleaseVersionPolicy.ParseStableVersion(
						columns[2], $"{package} file version", 3, 4);
					if (package == "SkiaSharp")
						skiaFile = version;
					else
						harfBuzzFile = version;
				}
			}

			if (skiaRoot is null || harfBuzzRoot is null || skiaFile is null || harfBuzzFile is null)
				throw new PlanException("scripts/VERSIONS.txt is missing a required root nuget or file row");
			RequireConsistent(skiaVersions, skiaRoot, "SkiaSharp nuget");
			RequireConsistent(harfBuzzVersions, harfBuzzRoot, "HarfBuzzSharp nuget");

			var expectedSkiaFile = skiaRoot.Version.Revision < 0
				? new Version(skiaRoot.Major, skiaRoot.Minor, skiaRoot.Patch, 0)
				: skiaRoot.Version;
			if (!Equals(skiaFile.Version, expectedSkiaFile))
				throw new PlanException("SkiaSharp file version does not match its nuget version");
			if (!Equals(harfBuzzFile.Version, harfBuzzRoot.Version))
				throw new PlanException("HarfBuzzSharp file version does not match its nuget version");

			return new VersionsDocument(
				skiaRoot,
				harfBuzzRoot,
				skiaFile,
				harfBuzzFile,
				skiaComponentCount,
				harfBuzzComponentCount,
				skiaVersions.Count,
				harfBuzzVersions.Count);
		}

		public static NuGetVersion ParseSkiaSharpNugetVersion(string versionsText) =>
			Parse(versionsText).SkiaSharp;

		public static NuGetVersion ParseHarfBuzzSharpNugetVersion(string versionsText) =>
			Parse(versionsText).HarfBuzzSharp;

		public static (int Major, int CurrentSkiaMilestone) ParseCurrentMajorAndMilestone(string versionsText)
		{
			var versions = Parse(versionsText);
			var milestones = TextFileLines.Split(versionsText)
				.Select(static line => TextFileLines.Columns(line.Content))
				.Where(static columns =>
					columns.Length >= 3 &&
					columns[0] == "libSkiaSharp" &&
					columns[1] == "milestone")
				.ToArray();
			if (milestones.Length != 1 ||
				!int.TryParse(milestones[0][2], out var milestone) ||
				milestone < 0)
			{
				throw new PlanException("scripts/VERSIONS.txt must contain exactly one valid libSkiaSharp milestone row");
			}

			return (versions.SkiaSharp.Major, milestone);
		}

		public static IReadOnlyDictionary<string, IReadOnlyList<string>> ParsePackageFamilies(
			string versionsText)
		{
			var lines = TextFileLines.Split(versionsText);
			var starts = lines
				.Select((line, index) => (line.Content, Index: index))
				.Where(static item => item.Content.Trim() == NuGetSectionHeader)
				.ToArray();
			if (starts.Length != 1)
				throw new PlanException($"scripts/VERSIONS.txt must contain exactly one '{NuGetSectionHeader}' section");

			var families = new Dictionary<string, List<string>>(StringComparer.Ordinal);
			string? currentFamily = null;
			foreach (var line in lines.Skip(starts[0].Index + 1))
			{
				var trimmed = line.Content.Trim();
				if (trimmed is "# SkiaSharp" or "# HarfBuzzSharp")
				{
					currentFamily = trimmed[2..];
					families.TryAdd(currentFamily, []);
					continue;
				}

				var columns = TextFileLines.Columns(line.Content);
				if (columns.Length != 3 || columns[1] != "nuget")
					continue;
				if (currentFamily is null)
					throw new PlanException("scripts/VERSIONS.txt has a nuget package row before a family heading");
				if (families[currentFamily].Contains(columns[0], StringComparer.Ordinal))
					throw new PlanException($"scripts/VERSIONS.txt contains duplicate package '{columns[0]}'");
				_ = ReleaseVersionPolicy.ParseStableVersion(
					columns[2],
					$"{columns[0]} nuget version",
					3,
					4);
				families[currentFamily].Add(columns[0]);
			}

			if (!families.TryGetValue("SkiaSharp", out var skia) || skia.Count == 0 ||
				!families.TryGetValue("HarfBuzzSharp", out var harfBuzz) || harfBuzz.Count == 0)
			{
				throw new PlanException("scripts/VERSIONS.txt must declare non-empty SkiaSharp and HarfBuzzSharp package families");
			}

			return families.ToDictionary(
				static pair => pair.Key,
				static pair => (IReadOnlyList<string>)pair.Value,
				StringComparer.Ordinal);
		}

		internal static bool IsFamilyNugetRow(string line, string family)
		{
			var columns = TextFileLines.Columns(line);
			return columns.Length >= 3 &&
				columns[0].StartsWith(family, StringComparison.Ordinal) &&
				columns[1] == "nuget";
		}

		internal static bool IsRootFileRow(string line, string family)
		{
			var columns = TextFileLines.Columns(line);
			return columns.Length >= 3 && columns[0] == family && columns[1] == "file";
		}

		private static void RequireUnique(
			HashSet<string> rows,
			string key,
			string description)
		{
			if (!rows.Add(key))
				throw new PlanException($"scripts/VERSIONS.txt contains duplicate {description} rows");
		}

		private static void RequireConsistent(
			IReadOnlyList<NuGetVersion> versions,
			NuGetVersion expected,
			string description)
		{
			if (versions.Count == 0 ||
				versions.Any(version => !VersionComparer.VersionRelease.Equals(version, expected)))
			{
				throw new PlanException($"scripts/VERSIONS.txt has inconsistent {description} versions");
			}
		}
	}
}
