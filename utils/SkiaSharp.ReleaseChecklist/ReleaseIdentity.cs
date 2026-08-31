using NuGet.Versioning;

namespace SkiaSharp.ReleaseChecklist;

/// <summary>Represents a validated SkiaSharp release identity without a public build revision.</summary>
public sealed record ReleaseIdentity
{
	private ReleaseIdentity(
		NuGetVersion version,
		int componentCount,
		string raw,
		ReleaseChannel channel,
		int? iteration)
	{
		Version = version;
		ComponentCount = componentCount;
		Raw = raw;
		Channel = channel;
		Iteration = iteration;
	}

	/// <summary>Gets the parsed NuGet version.</summary>
	/// <value>The normalized release version.</value>
	public NuGetVersion Version { get; }
	/// <summary>Gets the number of numeric version components.</summary>
	/// <value>Three for a normal release or four for a hotfix.</value>
	public int ComponentCount { get; }
	/// <summary>Gets the normalized textual identity.</summary>
	/// <value>The exact identity supplied to or produced by the parser.</value>
	public string Raw { get; }
	/// <summary>Gets the release channel.</summary>
	/// <value>The stable, preview, or release-candidate channel.</value>
	public ReleaseChannel Channel { get; }
	/// <summary>Gets the prerelease iteration.</summary>
	/// <value>The positive iteration number, or <see langword="null" /> for stable.</value>
	public int? Iteration { get; }
	/// <summary>Gets a value indicating whether the identity has four numeric components.</summary>
	/// <value><see langword="true" /> for a hotfix; otherwise, <see langword="false" />.</value>
	public bool IsHotfix => ComponentCount == 4;
	/// <summary>Gets a value indicating whether the identity is stable.</summary>
	/// <value><see langword="true" /> for stable; otherwise, <see langword="false" />.</value>
	public bool IsStable => Channel == ReleaseChannel.Stable;
	/// <summary>Gets the numeric version without a prerelease label.</summary>
	/// <value>The three- or four-part numeric version.</value>
	public string Numeric => Version.Version.ToString(ComponentCount);
	/// <summary>Gets the major-minor release line.</summary>
	/// <value>The <c>X.Y</c> release line.</value>
	public string Line => $"{Version.Major}.{Version.Minor}";
	/// <summary>Gets the maintenance branch name.</summary>
	/// <value>The <c>release/X.Y.x</c> branch.</value>
	public string MaintenanceBranch => $"release/{Line}.x";
	/// <summary>Gets the exact release branch name.</summary>
	/// <value>The <c>release/{identity}</c> branch.</value>
	public string ReleaseBranch => $"release/{Raw}";
	/// <summary>Gets the value stored in <c>PREVIEW_LABEL</c>.</summary>
	/// <value><c>preview.N</c>, <c>rc.N</c>, or <c>stable</c>.</value>
	public string Label => Channel switch
	{
		ReleaseChannel.Stable => "stable",
		ReleaseChannel.Preview => $"preview.{Iteration}",
		ReleaseChannel.ReleaseCandidate => $"rc.{Iteration}",
		_ => throw new InvalidOperationException("Unknown release channel."),
	};

	/// <summary>Parses and validates an exact release identity.</summary>
	/// <param name="value">The identity in <c>X.Y.Z[.F][-preview.N|-rc.N]</c> form.</param>
	/// <returns>The parsed identity.</returns>
	/// <exception cref="ReleasePolicyException"><paramref name="value" /> is not canonical or supported.</exception>
	public static ReleaseIdentity Parse(string value)
	{
		if (!TryParse(value, out var identity))
			throw new ReleasePolicyException(
				$"Invalid release '{value}'; expected X.Y.Z[.F][-preview.N|-rc.N] with N greater than zero.");
		return identity;
	}

	/// <summary>Attempts to parse and validate an exact release identity.</summary>
	/// <param name="value">The identity to parse.</param>
	/// <param name="identity">When this method returns, contains the parsed identity if successful.</param>
	/// <returns><see langword="true" /> if parsing succeeded; otherwise, <see langword="false" />.</returns>
	public static bool TryParse(string? value, out ReleaseIdentity identity)
	{
		identity = null!;
		if (string.IsNullOrWhiteSpace(value) ||
			!string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
			!NuGetVersion.TryParse(value, out var version) ||
			version.HasMetadata ||
			!string.Equals(version.ToNormalizedString(), value, StringComparison.Ordinal))
		{
			return false;
		}

		var numeric = value.Split(['-', '+'], 2)[0];
		var parts = numeric.Split('.');
		if (parts.Length is not (3 or 4) ||
			parts.Any(static part => part.Length == 0 || part.Any(static c => !char.IsAsciiDigit(c))))
		{
			return false;
		}

		var labels = version.ReleaseLabels.ToArray();
		ReleaseChannel channel;
		int? iteration;
		if (labels.Length == 0)
		{
			channel = ReleaseChannel.Stable;
			iteration = null;
		}
		else
		{
			if (labels.Length != 2 ||
				labels[0] is not ("preview" or "rc") ||
				!int.TryParse(labels[1], out var parsedIteration) ||
				parsedIteration <= 0)
			{
				return false;
			}
			channel = labels[0] == "preview"
				? ReleaseChannel.Preview
				: ReleaseChannel.ReleaseCandidate;
			iteration = parsedIteration;
		}

		identity = new ReleaseIdentity(version, parts.Length, value, channel, iteration);
		return true;
	}
}
