namespace ReleaseChecklist.Git;

/// <summary>Describes the observed state of a remote Git branch.</summary>
/// <param name="Name">The short branch name.</param>
/// <param name="FullRef">The fully qualified branch ref.</param>
/// <param name="Exists"><see langword="true" /> if the branch exists on the remote.</param>
/// <param name="Sha">The branch target SHA, or <see langword="null" /> when the branch is absent.</param>
public sealed record GitRemoteBranchState(
	string Name,
	string FullRef,
	bool Exists,
	string? Sha);
