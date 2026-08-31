namespace ReleaseChecklist.GitHub;

/// <summary>Defines the GitHub repository operations used by checklist primitives.</summary>
public interface IGitHubRepositoryClient
{
	/// <summary>Gets the exact target of a branch.</summary>
	/// <param name="repository">The target repository.</param>
	/// <param name="branch">The short branch name.</param>
	/// <param name="cancellationToken">A token that cancels the request.</param>
	/// <returns>The target SHA, or <see langword="null" /> if the branch is absent.</returns>
	Task<string?> GetBranchShaAsync(
		GitHubRepositoryIdentity repository,
		string branch,
		CancellationToken cancellationToken);

	/// <summary>Creates a branch at an exact commit.</summary>
	/// <param name="repository">The target repository.</param>
	/// <param name="branch">The short branch name.</param>
	/// <param name="sha">The exact target commit SHA.</param>
	/// <param name="cancellationToken">A token that cancels the request.</param>
	/// <returns>A task that represents the request.</returns>
	Task CreateBranchAsync(
		GitHubRepositoryIdentity repository,
		string branch,
		string sha,
		CancellationToken cancellationToken);

	/// <summary>Finds pull requests with an exact head and base branch.</summary>
	/// <param name="repository">The target repository.</param>
	/// <param name="head">The short source branch name.</param>
	/// <param name="base">The short target branch name.</param>
	/// <param name="cancellationToken">A token that cancels the request.</param>
	/// <returns>The matching open and merged pull requests.</returns>
	Task<IReadOnlyList<GitHubPullRequestState>> FindPullRequestsAsync(
		GitHubRepositoryIdentity repository,
		string head,
		string @base,
		CancellationToken cancellationToken);

	/// <summary>Creates a pull request.</summary>
	/// <param name="repository">The target repository.</param>
	/// <param name="head">The short source branch name.</param>
	/// <param name="base">The short target branch name.</param>
	/// <param name="title">The pull request title.</param>
	/// <param name="body">The pull request body.</param>
	/// <param name="cancellationToken">A token that cancels the request.</param>
	/// <returns>The created pull request.</returns>
	Task<GitHubPullRequestState> CreatePullRequestAsync(
		GitHubRepositoryIdentity repository,
		string head,
		string @base,
		string title,
		string body,
		CancellationToken cancellationToken);
}
