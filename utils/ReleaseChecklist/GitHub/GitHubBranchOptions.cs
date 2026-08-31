using ReleaseChecklist.Core;

namespace ReleaseChecklist.GitHub;

/// <summary>Configures a GitHub branch step.</summary>
public sealed record GitHubBranchOptions
{
	/// <summary>Gets the stable step identifier.</summary>
	/// <value>The identifier used in reports.</value>
	public required string Id { get; init; }

	/// <summary>Gets the human-readable step title.</summary>
	/// <value>The title shown in reports.</value>
	public required string Title { get; init; }

	/// <summary>Gets the GitHub client.</summary>
	/// <value>The client used for reads and writes.</value>
	public required IGitHubRepositoryClient Client { get; init; }

	/// <summary>Gets the target repository.</summary>
	/// <value>The repository identity.</value>
	public required GitHubRepositoryIdentity Repository { get; init; }

	/// <summary>Gets the short branch name.</summary>
	/// <value>The branch name without a <c>refs/heads/</c> prefix.</value>
	public required string Branch { get; init; }

	/// <summary>Gets the exact desired target commit.</summary>
	/// <value>The lowercase 40-character commit SHA.</value>
	public required string ExpectedSha { get; init; }

	/// <summary>Gets an optional policy for accepting an existing branch target.</summary>
	/// <value>A callback that validates an existing SHA, or <see langword="null" /> to require <see cref="ExpectedSha" />.</value>
	public Func<string, bool>? AcceptExisting { get; init; }

	/// <summary>Gets the optional applicability condition.</summary>
	/// <value>The condition, or <see langword="null" /> to always include the step.</value>
	public IChecklistCondition? When { get; init; }
}
