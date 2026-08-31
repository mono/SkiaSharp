using ReleaseChecklist.Core;

namespace ReleaseChecklist.Git;

/// <summary>Configures a step that creates or switches to a local Git branch.</summary>
public sealed record GitBranchOptions
{
	/// <summary>Gets the stable step identifier.</summary>
	/// <value>The identifier used in reports.</value>
	public required string Id { get; init; }

	/// <summary>Gets the human-readable step title.</summary>
	/// <value>The title shown in reports.</value>
	public required string Title { get; init; }

	/// <summary>Gets the repository whose worktree is changed.</summary>
	/// <value>The Git repository.</value>
	public required GitRepository Repository { get; init; }

	/// <summary>Gets the local branch name.</summary>
	/// <value>The short branch name.</value>
	public required string Branch { get; init; }

	/// <summary>Gets the start point used when the branch does not exist.</summary>
	/// <value>A resolvable Git revision.</value>
	public required string StartPoint { get; init; }

	/// <summary>Gets the optional applicability condition.</summary>
	/// <value>The condition, or <see langword="null" /> to always include the step.</value>
	public IChecklistCondition? When { get; init; }
}
