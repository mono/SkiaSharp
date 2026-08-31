using NuGet.Versioning;
using ReleaseChecklist.Git;

namespace SkiaSharp.ReleaseChecklist;

/// <summary>Reads, validates, and edits SkiaSharp release version files.</summary>
public static class VersionFiles
{
	/// <summary>The repository-relative Azure template variables path.</summary>
	public const string VariablesPath = "scripts/azure-templates-variables.yml";
	/// <summary>The repository-relative package versions path.</summary>
	public const string VersionsPath = "scripts/VERSIONS.txt";

	/// <summary>Parses and cross-validates release version file content.</summary>
	/// <param name="variables">The Azure template variables content.</param>
	/// <param name="versions">The package versions content.</param>
	/// <returns>The validated version state.</returns>
	/// <exception cref="ReleasePolicyException">The content is missing, malformed, or inconsistent.</exception>
	public static VersionState Parse(string variables, string versions)
	{
		var skia = Assignment(variables, "SKIASHARP_VERSION");
		var label = Assignment(variables, "PREVIEW_LABEL");
		if (label != "stable" &&
			(label.Split('.') is not [var channel, var iteration] ||
				channel is not ("preview" or "rc") ||
				!int.TryParse(iteration, out var number) ||
				number < 0))
		{
			throw new ReleasePolicyException($"Invalid PREVIEW_LABEL '{label}'.");
		}
		var skiaRows = PackageRows(versions, "SkiaSharp");
		var harfBuzzRows = PackageRows(versions, "HarfBuzzSharp");
		if (skiaRows.Count == 0 || harfBuzzRows.Count == 0)
			throw new ReleasePolicyException("VERSIONS.txt has no root SkiaSharp or HarfBuzzSharp package row.");
		var rootSkia = skiaRows.Single(row => row.Package == "SkiaSharp").Version;
		var rootHarfBuzz = harfBuzzRows.Single(row => row.Package == "HarfBuzzSharp").Version;
		var skiaFile = FileVersion(versions, "SkiaSharp");
		var harfBuzzFile = FileVersion(versions, "HarfBuzzSharp");
		if (rootSkia != skia || skiaRows.Any(row => row.Version != skia) ||
			harfBuzzRows.Any(row => row.Version != rootHarfBuzz) ||
			!VersionComparer.VersionRelease.Equals(
				NuGetVersion.Parse(skiaFile), NuGetVersion.Parse(rootSkia)) ||
			!VersionComparer.VersionRelease.Equals(
				NuGetVersion.Parse(harfBuzzFile), NuGetVersion.Parse(rootHarfBuzz)))
		{
			throw new ReleasePolicyException("Version files contain inconsistent package family versions.");
		}
		_ = NuGetVersion.Parse(skia);
		_ = NuGetVersion.Parse(rootHarfBuzz);
		return new VersionState(skia, rootHarfBuzz, label);
	}

	/// <summary>Asynchronously reads version state from a Git revision.</summary>
	/// <param name="repository">The Git repository.</param>
	/// <param name="reference">The revision containing the version files.</param>
	/// <param name="cancellationToken">A token that cancels the read.</param>
	/// <returns>The validated version state.</returns>
	public static async Task<VersionState> ReadAsync(
		GitRepository repository,
		string reference,
		CancellationToken cancellationToken)
	{
		var variables = await repository.ReadRefFileAsync(
			reference, VariablesPath, cancellationToken).ConfigureAwait(false);
		var versions = await repository.ReadRefFileAsync(
			reference, VersionsPath, cancellationToken).ConfigureAwait(false);
		return Parse(variables, versions);
	}

	/// <summary>Asynchronously configures the exact version and label for a release branch.</summary>
	/// <param name="repository">The isolated Git worktree to edit.</param>
	/// <param name="identity">The target release identity.</param>
	/// <param name="cancellationToken">A token that cancels the edit.</param>
	/// <returns>A task that represents the edit.</returns>
	public static async ValueTask ConfigureReleaseAsync(
		GitRepository repository,
		ReleaseIdentity identity,
		CancellationToken cancellationToken)
	{
		var variables = await repository.ReadWorktreeFileAsync(
			VariablesPath, cancellationToken).ConfigureAwait(false);
		var versions = await repository.ReadWorktreeFileAsync(
			VersionsPath, cancellationToken).ConfigureAwait(false);
		var updatedVariables = ReplaceAssignment(
			ReplaceAssignment(variables, "SKIASHARP_VERSION", identity.Numeric, quote: false),
			"PREVIEW_LABEL",
			identity.Label,
			quote: true);
		var updatedVersions = ReplaceFamily(versions, "SkiaSharp", identity.Numeric);
		var updated = Parse(updatedVariables, updatedVersions);
		if (updated.SkiaSharpVersion != identity.Numeric ||
			updated.Label != identity.Label)
			throw new ReleasePolicyException("Release version edit did not produce the requested state.");
		await WriteIfChangedAsync(
			repository,
			VariablesPath,
			variables,
			updatedVariables,
			cancellationToken).ConfigureAwait(false);
		await WriteIfChangedAsync(
			repository,
			VersionsPath,
			versions,
			updatedVersions,
			cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Asynchronously configures a maintenance branch for the next preview.</summary>
	/// <param name="repository">The isolated Git worktree to edit.</param>
	/// <param name="nextSkia">The next SkiaSharp numeric version.</param>
	/// <param name="nextHarfBuzz">The next HarfBuzzSharp numeric version.</param>
	/// <param name="cancellationToken">A token that cancels the edit.</param>
	/// <returns>A task that represents the edit.</returns>
	public static async ValueTask ConfigureNextPreviewAsync(
		GitRepository repository,
		string nextSkia,
		string nextHarfBuzz,
		CancellationToken cancellationToken)
	{
		var variables = await repository.ReadWorktreeFileAsync(
			VariablesPath, cancellationToken).ConfigureAwait(false);
		var versions = await repository.ReadWorktreeFileAsync(
			VersionsPath, cancellationToken).ConfigureAwait(false);
		var updatedVariables = ReplaceAssignment(
			ReplaceAssignment(variables, "SKIASHARP_VERSION", nextSkia, quote: false),
			"PREVIEW_LABEL",
			"preview.0",
			quote: true);
		var updatedVersions = ReplaceFamily(
			ReplaceFamily(versions, "SkiaSharp", nextSkia),
			"HarfBuzzSharp",
			nextHarfBuzz);
		var updated = Parse(updatedVariables, updatedVersions);
		if (updated.SkiaSharpVersion != nextSkia ||
			updated.HarfBuzzSharpVersion != nextHarfBuzz ||
			updated.Label != "preview.0")
			throw new ReleasePolicyException("Next-preview edit did not produce the requested state.");
		await WriteIfChangedAsync(
			repository,
			VariablesPath,
			variables,
			updatedVariables,
			cancellationToken).ConfigureAwait(false);
		await WriteIfChangedAsync(
			repository,
			VersionsPath,
			versions,
			updatedVersions,
			cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Calculates the next SkiaSharp patch and HarfBuzzSharp revision.</summary>
	/// <param name="stable">The stable non-hotfix identity that was released.</param>
	/// <param name="state">The released version state.</param>
	/// <returns>The next SkiaSharp and HarfBuzzSharp numeric versions.</returns>
	/// <exception cref="ReleasePolicyException"><paramref name="stable" /> is not a three-part stable release.</exception>
	public static (string SkiaSharp, string HarfBuzzSharp) NextVersions(
		ReleaseIdentity stable,
		VersionState state)
	{
		if (!stable.IsStable || stable.IsHotfix || stable.Version.Patch == int.MaxValue)
			throw new ReleasePolicyException("Next preview requires a three-part stable release.");
		var skia = $"{stable.Version.Major}.{stable.Version.Minor}.{stable.Version.Patch + 1}";
		var harfBuzz = NuGetVersion.Parse(state.HarfBuzzSharpVersion);
		var revision = harfBuzz.Version.Revision;
		var nextHarfBuzz = revision < 0
			? $"{harfBuzz.Version.ToString(3)}.1"
			: new Version(
				harfBuzz.Major,
				harfBuzz.Minor,
				harfBuzz.Patch,
				checked(revision + 1)).ToString(4);
		return (skia, nextHarfBuzz);
	}

	private static async Task WriteIfChangedAsync(
		GitRepository repository,
		string path,
		string before,
		string after,
		CancellationToken cancellationToken)
	{
		if (before != after)
			await repository.WriteWorktreeFileAsync(path, after, cancellationToken).ConfigureAwait(false);
	}

	private static string Assignment(string text, string name)
	{
		var matches = Lines(text)
			.Select(line => TryAssignment(line, name))
			.Where(static value => value is not null)
			.ToArray();
		return matches.Length == 1
			? matches[0]!
			: throw new ReleasePolicyException($"Expected exactly one {name} assignment.");
	}

	private static string? TryAssignment(string line, string name)
	{
		var trimmed = line.TrimStart();
		if (!trimmed.StartsWith($"{name}:", StringComparison.Ordinal))
			return null;
		var value = trimmed[(name.Length + 1)..].Trim();
		if (value.Length >= 2 && value[0] is '\'' or '"' && value[^1] == value[0])
			value = value[1..^1];
		return value;
	}

	private static string ReplaceAssignment(
		string text,
		string name,
		string value,
		bool quote)
	{
		var replacements = 0;
		var lines = LinesWithEndings(text)
			.Select(line =>
			{
				if (TryAssignment(line.Content, name) is null)
					return line.Content + line.Ending;
				replacements++;
				var colon = line.Content.IndexOf(':');
				var rendered = quote ? $"'{value}'" : value;
				return line.Content[..(colon + 1)] + " " + rendered + line.Ending;
			});
		var result = string.Concat(lines);
		return replacements == 1
			? result
			: throw new ReleasePolicyException($"Expected exactly one {name} assignment.");
	}

	private static string ReplaceFamily(string text, string family, string version)
	{
		var rootFileSeen = false;
		var rows = 0;
		var result = string.Concat(LinesWithEndings(text).Select(line =>
		{
			var columns = line.Content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
			var replace =
				columns.Length >= 3 &&
				(columns[1] == "nuget" && columns[0].StartsWith(family, StringComparison.Ordinal) ||
				 columns[1] == "file" && columns[0] == family);
			if (!replace)
				return line.Content + line.Ending;
			rows++;
			rootFileSeen |= columns[1] == "file" && columns[0] == family;
			var rendered = columns[1] == "file" && family == "SkiaSharp" &&
				version.Split('.').Length == 3
					? $"{version}.0"
					: version;
			var last = line.Content.LastIndexOf(columns[^1], StringComparison.Ordinal);
			return line.Content[..last] + rendered + line.Content[(last + columns[^1].Length)..] + line.Ending;
		}));
		if (rows == 0 || family == "SkiaSharp" && !rootFileSeen)
			throw new ReleasePolicyException($"No replaceable {family} rows were found.");
		return result;
	}

	private static IReadOnlyList<(string Package, string Version)> PackageRows(
		string text,
		string family) =>
		Lines(text)
			.Select(static line => line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
			.Where(columns =>
				columns.Length >= 3 &&
				columns[1] == "nuget" &&
				columns[0].StartsWith(family, StringComparison.Ordinal))
			.Select(static columns => (columns[0], columns[2]))
			.ToArray();

	private static string FileVersion(string text, string family)
	{
		var matches = Lines(text)
			.Select(static line => line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
			.Where(columns =>
				columns.Length >= 3 &&
				columns[0] == family &&
				columns[1] == "file")
			.Select(static columns => columns[2])
			.ToArray();
		return matches.Length == 1
			? matches[0]
			: throw new ReleasePolicyException($"Expected exactly one {family} file row.");
	}

	private static IEnumerable<string> Lines(string text)
	{
		using var reader = new StringReader(text);
		while (reader.ReadLine() is { } line)
			yield return line;
	}

	private static IEnumerable<(string Content, string Ending)> LinesWithEndings(string text)
	{
		var position = 0;
		while (position < text.Length)
		{
			var newline = text.IndexOf('\n', position);
			if (newline < 0)
			{
				yield return (text[position..], "");
				yield break;
			}
			var hasCarriageReturn = newline > position && text[newline - 1] == '\r';
			yield return (
				text[position..(hasCarriageReturn ? newline - 1 : newline)],
				hasCarriageReturn ? "\r\n" : "\n");
			position = newline + 1;
		}
	}
}
