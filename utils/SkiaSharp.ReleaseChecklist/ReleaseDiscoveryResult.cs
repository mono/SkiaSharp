namespace SkiaSharp.ReleaseChecklist;

/// <summary>Contains the immutable release inputs discovered before composing Prepare.</summary>
/// <param name="Identity">The exact release identity.</param>
/// <param name="SourceKind">The source branch shape.</param>
/// <param name="SourceBranch">The normalized source branch name.</param>
/// <param name="SourceRef">The fully qualified source ref.</param>
/// <param name="SourceSha">The frozen source commit SHA.</param>
/// <param name="SourceVersion">The source version metadata.</param>
/// <param name="MaintenanceBranch">The expected maintenance branch.</param>
/// <param name="MaintenanceExists"><see langword="true" /> if the maintenance branch already exists.</param>
/// <param name="MaintenanceExpectedSha">The existing target or reviewed creation commit for maintenance.</param>
/// <param name="ReleaseBaseRef">The ref selected as release lineage.</param>
/// <param name="ReleaseBaseSha">The frozen release base commit SHA.</param>
/// <param name="ReleaseBranch">The exact SkiaSharp and mono/skia release branch name.</param>
/// <param name="SkiaSha">The frozen <c>externals/skia</c> gitlink SHA.</param>
/// <param name="StableBump">The optional post-stable-release bump.</param>
public sealed record ReleaseDiscoveryResult(
	ReleaseIdentity Identity,
	ReleaseSourceKind SourceKind,
	string SourceBranch,
	string SourceRef,
	string SourceSha,
	VersionState SourceVersion,
	string MaintenanceBranch,
	bool MaintenanceExists,
	string MaintenanceExpectedSha,
	string ReleaseBaseRef,
	string ReleaseBaseSha,
	string ReleaseBranch,
	string SkiaSha,
	StableBump StableBump);
