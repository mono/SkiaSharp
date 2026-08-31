using ReleaseChecklist.Core;

namespace ReleaseChecklist.GitHub;

/// <summary>Configures a GitHub pull request step.</summary>
public sealed record GitHubPullRequestOptions
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

	/// <summary>Gets the source branch.</summary>
	/// <value>The short head branch name.</value>
	public required string Head { get; init; }

	/// <summary>Gets the target branch.</summary>
	/// <value>The short base branch name.</value>
	public required string Base { get; init; }

	/// <summary>Gets the pull request title.</summary>
	/// <value>The title used when creating a pull request.</value>
	public required string PullRequestTitle { get; init; }

	/// <summary>Gets the pull request body.</summary>
	/// <value>The body used when creating a pull request.</value>
	public required string Body { get; init; }

	/// <summary>Gets the optional applicability condition.</summary>
	/// <value>The condition, or <see langword="null" /> to always include the step.</value>
	public IChecklistCondition? When { get; init; }
}
