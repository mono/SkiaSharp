namespace SkiaSharp.ReleaseChecklist;

/// <summary>Specifies the publication channel encoded by a release identity.</summary>
public enum ReleaseChannel
{
	/// <summary>The stable channel without a prerelease label.</summary>
	Stable,
	/// <summary>The preview prerelease channel.</summary>
	Preview,
	/// <summary>The release-candidate prerelease channel.</summary>
	ReleaseCandidate,
}
