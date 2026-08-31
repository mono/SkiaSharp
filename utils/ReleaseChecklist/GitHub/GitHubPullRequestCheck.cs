using ReleaseChecklist.Core;

namespace ReleaseChecklist.GitHub;

/// <summary>Checks for one open or merged pull request with an exact head and base.</summary>
public sealed class GitHubPullRequestCheck : IChecklistCheck
{
	private readonly IGitHubRepositoryClient client;
	private readonly GitHubRepositoryIdentity repository;
	private readonly string head;
	private readonly string @base;

	/// <summary>Initializes a new instance of the <see cref="GitHubPullRequestCheck" /> class.</summary>
	/// <param name="options">The pull request configuration.</param>
	public GitHubPullRequestCheck(GitHubPullRequestOptions options)
	{
		client = options.Client;
		repository = options.Repository;
		head = options.Head;
		@base = options.Base;
	}

	/// <inheritdoc />
	public async ValueTask<CheckResult> EvaluateAsync(CancellationToken cancellationToken)
	{
		var matches = await client.FindPullRequestsAsync(
			repository, head, @base, cancellationToken).ConfigureAwait(false);
		var current = matches.Where(static pr => pr.Open || pr.Merged).ToArray();
		var observation = new ObservationBuilder()
			.Add("repository", repository.ToString())
			.Add("head", head)
			.Add("base", @base)
			.Add("matches", matches.Count)
			.Add("current-matches", current.Length)
			.Add("numbers", string.Join(',', matches.Select(static pr => pr.Number)))
			.Build();
		if (current.Length == 0)
			return CheckResult.NotDone($"No open pull request from '{head}' to '{@base}'.", observation);
		if (current.Length > 1)
			return CheckResult.Blocked($"Multiple pull requests from '{head}' to '{@base}'.", observation);
		var state = current[0];
		return CheckResult.Done(
			$"Pull request #{state.Number} is {(state.Merged ? "merged" : "open")}.",
			observation);
	}
}
