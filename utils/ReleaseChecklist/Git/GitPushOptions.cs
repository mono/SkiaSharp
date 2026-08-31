using ReleaseChecklist.Core;

namespace ReleaseChecklist.Git;

/// <summary>Configures a step that pushes a local branch when it is ahead of its remote branch.</summary>
public sealed record GitPushOptions
{
	/// <summary>Gets the stable step identifier.</summary>
	/// <value>The identifier used in reports.</value>
	public required string Id { get; init; }

	/// <summary>Gets the human-readable step title.</summary>
	/// <value>The title shown in reports.</value>
	public required string Title { get; init; }

	/// <summary>Gets the repository containing the local branch.</summary>
	/// <value>The Git repository.</value>
	public required GitRepository Repository { get; init; }

	/// <summary>Gets the branch to push.</summary>
	/// <value>The short branch name.</value>
	public required string Branch { get; init; }

	/// <summary>Gets the optional applicability condition.</summary>
	/// <value>The condition, or <see langword="null" /> to always include the step.</value>
	public IChecklistCondition? When { get; init; }
}
