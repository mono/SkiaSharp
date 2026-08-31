using ReleaseChecklist.Core;

namespace ReleaseChecklist.GitHub;

/// <summary>Creates a missing pull request and accepts a concurrent exact creation.</summary>
public sealed class CreateGitHubPullRequest : IChecklistAction
{
	private readonly IGitHubRepositoryClient client;
	private readonly GitHubRepositoryIdentity repository;
	private readonly string head;
	private readonly string @base;
	private readonly string title;
	private readonly string body;

	/// <summary>Initializes a new instance of the <see cref="CreateGitHubPullRequest" /> class.</summary>
	/// <param name="options">The pull request configuration.</param>
	public CreateGitHubPullRequest(GitHubPullRequestOptions options)
	{
		client = options.Client;
		repository = options.Repository;
		head = options.Head;
		@base = options.Base;
		title = options.PullRequestTitle;
		body = options.Body;
	}

	/// <inheritdoc />
	public async ValueTask ExecuteAsync(CancellationToken cancellationToken)
	{
		try
		{
			_ = await client.CreatePullRequestAsync(
				repository, head, @base, title, body, cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			var raced = await client.FindPullRequestsAsync(
				repository, head, @base, cancellationToken).ConfigureAwait(false);
			if (raced.Count(static pr => pr.Open || pr.Merged) == 1)
				return;
			throw;
		}
	}
}
