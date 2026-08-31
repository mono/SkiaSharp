using ReleaseChecklist.Core;

namespace ReleaseChecklist.Git;

/// <summary>Configures a step that commits selected worktree paths when they differ.</summary>
public sealed record GitCommitOptions
{
	/// <summary>Gets the stable step identifier.</summary>
	/// <value>The identifier used in reports.</value>
	public required string Id { get; init; }

	/// <summary>Gets the human-readable step title.</summary>
	/// <value>The title shown in reports.</value>
	public required string Title { get; init; }

	/// <summary>Gets the repository containing the paths.</summary>
	/// <value>The Git repository.</value>
	public required GitRepository Repository { get; init; }

	/// <summary>Gets the repository-relative paths to commit.</summary>
	/// <value>The selected paths.</value>
	public required IReadOnlyList<string> Paths { get; init; }

	/// <summary>Gets the commit message.</summary>
	/// <value>The commit message.</value>
	public required string Message { get; init; }

	/// <summary>Gets the optional applicability condition.</summary>
	/// <value>The condition, or <see langword="null" /> to always include the step.</value>
	public IChecklistCondition? When { get; init; }
}
