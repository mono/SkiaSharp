using ReleaseChecklist.Core;

namespace ReleaseChecklist.Git;

/// <summary>Configures a remote Git branch step.</summary>
public sealed record GitRemoteBranchOptions
{
	/// <summary>Gets the stable step identifier.</summary>
	/// <value>The identifier used in reports.</value>
	public required string Id { get; init; }

	/// <summary>Gets the human-readable step title.</summary>
	/// <value>The title shown in reports.</value>
	public required string Title { get; init; }

	/// <summary>Gets the repository to inspect and mutate.</summary>
	/// <value>The Git repository.</value>
	public required GitRepository Repository { get; init; }

	/// <summary>Gets the short branch name.</summary>
	/// <value>The branch name without a <c>refs/heads/</c> prefix.</value>
	public required string Branch { get; init; }

	/// <summary>Gets the commit used to create a missing branch.</summary>
	/// <value>A frozen commit SHA or other resolvable Git commit.</value>
	public required string StartPoint { get; init; }

	/// <summary>Gets the expected target used by the default existing-branch check.</summary>
	/// <value>The expected branch target SHA.</value>
	public required string ExpectedTarget { get; init; }

	/// <summary>Gets an optional policy for accepting an existing branch.</summary>
	/// <value>A callback that validates existing branch state, or <see langword="null" /> to require <see cref="ExpectedTarget" />.</value>
	public Func<GitRemoteBranchState, CancellationToken, ValueTask<bool>>? AcceptExisting { get; init; }

	/// <summary>Gets an optional callback that changes files before the branch commit is created.</summary>
	/// <value>A callback that edits the isolated worktree, or <see langword="null" /> to create the branch without a new commit.</value>
	public Func<GitRepository, CancellationToken, ValueTask>? ConfigureCommit { get; init; }

	/// <summary>Gets the commit message used when the configuration callback changes files.</summary>
	/// <value>The commit message, or <see langword="null" /> to use a generated message.</value>
	public string? CommitMessage { get; init; }

	/// <summary>Gets the optional applicability condition.</summary>
	/// <value>The condition, or <see langword="null" /> to always include the step.</value>
	public IChecklistCondition? When { get; init; }
}
